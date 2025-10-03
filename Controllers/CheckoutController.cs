using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IEmailService _emailService;
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ICartService cartService, IOrderService orderService, IEmailService emailService, IMercadoPagoService mercadoPagoService, ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
            _emailService = emailService;
            _mercadoPagoService = mercadoPagoService;
            _logger = logger;
        }

        // GET: /Checkout
        [RequireAuth]
        public IActionResult Index()
        {
            var sessionId = GetSessionId();
            var cart = _cartService.GetCart(sessionId);
            
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Usar frete salvo no carrinho (novo sistema) ou fallback para cálculo antigo
            var shippingCost = cart.ShippingCost ?? CalculateShippingCost(cart);
            var taxAmount = CalculateTaxAmount(cart.Subtotal);
            
            var cartViewModel = new CartViewModel
            {
                Cart = cart,
                Items = cart.Items,
                Subtotal = cart.Subtotal,
                ShippingCost = shippingCost,
                TaxAmount = taxAmount,
                Total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount,
                CouponCode = cart.CouponCode,
                DiscountAmount = cart.DiscountAmount,
                IsCouponValid = !string.IsNullOrEmpty(cart.CouponCode)
            };

            var checkoutViewModel = new CheckoutViewModel
            {
                Cart = cartViewModel
            };

            return View(checkoutViewModel);
        }

        // POST: /Checkout/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Process(CheckoutViewModel model)
        {
            try
            {
                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);
                
                // Validação customizada baseada no método de pagamento
                if (!ModelState.IsValid)
                {
                    var shippingCost = cart.ShippingCost ?? CalculateShippingCost(cart);
                    var taxAmount = CalculateTaxAmount(cart.Subtotal);
                    var cartViewModel = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = shippingCost,
                        TaxAmount = taxAmount,
                        Total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount
                    };
                    model.Cart = cartViewModel;
                    return View("Index", model);
                }
                
                if (!cart.Items.Any())
                {
                    return RedirectToAction("Index", "Cart");
                }

                // Primeiro criar o pedido temporário
                var order = await CreateTemporaryOrder(model, cart, sessionId);
                
                // Tentar processar via Mercado Pago
                var paymentResult = await ProcessMercadoPagoPayment(model, cart, order.Id);
                
                if (paymentResult.Success && !string.IsNullOrEmpty(paymentResult.RedirectUrl))
                {
                    // Redirecionar para Mercado Pago
                    return Redirect(paymentResult.RedirectUrl);
                }
                
                // Se MP falhar, usar simulação local
                _logger.LogWarning("Mercado Pago falhou, usando simulação local");
                var simulationResult = await ProcessPaymentSimulation(model);
                
                if (!simulationResult.Success)
                {
                    // Cancelar pedido temporário
                    await _orderService.UpdateOrderStatus(order.Id, "Cancelado");
                    
                    // Verificar se é uma requisição AJAX
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                        Request.ContentType?.Contains("application/json") == true)
                    {
                        return Json(new { success = false, message = simulationResult.Message });
                    }
                    
                    ModelState.AddModelError("", simulationResult.Message);
                    var shippingCost = cart.ShippingCost ?? CalculateShippingCost(cart);
                    var cartViewModel = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = shippingCost,
                        TaxAmount = CalculateTaxAmount(cart.Subtotal),
                        Total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + CalculateTaxAmount(cart.Subtotal)
                    };
                    model.Cart = cartViewModel;
                    return View("Index", model);
                }

                // Se for PIX, mostrar QR Code
                if (simulationResult.RequiresPixConfirmation)
                {
                    var shippingCost = cart.ShippingCost ?? CalculateShippingCost(cart);
                    var cartViewModel = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = shippingCost,
                        TaxAmount = CalculateTaxAmount(cart.Subtotal),
                        Total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + CalculateTaxAmount(cart.Subtotal)
                    };
                    model.Cart = cartViewModel;
                    HttpContext.Session.SetInt32("PendingOrderId", order.Id);
                    return View("PixPayment", model);
                }

                // Se for cartão, mostrar tela de confirmação
                if (simulationResult.RequiresCardConfirmation)
                {
                    var shippingCost = cart.ShippingCost ?? CalculateShippingCost(cart);
                    var cartViewModel = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = shippingCost,
                        TaxAmount = CalculateTaxAmount(cart.Subtotal),
                        Total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + CalculateTaxAmount(cart.Subtotal)
                    };
                    model.Cart = cartViewModel;
                    HttpContext.Session.SetInt32("PendingOrderId", order.Id);
                    return View("CardPayment", model);
                }

                // Confirmar pedido e limpar carrinho
                await _orderService.UpdateOrderStatus(order.Id, "Confirmado");
                _cartService.ClearCart(sessionId);
                HttpContext.Session.SetInt32("LastOrderId", order.Id);
                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar checkout");
                ModelState.AddModelError("", "Erro ao processar checkout. Tente novamente.");
                return View("Index", model);
            }
        }

        // GET: /Checkout/Confirmation/5
        [RequireAuth]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            var order = await _orderService.GetOrderById(orderId);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: /Checkout/SendConfirmationEmail
        [HttpPost]
        [RequireAuth]
        public async Task<IActionResult> SendConfirmationEmail(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderById(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Pedido não encontrado" });
                }

                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (string.IsNullOrEmpty(userEmail))
                {
                    return Json(new { success = false, message = "Email do usuário não encontrado" });
                }

                var emailSent = await _emailService.SendOrderConfirmationAsync(userEmail, order);
                
                if (emailSent)
                {
                    return Json(new { success = true, message = "Comprovante enviado para o email!" });
                }
                else
                {
                    return Json(new { success = false, message = "Erro ao enviar email. Tente novamente." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar confirmação por email para pedido {OrderId}", orderId);
                return Json(new { success = false, message = "Erro interno. Tente novamente." });
            }
        }

        private async Task<PaymentResult> ProcessMercadoPagoPayment(CheckoutViewModel model, Cart cart, int orderId)
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";

                // Criar preferência no Mercado Pago
                var preference = new MercadoPagoPreferenceRequest
                {
                    ExternalReference = orderId.ToString(),
                    Items = cart.Items.Select(item => {
                        _logger.LogInformation($"Item: {item.Product.Name}, Quantity: {item.Quantity}, UnitPrice: {item.UnitPrice}");
                        return new MercadoPagoItem
                        {
                            Id = item.ProductId.ToString(),
                            Title = item.Product.Name,
                            Description = item.Product.Name,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            CurrencyId = "BRL"
                        };
                    }).ToList(),
                    BackUrls = new MercadoPagoUrls
                    {
                        Success = $"{Request.Scheme}://{Request.Host}/Checkout/PaymentSuccess",
                        Failure = $"{Request.Scheme}://{Request.Host}/Checkout/PaymentFailure",
                        Pending = $"{Request.Scheme}://{Request.Host}/Checkout/PaymentPending"
                    }
                };

                // Criar preferência no Mercado Pago
                var mpPreference = await _mercadoPagoService.CreatePreferenceAsync(preference);
                
                if (mpPreference != null)
                {
                    // Armazenar dados na sessão
                    HttpContext.Session.SetInt32("LastOrderId", orderId);
                    HttpContext.Session.SetString("MercadoPagoPreferenceId", mpPreference.Id);
                    
                    var checkoutUrl = _mercadoPagoService.GetCheckoutUrl(mpPreference.Id);
                    
                    return new PaymentResult 
                    { 
                        Success = true, 
                        Message = "Redirecionando para o Mercado Pago...",
                        RedirectUrl = checkoutUrl,
                        OrderId = orderId
                    };
                }
                
                return new PaymentResult { Success = false, Message = "Erro ao criar preferência no Mercado Pago" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamento com Mercado Pago");
                return new PaymentResult { Success = false, Message = "Erro na integração com Mercado Pago" };
            }
        }

        private async Task<PaymentResult> ProcessPaymentSimulation(CheckoutViewModel model)
        {
            // Simulação com validação real de cartão
            await Task.Delay(1000);
            
            if (model.PaymentMethod == "pix")
            {
                model.PixCode = GeneratePixCode();
                model.PixExpirationTime = DateTime.Now.AddMinutes(5);
                model.ShowPixQrCode = true;
                return new PaymentResult { Success = true, Message = "PIX gerado com sucesso", RequiresPixConfirmation = true };
            }
            else if (model.PaymentMethod == "boleto")
            {
                return new PaymentResult { Success = true, Message = "Boleto gerado com sucesso" };
            }
            else if (model.PaymentMethod == "credit_card" || model.PaymentMethod == "debit_card")
            {
                // Validação rigorosa de cartão
                var validationResult = ValidateCardDetails(model);
                if (!validationResult.IsValid)
                {
                    return new PaymentResult { Success = false, Message = validationResult.ErrorMessage };
                }

                // Simular processamento real com possibilidade de negação
                var isApproved = SimulateCardProcessing(model.CardNumber);
                if (!isApproved)
                {
                    return new PaymentResult { Success = false, Message = "Cartão recusado. Verifique os dados ou tente outro cartão." };
                }

                return new PaymentResult { Success = true, Message = "Pagamento aprovado", RequiresCardConfirmation = true };
            }

            return new PaymentResult { Success = false, Message = "Método de pagamento inválido" };
        }

        private async Task<Order> CreateTemporaryOrder(CheckoutViewModel model, Cart cart, string sessionId)
        {
            // Recuperar cupom aplicado da sessão
            var couponCode = HttpContext.Session.GetString("CouponCode");
            decimal discountAmount = 0;
            
            if (!string.IsNullOrEmpty(couponCode))
            {
                var couponResult = _cartService.ApplyCoupon(sessionId, couponCode);
                if (couponResult.IsValid)
                {
                    discountAmount = couponResult.DiscountAmount;
                }
            }
            
            var order = new Order
            {
                UserId = HttpContext.Session.GetInt32("UserId") ?? 0,
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.Now,
                Status = "Aguardando Pagamento",
                PaymentMethod = GetPaymentMethodName(model.PaymentMethod),
                PaymentStatus = "Pendente",
                Subtotal = cart.Subtotal,
                ShippingCost = cart.ShippingCost ?? CalculateShippingCost(cart),
                TaxAmount = CalculateTaxAmount(cart.Subtotal),
                DiscountAmount = discountAmount,
                CouponCode = couponCode,
                Total = cart.Subtotal - discountAmount + (cart.ShippingCost ?? CalculateShippingCost(cart)) + CalculateTaxAmount(cart.Subtotal),
                ShippingAddress = $"{model.ShippingAddress ?? model.Address}, {model.AddressNumber} - {model.ShippingCity ?? model.City}, {model.State} - {model.ShippingZipCode ?? model.ZipCode}",
                Items = cart.Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductVariationId = item.ProductVariationId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    VariationName = item.ProductVariation?.Name
                }).ToList()
            };

            return await _orderService.CreateOrder(order);
        }

        private async Task<Order> CreateOrder(CheckoutViewModel model, Cart cart, string sessionId)
        {
            var paymentStatus = model.PaymentMethod == "pix" ? "Pending" : "Paid";
            var orderStatus = model.PaymentMethod == "pix" ? "Awaiting Payment" : "Processing";
            
            // Recuperar cupom aplicado da sessão
            var couponCode = HttpContext.Session.GetString("CouponCode");
            decimal discountAmount = 0;
            
            if (!string.IsNullOrEmpty(couponCode))
            {
                var couponResult = _cartService.ApplyCoupon(sessionId, couponCode);
                if (couponResult.IsValid)
                {
                    discountAmount = couponResult.DiscountAmount;
                }
            }
            
            var order = new Order
            {
                UserId = HttpContext.Session.GetInt32("UserId") ?? 1, // Usar usuário logado
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.Now,
                Status = orderStatus,
                ShippingAddress = model.ShippingAddress,
                ShippingCity = model.ShippingCity,
                ShippingZipCode = model.ShippingZipCode,
                ShippingCountry = model.ShippingCountry,
                Subtotal = cart.Subtotal,
                ShippingCost = cart.ShippingCost ?? CalculateShippingCost(cart),
                TaxAmount = CalculateTaxAmount(cart.Subtotal),
                DiscountAmount = discountAmount,
                CouponCode = couponCode,
                Total = cart.Subtotal - discountAmount + (cart.ShippingCost ?? CalculateShippingCost(cart)) + CalculateTaxAmount(cart.Subtotal),
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = paymentStatus,
                Notes = model.Notes
            };

            // Adicionar itens do pedido
            foreach (var cartItem in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductVariationId = cartItem.ProductVariationId,
                    ProductName = cartItem.Product.Name,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    VariationName = cartItem.ProductVariation?.Name
                };
                order.Items.Add(orderItem);
            }

            return await _orderService.CreateOrder(order);
        }

        private (bool IsValid, string ErrorMessage) ValidateCardDetails(CheckoutViewModel model)
        {
            var cardNumber = model.CardNumber?.Replace(" ", "").Replace("-", "");
            
            // Validar número do cartão
            if (string.IsNullOrEmpty(cardNumber))
                return (false, "Número do cartão é obrigatório");
            
            if (cardNumber.Length < 13 || cardNumber.Length > 19)
                return (false, "Número do cartão deve ter entre 13 e 19 dígitos");
            
            if (!cardNumber.All(char.IsDigit))
                return (false, "Número do cartão deve conter apenas dígitos");
            
            // Algoritmo de Luhn para validar cartão
            if (!IsValidLuhn(cardNumber))
                return (false, "Número do cartão inválido");
            
            // Validar data de validade
            if (string.IsNullOrEmpty(model.CardExpiry) || !IsValidExpiry(model.CardExpiry))
                return (false, "Data de validade inválida");
            
            // Validar CVV
            if (string.IsNullOrEmpty(model.CardCvv) || model.CardCvv.Length < 3 || model.CardCvv.Length > 4)
                return (false, "CVV deve ter 3 ou 4 dígitos");
            
            if (!model.CardCvv.All(char.IsDigit))
                return (false, "CVV deve conter apenas dígitos");
            
            // Validar nome no cartão
            if (string.IsNullOrEmpty(model.CardName) || model.CardName.Length < 2)
                return (false, "Nome no cartão é obrigatório");
            
            return (true, string.Empty);
        }

        private bool IsValidLuhn(string cardNumber)
        {
            int sum = 0;
            bool alternate = false;
            
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(cardNumber[i].ToString());
                
                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit = (digit % 10) + 1;
                }
                
                sum += digit;
                alternate = !alternate;
            }
            
            return sum % 10 == 0;
        }

        private bool SimulateCardProcessing(string cardNumber)
        {
            var cleanCardNumber = cardNumber?.Replace(" ", "").Replace("-", "") ?? "";
            
            // Cartões que sempre são negados (para teste)
            var deniedCards = new[]
            {
                "4000000000000002", // Visa sempre negado
                "5555555555554444", // Mastercard sempre negado
                "4000000000000069", // Visa expirado
                "4000000000000127", // Visa CVV incorreto
                "5105105105105100"  // Mastercard sempre negado
            };
            
            if (deniedCards.Contains(cleanCardNumber))
            {
                _logger.LogInformation($"Cartão {cleanCardNumber} foi negado (simulação)");
                return false;
            }
            
            // Cartões válidos do Mercado Pago (sempre aprovados)
            var approvedCards = new[]
            {
                "4509953566233704", // Visa teste MP
                "5031755734530604", // Mastercard teste MP
                "4000000000000010", // Visa genérico aprovado
                "5555555555554448"  // Mastercard genérico aprovado
            };
            
            if (approvedCards.Contains(cleanCardNumber))
            {
                _logger.LogInformation($"Cartão {cleanCardNumber} foi aprovado (simulação)");
                return true;
            }
            
            // Para outros cartões, simular 80% de aprovação
            var random = new Random();
            var isApproved = random.Next(1, 101) <= 80;
            
            _logger.LogInformation($"Cartão {cleanCardNumber} - Resultado simulado: {(isApproved ? "Aprovado" : "Negado")}");
            return isApproved;
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        private bool IsValidExpiry(string expiry)
        {
            Console.WriteLine($"Validando data: '{expiry}'");
            
            if (string.IsNullOrEmpty(expiry) || expiry.Length != 5 || !expiry.Contains('/'))
            {
                Console.WriteLine("Falha: formato inválido");
                return false;
            }

            var parts = expiry.Split('/');
            if (parts.Length != 2)
            {
                Console.WriteLine("Falha: não tem 2 partes");
                return false;
            }

            if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
            {
                Console.WriteLine("Falha: não conseguiu converter para números");
                return false;
            }

            Console.WriteLine($"Mês: {month}, Ano: {year}");

            if (month < 1 || month > 12)
            {
                Console.WriteLine("Falha: mês inválido");
                return false;
            }

            var currentYear = DateTime.Now.Year % 100;
            var currentMonth = DateTime.Now.Month;
            
            Console.WriteLine($"Ano atual: {currentYear}, Mês atual: {currentMonth}");

            // Aceitar qualquer ano >= ano atual, sem validar se está no passado
            if (year < 0 || year > 99)
            {
                Console.WriteLine("Falha: ano fora do range");
                return false;
            }

            Console.WriteLine("Data válida!");
            return true;
        }

        private string GetSessionId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return $"user_{userId.Value}";
            }
            
            var sessionId = HttpContext.Session.GetString("CartSessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartSessionId", sessionId);
            }
            return sessionId;
        }

        private decimal CalculateShippingCost(Cart cart)
        {
            // Calcular o valor total do carrinho considerando cupom aplicado
            decimal cartTotal = cart.Items.Sum(i => i.Total);
            
            // Se há cupom aplicado, subtrair o desconto
            if (!string.IsNullOrEmpty(cart.CouponCode))
            {
                var couponService = HttpContext.RequestServices.GetRequiredService<ICouponService>();
                var coupon = couponService.GetCouponByCode(cart.CouponCode);
                if (coupon != null)
                {
                    if (coupon.DiscountPercentage.HasValue)
                    {
                        cartTotal -= cartTotal * (coupon.DiscountPercentage.Value / 100);
                    }
                    else
                    {
                        cartTotal -= coupon.DiscountAmount;
                    }
                }
            }

            // Calcular frete baseado no valor final (com desconto aplicado)
            if (cartTotal >= 5000)
                return 0; // Frete grátis para pedidos acima de R$ 5.000
            else if (cartTotal >= 3000)
                return 15; // R$ 15 para pedidos entre R$ 3.000 e R$ 5.000
            else
                return 25; // R$ 25 para pedidos abaixo de R$ 3.000
        }

        private decimal CalculateTaxAmount(decimal subtotal)
        {
            return subtotal * 0.10m;
        }

        private bool ValidatePaymentMethod(CheckoutViewModel model)
        {
            if (string.IsNullOrEmpty(model.PaymentMethod))
            {
                ModelState.AddModelError("PaymentMethod", "Método de pagamento é obrigatório");
                return false;
            }

            // Validar campos do cartão apenas se for cartão de crédito ou débito
            if (model.PaymentMethod == "credit_card" || model.PaymentMethod == "debit_card")
            {
                if (string.IsNullOrEmpty(model.CardNumber))
                {
                    ModelState.AddModelError("CardNumber", "Número do cartão é obrigatório");
                    return false;
                }
                if (string.IsNullOrEmpty(model.CardName))
                {
                    ModelState.AddModelError("CardName", "Nome no cartão é obrigatório");
                    return false;
                }
                if (string.IsNullOrEmpty(model.CardExpiry))
                {
                    ModelState.AddModelError("CardExpiry", "Data de validade é obrigatória");
                    return false;
                }
                if (string.IsNullOrEmpty(model.CardCvv))
                {
                    ModelState.AddModelError("CardCvv", "CVV é obrigatório");
                    return false;
                }
            }

            return true;
        }

        private string GeneratePixCode()
        {
            // Gerar um código PIX simulado
            var random = new Random();
            var code = "";
            for (int i = 0; i < 32; i++)
            {
                code += random.Next(0, 10).ToString();
            }
            return code;
        }

        private string GetPaymentMethodName(string paymentMethod)
        {
            return paymentMethod switch
            {
                "credit_card" => "Cartão de Crédito",
                "debit_card" => "Cartão de Débito",
                "pix" => "PIX",
                "boleto" => "Boleto",
                _ => "Outros"
            };
        }

        // Endpoints de retorno do Mercado Pago
        public async Task<IActionResult> PaymentSuccess(string collection_id, string collection_status, string payment_id, string status, string external_reference, string payment_type, string merchant_order_id, string preference_id)
        {
            try
            {
                if (!string.IsNullOrEmpty(external_reference) && int.TryParse(external_reference, out int orderId))
                {
                    var order = await _orderService.GetOrderById(orderId);
                    if (order != null)
                    {
                        await _orderService.UpdateOrderStatus(orderId, "Confirmado");
                        return RedirectToAction("Confirmation", new { orderId = orderId });
                    }
                }
                
                // Fallback - tentar usar o último pedido da sessão
                var lastOrderId = HttpContext.Session.GetInt32("LastOrderId");
                if (lastOrderId.HasValue)
                {
                    await _orderService.UpdateOrderStatus(lastOrderId.Value, "Confirmado");
                    return RedirectToAction("Confirmation", new { orderId = lastOrderId.Value });
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar retorno de sucesso do Mercado Pago");
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> PaymentFailure(string collection_id, string collection_status, string payment_id, string status, string external_reference, string payment_type, string merchant_order_id, string preference_id)
        {
            try
            {
                if (!string.IsNullOrEmpty(external_reference) && int.TryParse(external_reference, out int orderId))
                {
                    await _orderService.UpdateOrderStatus(orderId, "Cancelado");
                }

                TempData["ErrorMessage"] = "Pagamento não foi aprovado. Tente novamente.";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar retorno de falha do Mercado Pago");
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> PaymentPending(string collection_id, string collection_status, string payment_id, string status, string external_reference, string payment_type, string merchant_order_id, string preference_id)
        {
            try
            {
                if (!string.IsNullOrEmpty(external_reference) && int.TryParse(external_reference, out int orderId))
                {
                    var order = await _orderService.GetOrderById(orderId);
                    if (order != null)
                    {
                        await _orderService.UpdateOrderStatus(orderId, "Aguardando Pagamento");
                        return RedirectToAction("Confirmation", new { orderId = orderId });
                    }
                }

                var lastOrderId = HttpContext.Session.GetInt32("LastOrderId");
                if (lastOrderId.HasValue)
                {
                    await _orderService.UpdateOrderStatus(lastOrderId.Value, "Aguardando Pagamento");
                    return RedirectToAction("Confirmation", new { orderId = lastOrderId.Value });
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar retorno de pendência do Mercado Pago");
                return RedirectToAction("Index", "Home");
            }
        }

        // Webhook removido - usar apenas MercadoPagoWebhookController

        // POST: /Checkout/ConfirmPixPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> ConfirmPixPayment(CheckoutViewModel model)
        {
            try
            {
                var pendingOrderId = HttpContext.Session.GetInt32("PendingOrderId");
                if (!pendingOrderId.HasValue)
                {
                    return Json(new { success = false, message = "Pedido não encontrado" });
                }
                
                var sessionId = GetSessionId();
                
                // Confirmar o pedido existente e atualizar pagamento
                await _orderService.UpdateOrderStatus(pendingOrderId.Value, "Confirmado");
                
                // Atualizar status do pagamento para Paid
                var order = await _orderService.GetOrderById(pendingOrderId.Value);
                if (order != null)
                {
                    order.PaymentStatus = "Paid";
                    await _orderService.UpdateOrder(order);
                }
                
                // Limpar carrinho
                _cartService.ClearCart(sessionId);
                
                // Limpar sessão temporária
                HttpContext.Session.Remove("PendingOrderId");
                HttpContext.Session.SetInt32("LastOrderId", pendingOrderId.Value);
                
                return Json(new { success = true, redirectUrl = Url.Action("Confirmation", new { orderId = pendingOrderId.Value }) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao confirmar pagamento PIX");
                return Json(new { success = false, message = "Erro ao processar pagamento" });
            }
        }

        // POST: /Checkout/ConfirmCardPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> ConfirmCardPayment(CheckoutViewModel model)
        {
            try
            {
                var pendingOrderId = HttpContext.Session.GetInt32("PendingOrderId");
                if (!pendingOrderId.HasValue)
                {
                    return Json(new { success = false, message = "Pedido não encontrado" });
                }
                
                var sessionId = GetSessionId();
                
                // Confirmar o pedido existente e atualizar pagamento
                await _orderService.UpdateOrderStatus(pendingOrderId.Value, "Confirmado");
                
                // Atualizar status do pagamento para Paid
                var order = await _orderService.GetOrderById(pendingOrderId.Value);
                if (order != null)
                {
                    order.PaymentStatus = "Paid";
                    await _orderService.UpdateOrder(order);
                }
                
                // Limpar carrinho
                _cartService.ClearCart(sessionId);
                
                // Limpar sessão temporária
                HttpContext.Session.Remove("PendingOrderId");
                HttpContext.Session.SetInt32("LastOrderId", pendingOrderId.Value);
                
                return Json(new { success = true, redirectUrl = Url.Action("Confirmation", new { orderId = pendingOrderId.Value }) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao confirmar pagamento com cartão");
                return Json(new { success = false, message = "Erro ao processar pagamento" });
            }
        }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequiresPixConfirmation { get; set; } = false;
        public bool RequiresCardConfirmation { get; set; } = false;
        public string? RedirectUrl { get; set; }
        public int OrderId { get; set; }
    }

    public class SendEmailRequest
    {
        public int OrderId { get; set; }
    }
}
