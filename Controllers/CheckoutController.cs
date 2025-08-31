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
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ICartService cartService, IOrderService orderService, IEmailService emailService, ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
            _emailService = emailService;
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

            var cartViewModel = new CartViewModel
            {
                Cart = cart,
                Items = cart.Items,
                Subtotal = cart.Subtotal,
                ShippingCost = CalculateShippingCost(cart),
                TaxAmount = CalculateTaxAmount(cart.Subtotal),
                Total = cart.Total + CalculateShippingCost(cart) + CalculateTaxAmount(cart.Subtotal)
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
                if (!ValidatePaymentMethod(model))
                {
                    var cartViewModel = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = CalculateShippingCost(cart),
                        TaxAmount = CalculateTaxAmount(cart.Subtotal),
                        Total = cart.Total + CalculateShippingCost(cart) + CalculateTaxAmount(cart.Subtotal)
                    };
                    model.Cart = cartViewModel;
                    return View("Index", model);
                }
                
                if (!cart.Items.Any())
                {
                    return RedirectToAction("Index", "Cart");
                }

                // Processar pagamento (simulado)
                var paymentResult = await ProcessPayment(model);
                
                // Debug: Log do resultado do pagamento
                Console.WriteLine($"PaymentResult - Success: {paymentResult.Success}");
                Console.WriteLine($"PaymentResult - RequiresPixConfirmation: {paymentResult.RequiresPixConfirmation}");
                Console.WriteLine($"PaymentResult - RequiresCardConfirmation: {paymentResult.RequiresCardConfirmation}");
                Console.WriteLine($"PaymentMethod: {model.PaymentMethod}");
                
                if (!paymentResult.Success)
                {
                    ModelState.AddModelError("", paymentResult.Message);
                    var cartViewModel = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = CalculateShippingCost(cart),
                        TaxAmount = CalculateTaxAmount(cart.Subtotal),
                        Total = cart.Total + CalculateShippingCost(cart) + CalculateTaxAmount(cart.Subtotal)
                    };
                    model.Cart = cartViewModel;
                    return View("Index", model);
                }

                // Se for PIX e requer confirmação, mostrar QR Code
                if (paymentResult.RequiresPixConfirmation)
                {
                    var cartViewModel = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = CalculateShippingCost(cart),
                        TaxAmount = CalculateTaxAmount(cart.Subtotal),
                        Total = cart.Total + CalculateShippingCost(cart) + CalculateTaxAmount(cart.Subtotal)
                    };
                    model.Cart = cartViewModel;
                    return View("PixPayment", model);
                }

                // Se for cartão de crédito ou débito, mostrar tela de confirmação
                if (paymentResult.RequiresCardConfirmation)
                {
                    Console.WriteLine("Redirecionando para CardPayment view");
                    var cartViewModel = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = CalculateShippingCost(cart),
                        TaxAmount = CalculateTaxAmount(cart.Subtotal),
                        Total = cart.Total + CalculateShippingCost(cart) + CalculateTaxAmount(cart.Subtotal)
                    };
                    model.Cart = cartViewModel;
                    return View("CardPayment", model);
                }

                // Criar pedido
                var order = await CreateOrder(model, cart, sessionId);
                
                // Limpar carrinho
                _cartService.ClearCart(sessionId);
                
                // Redirecionar para confirmação
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

        private async Task<PaymentResult> ProcessPayment(CheckoutViewModel model)
        {
            // Simulação de processamento de pagamento
            await Task.Delay(1000); // Simular delay de API
            
            // Validação baseada no método de pagamento
            if (model.PaymentMethod == "pix")
            {
                // Para PIX, gerar código e definir expiração
                model.PixCode = GeneratePixCode();
                model.PixExpirationTime = DateTime.Now.AddMinutes(5);
                model.ShowPixQrCode = true;
                return new PaymentResult { Success = true, Message = "PIX gerado com sucesso", RequiresPixConfirmation = true };
            }
            else if (model.PaymentMethod == "boleto")
            {
                // Para boleto, apenas aprovar
                return new PaymentResult { Success = true, Message = "Boleto gerado com sucesso" };
            }
            else if (model.PaymentMethod == "credit_card" || model.PaymentMethod == "debit_card")
            {
                // Debug: Log dos dados do cartão
                Console.WriteLine($"CardNumber: '{model.CardNumber}'");
                Console.WriteLine($"CardName: '{model.CardName}'");
                Console.WriteLine($"CardExpiry: '{model.CardExpiry}'");
                Console.WriteLine($"CardCvv: '{model.CardCvv}'");

                // Validação básica do cartão
                var cardNumber = model.CardNumber?.Replace(" ", "");
                if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 13)
                {
                    Console.WriteLine("Falha: Número do cartão inválido");
                    return new PaymentResult { Success = false, Message = "Número do cartão inválido" };
                }

                if (string.IsNullOrEmpty(model.CardExpiry) || !IsValidExpiry(model.CardExpiry))
                {
                    Console.WriteLine("Falha: Data de validade inválida");
                    return new PaymentResult { Success = false, Message = "Data de validade inválida" };
                }

                if (string.IsNullOrEmpty(model.CardCvv) || model.CardCvv.Length < 3)
                {
                    Console.WriteLine("Falha: CVV inválido");
                    return new PaymentResult { Success = false, Message = "CVV inválido" };
                }

                // Simular processamento bem-sucedido para cartão
                Console.WriteLine("Cartão validado com sucesso!");
                return new PaymentResult { Success = true, Message = "Pagamento aprovado", RequiresCardConfirmation = true };
            }

            return new PaymentResult { Success = false, Message = "Método de pagamento inválido" };
        }

        private async Task<Order> CreateOrder(CheckoutViewModel model, Cart cart, string sessionId)
        {
            var paymentStatus = model.PaymentMethod == "pix" ? "Pending" : "Paid";
            var orderStatus = model.PaymentMethod == "pix" ? "Awaiting Payment" : "Processing";
            
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
                ShippingCost = CalculateShippingCost(cart),
                TaxAmount = CalculateTaxAmount(cart.Subtotal),
                DiscountAmount = 0, // Desconto simulado - será implementado com cupons posteriormente
                Total = cart.Total + CalculateShippingCost(cart) + CalculateTaxAmount(cart.Subtotal),
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
            if (cart.Total >= 200)
                return 0;
            else if (cart.Total >= 100)
                return 15;
            else
                return 25;
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

        // POST: /Checkout/ConfirmPixPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> ConfirmPixPayment(CheckoutViewModel model)
        {
            try
            {
                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);
                
                // Criar pedido
                var order = await CreateOrder(model, cart, sessionId);
                
                // Limpar carrinho
                _cartService.ClearCart(sessionId);
                
                // Redirecionar para confirmação
                return RedirectToAction("Confirmation", new { orderId = order.Id });
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
                Console.WriteLine("ConfirmCardPayment chamado");
                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);
                
                Console.WriteLine($"Cart items: {cart.Items.Count}");
                
                // Criar pedido
                var order = await CreateOrder(model, cart, sessionId);
                Console.WriteLine($"Pedido criado com ID: {order.Id}");
                
                // Limpar carrinho
                _cartService.ClearCart(sessionId);
                Console.WriteLine("Carrinho limpo");
                
                // Redirecionar para confirmação
                Console.WriteLine("Redirecionando para Confirmation");
                return RedirectToAction("Confirmation", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro em ConfirmCardPayment: {ex.Message}");
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
    }

    public class SendEmailRequest
    {
        public int OrderId { get; set; }
    }
}
