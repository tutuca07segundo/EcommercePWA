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
        private readonly IPixService _pixService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ICartService cartService, IOrderService orderService, IEmailService emailService, IMercadoPagoService mercadoPagoService, IPixService pixService, ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
            _emailService = emailService;
            _mercadoPagoService = mercadoPagoService;
            _pixService = pixService;
            _logger = logger;
        }

        // GET: /Checkout
        // [RequireAuth] - Temporariamente removido para teste
        public IActionResult Index()
        {
            var sessionId = GetSessionId();
            _logger.LogInformation($"Checkout acessado - SessionId: {sessionId}");
            
            var cart = _cartService.GetCart(sessionId);
            _logger.LogInformation($"Carrinho encontrado - Items: {cart.Items.Count}, Subtotal: {cart.Subtotal}");
            
            if (!cart.Items.Any())
            {
                _logger.LogWarning("Carrinho vazio - redirecionando para /Cart");
                return RedirectToAction("Index", "Cart");
            }
            
            // VALIDAÇÃO OBRIGATÓRIA: Verificar se frete foi calculado e selecionado
            if (!cart.ShippingCost.HasValue || cart.ShippingCost.Value < 0)
            {
                _logger.LogWarning("Frete não calculado - redirecionando para /Cart");
                TempData["ErrorMessage"] = "⚠️ FRETE OBRIGATÓRIO: Por favor, calcule e selecione uma opção de frete antes de finalizar a compra.";
                return RedirectToAction("Index", "Cart");
            }
            
            _logger.LogInformation($"Carrinho válido - Frete: R$ {cart.ShippingCost.Value:N2} - prosseguindo para checkout");

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
        // [RequireAuth] - Temporariamente removido para teste
        public async Task<IActionResult> Process(CheckoutViewModel model)
        {
            try
            {
                var sessionId = GetSessionId();
                _logger.LogInformation($"Processando checkout - SessionId: {sessionId}, Método: {model.PaymentMethod}");
                
                var cart = _cartService.GetCart(sessionId);
                _logger.LogInformation($"Dados do formulário - CardNumber: {!string.IsNullOrEmpty(model.CardNumber)}, CardName: {!string.IsNullOrEmpty(model.CardName)}");
                
                // SEMPRE garantir que o Cart esteja preenchido no modelo para evitar erro de validação
                var shippingCost = cart.ShippingCost ?? CalculateShippingCost(cart);
                var taxAmount = CalculateTaxAmount(cart.Subtotal);
                model.Cart = new CartViewModel
                {
                    Cart = cart,
                    Items = cart.Items,
                    Subtotal = cart.Subtotal,
                    ShippingCost = shippingCost,
                    TaxAmount = taxAmount,
                    Total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount
                };
                _logger.LogInformation("Cart preenchido no modelo para evitar erro de validação");
                
                // Validação customizada baseada no método de pagamento
                ValidatePaymentMethod(model);
                
                // Debug: verificar erros restantes
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido após validação:");
                    foreach (var error in ModelState)
                    {
                        if (error.Value.Errors.Count > 0)
                        {
                            _logger.LogWarning($"Campo: {error.Key}, Erros: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                }
                
                if (!ModelState.IsValid)
                {
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
                
                // Verificar método de pagamento para decidir o processamento
                PaymentResult paymentResult;
                
                if (model.PaymentMethod == "MercadoPago")
                {
                    _logger.LogInformation("Processando via Mercado Pago");
                    // Tentar processar via Mercado Pago
                    paymentResult = await ProcessMercadoPagoPayment(model, cart, order.Id);
                    
                    if (paymentResult.Success && !string.IsNullOrEmpty(paymentResult.RedirectUrl))
                    {
                        // Redirecionar para Mercado Pago
                        return Redirect(paymentResult.RedirectUrl);
                    }
                    
                    // Se MP falhar, cancelar pedido e mostrar erro
                    await _orderService.UpdateOrderStatus(order.Id, "Cancelado");
                    ModelState.AddModelError("", "Erro na integração com Mercado Pago. Tente outro método de pagamento.");
                    
                    // Reutilizar variáveis já declaradas
                    var cartViewModelError = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = shippingCost,
                        TaxAmount = taxAmount,
                        Total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount
                    };
                    model.Cart = cartViewModelError;
                    return View("Index", model);
                }
                else
                {
                    _logger.LogInformation($"Processando via simulação: {model.PaymentMethod}");
                    // Para outros métodos, usar simulação local
                    paymentResult = await ProcessPaymentSimulation(model);
                }
                
                if (!paymentResult.Success)
                {
                    // Cancelar pedido temporário
                    await _orderService.UpdateOrderStatus(order.Id, "Cancelado");
                    
                    // Verificar se é uma requisição AJAX
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || 
                        Request.ContentType?.Contains("application/json") == true)
                    {
                        return Json(new { success = false, message = paymentResult.Message });
                    }
                    
                    ModelState.AddModelError("", paymentResult.Message);
                    // Reutilizar variáveis já declaradas
                    var cartViewModelError2 = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = shippingCost,
                        TaxAmount = taxAmount,
                        Total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount
                    };
                    model.Cart = cartViewModelError2;
                    return View("Index", model);
                }

                // Se for PIX, gerar código PIX real e mostrar QR Code
                if (paymentResult.RequiresPixConfirmation)
                {
                    // Gerar PIX usando o serviço real
                    var total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount;
                    var descricao = $"Pedido Kaboom E-commerce - {cart.Items.Count} item(s)";
                    var identificador = $"KABOOM{DateTime.Now:yyyyMMddHHmmss}";
                    
                    var pixData = await _pixService.GeneratePixAsync(total, descricao, identificador);
                    
                    if (pixData.Success)
                    {
                        model.PixCode = pixData.PixCode;
                        model.PixQRCodeBase64 = pixData.QRCodeBase64;
                        model.PixExpirationTime = pixData.DataVencimento;
                        model.ShowPixQrCode = true;
                    }
                    else
                    {
                        // Fallback: gerar código simulado se o serviço falhar
                        model.PixCode = GeneratePixCode();
                        model.PixExpirationTime = DateTime.Now.AddMinutes(30);
                        model.ShowPixQrCode = true;
                        _logger.LogWarning($"Erro ao gerar PIX real: {pixData.ErrorMessage}. Usando código simulado.");
                    }
                    
                    // Reutilizar variáveis já declaradas
                    var cartViewModelPix = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = shippingCost,
                        TaxAmount = taxAmount,
                        Total = total
                    };
                    model.Cart = cartViewModelPix;
                    HttpContext.Session.SetInt32("PendingOrderId", order.Id);
                    return View("PixPayment", model);
                }

                // Se for cartão, mostrar tela de confirmação
                if (paymentResult.RequiresCardConfirmation)
                {
                    // Reutilizar variáveis já declaradas
                    var cartViewModelCard = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = shippingCost,
                        TaxAmount = taxAmount,
                        Total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount
                    };
                    model.Cart = cartViewModelCard;
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
                var successUrl = $"{Request.Scheme}://{Request.Host}/Checkout/PaymentSuccess";
                var failureUrl = $"{Request.Scheme}://{Request.Host}/Checkout/PaymentFailure";
                var pendingUrl = $"{Request.Scheme}://{Request.Host}/Checkout/PaymentPending";
                
                _logger.LogInformation($"URLs de retorno configuradas - Success: {successUrl}, Failure: {failureUrl}, Pending: {pendingUrl}");
                
                // Calcular valores totais (mesma lógica do carrinho)
                var subtotal = cart.Subtotal;
                var discountAmount = cart.DiscountAmount ?? 0;
                var shippingCost = cart.ShippingCost ?? CalculateShippingCost(cart);
                var taxAmount = CalculateTaxAmount(cart.Subtotal); // Impostos sobre subtotal bruto
                var finalTotal = subtotal - discountAmount + shippingCost + taxAmount;
                
                _logger.LogInformation($"Valores calculados - Subtotal: {subtotal}, Desconto: {discountAmount}, Frete: {shippingCost}, Impostos: {taxAmount}, Total: {finalTotal}");
                _logger.LogInformation($"Cálculo detalhado: {subtotal} - {discountAmount} + {shippingCost} + {taxAmount} = {finalTotal}");
                
                var items = new List<MercadoPagoItem>();
                
                // Estratégia simplificada: criar um único item com o total
                // O MP às vezes agrupa itens, então vamos enviar como um pedido único
                var mainProduct = cart.Items.First().Product;
                var productImageUrl = !string.IsNullOrEmpty(mainProduct.ImageUrl) 
                    ? $"{Request.Scheme}://{Request.Host}{mainProduct.ImageUrl}"
                    : $"{Request.Scheme}://{Request.Host}/images/no-image.png";
                
                var itemsDescription = string.Join(", ", cart.Items.Select(i => $"{i.Product.Name} (x{i.Quantity})"));
                
                items.Add(new MercadoPagoItem
                {
                    Id = "pedido-completo",
                    Title = cart.Items.Count == 1 ? 
                        $"{mainProduct.Name} (Qtd: {cart.Items.First().Quantity})" : 
                        $"Pedido com {cart.Items.Count} item(s)",
                    Description = $"Itens: {itemsDescription} | Frete: R$ {shippingCost:F2} | Impostos: R$ {taxAmount:F2}" + 
                                 (discountAmount > 0 ? $" | Desconto: -R$ {discountAmount:F2}" : ""),
                    PictureUrl = productImageUrl,
                    Quantity = 1,
                    UnitPrice = finalTotal,
                    CurrencyId = "BRL"
                });
                
                _logger.LogInformation($"Item único criado: {itemsDescription}, Total: {finalTotal}, Imagem: {productImageUrl}");

                var preference = new MercadoPagoPreferenceRequest
                {
                    ExternalReference = orderId.ToString(),
                    Items = items,
                    BackUrls = new MercadoPagoUrls
                    {
                        Success = successUrl,
                        Failure = failureUrl,
                        Pending = pendingUrl
                    }
                };
                
                // Calcular total dos itens que serão enviados ao MP
                var mpTotal = preference.Items.Sum(i => i.UnitPrice * i.Quantity);
                _logger.LogInformation($"Preferência criada com ExternalReference: {preference.ExternalReference}, Items: {preference.Items.Count}");
                _logger.LogInformation($"Total calculado localmente: {finalTotal}, Total que será enviado ao MP: {mpTotal}");

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
            _logger.LogInformation($"Iniciando simulação de pagamento para método: {model.PaymentMethod}");
            
            // Simulação com validação real de cartão
            await Task.Delay(1000);
            
            if (model.PaymentMethod == "pix")
            {
                _logger.LogInformation("Processando PIX - sem validação de cartão");
                model.PixCode = GeneratePixCode();
                model.PixExpirationTime = DateTime.Now.AddMinutes(5);
                model.ShowPixQrCode = true;
                return new PaymentResult { Success = true, Message = "PIX gerado com sucesso", RequiresPixConfirmation = true };
            }
            else if (model.PaymentMethod == "boleto")
            {
                _logger.LogInformation("Processando Boleto - sem validação de cartão");
                return new PaymentResult { Success = true, Message = "Boleto gerado com sucesso" };
            }
            else if (model.PaymentMethod == "credit_card" || model.PaymentMethod == "debit_card")
            {
                _logger.LogInformation("Processando cartão - com validação completa");
                
                // Validação rigorosa de cartão APENAS para métodos de cartão
                var validationResult = ValidateCardDetails(model);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning($"Validação de cartão falhou: {validationResult.ErrorMessage}");
                    return new PaymentResult { Success = false, Message = validationResult.ErrorMessage };
                }

                // Simular processamento real com possibilidade de negação
                var isApproved = SimulateCardProcessing(model.CardNumber);
                if (!isApproved)
                {
                    _logger.LogInformation("Cartão foi negado na simulação");
                    return new PaymentResult { Success = false, Message = "Cartão recusado. Verifique os dados ou tente outro cartão." };
                }

                _logger.LogInformation("Cartão aprovado na simulação");
                return new PaymentResult { Success = true, Message = "Pagamento aprovado", RequiresCardConfirmation = true };
            }

            _logger.LogError($"Método de pagamento inválido: {model.PaymentMethod}");
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

        private void ValidatePaymentMethod(CheckoutViewModel model)
        {
            _logger.LogInformation($"Validando método de pagamento: {model.PaymentMethod}");
            
            // Para PIX, Boleto e Mercado Pago, remover validações obrigatórias
            if (model.PaymentMethod == "pix" || model.PaymentMethod == "boleto" || model.PaymentMethod == "MercadoPago")
            {
                _logger.LogInformation($"Método {model.PaymentMethod} selecionado - removendo validações obrigatórias");
                
                // Remover erros de validação de campos de entrega e cartão
                var fieldsToRemove = new[] { 
                    "ShippingName", "ShippingAddress", "ShippingCity", "ShippingZipCode", "ShippingCountry", "ShippingPhone",
                    "CardNumber", "CardName", "CardExpiry", "CardCvv", // Também remover validações de cartão
                    "Cart" // Remover erro do campo Cart também
                };
                
                foreach (var field in fieldsToRemove)
                {
                    if (ModelState.ContainsKey(field))
                    {
                        ModelState.Remove(field);
                        _logger.LogInformation($"Removido erro de validação para campo: {field}");
                    }
                }
                
                // PIX, Boleto e Mercado Pago não precisam de dados de cartão nem entrega obrigatória
                return;
            }
            
            // Só validar campos de cartão se o método for cartão (simulação)
            if (model.PaymentMethod == "credit_card" || model.PaymentMethod == "debit_card")
            {
                _logger.LogInformation("Validando campos de cartão para simulação");
                
                // Remover erro do Cart também para cartão
                if (ModelState.ContainsKey("Cart"))
                {
                    ModelState.Remove("Cart");
                    _logger.LogInformation("Removido erro de validação Cart para método cartão");
                }
                
                if (string.IsNullOrEmpty(model.CardNumber))
                    ModelState.AddModelError("CardNumber", "Número do cartão é obrigatório");
                
                if (string.IsNullOrEmpty(model.CardName))
                    ModelState.AddModelError("CardName", "Nome no cartão é obrigatório");
                
                if (string.IsNullOrEmpty(model.CardExpiry))
                    ModelState.AddModelError("CardExpiry", "Data de validade é obrigatória");
                
                if (string.IsNullOrEmpty(model.CardCvv))
                    ModelState.AddModelError("CardCvv", "CVV é obrigatório");
            }
            else
            {
                _logger.LogWarning($"Método de pagamento desconhecido: {model.PaymentMethod}");
            }
        }

        private string GetSessionId()
        {
            // Usar a mesma lógica do CartController para manter consistência
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

        [HttpPost]
        public async Task<IActionResult> GeneratePix([FromBody] GeneratePixRequest request)
        {
            try
            {
                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);

                if (cart == null || !cart.Items.Any())
                {
                    return Json(new { success = false, message = "Carrinho vazio" });
                }

                var shippingCost = cart.ShippingCost ?? CalculateShippingCost(cart);
                var taxAmount = CalculateTaxAmount(cart.Subtotal);
                var total = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount;

                var descricao = $"Pedido Kaboom E-commerce - {cart.Items.Count} item(s)";
                var identificador = $"KABOOM{DateTime.Now:yyyyMMddHHmmss}";

                var pixData = await _pixService.GeneratePixAsync(total, descricao, identificador);

                if (pixData.Success)
                {
                    return Json(new
                    {
                        success = true,
                        pixCode = pixData.PixCode,
                        qrCodeBase64 = pixData.QRCodeBase64,
                        valor = pixData.Valor,
                        descricao = pixData.Descricao,
                        identificador = pixData.Identificador,
                        chavePix = pixData.ChavePix,
                        dataVencimento = pixData.DataVencimento.ToString("dd/MM/yyyy HH:mm"),
                        message = "PIX gerado com sucesso!"
                    });
                }
                else
                {
                    return Json(new { success = false, message = pixData.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar PIX");
                return Json(new { success = false, message = "Erro interno do servidor" });
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

    public class GeneratePixRequest
    {
        // Pode ser expandido futuramente para incluir dados específicos
    }

    public class SendEmailRequest
    {
        public int OrderId { get; set; }
    }
}
