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
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ICartService cartService, IOrderService orderService, ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
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
                
                if (!ModelState.IsValid)
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

        private async Task<PaymentResult> ProcessPayment(CheckoutViewModel model)
        {
            // Simulação de processamento de pagamento
            await Task.Delay(1000); // Simular delay de API
            
            // Validação básica do cartão
            if (string.IsNullOrEmpty(model.CardNumber) || model.CardNumber.Length < 13)
            {
                return new PaymentResult { Success = false, Message = "Número do cartão inválido" };
            }

            if (string.IsNullOrEmpty(model.CardExpiry) || !IsValidExpiry(model.CardExpiry))
            {
                return new PaymentResult { Success = false, Message = "Data de validade inválida" };
            }

            if (string.IsNullOrEmpty(model.CardCvv) || model.CardCvv.Length < 3)
            {
                return new PaymentResult { Success = false, Message = "CVV inválido" };
            }

            // Simular falha aleatória (5% de chance)
            var random = new Random();
            if (random.Next(100) < 5)
            {
                return new PaymentResult { Success = false, Message = "Pagamento recusado. Tente novamente." };
            }

            return new PaymentResult { Success = true, Message = "Pagamento aprovado" };
        }

        private async Task<Order> CreateOrder(CheckoutViewModel model, Cart cart, string sessionId)
        {
            var order = new Order
            {
                UserId = HttpContext.Session.GetInt32("UserId") ?? 1, // Usar usuário logado
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.Now,
                Status = "Pending",
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
                PaymentStatus = "Paid",
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
            if (string.IsNullOrEmpty(expiry) || expiry.Length != 5 || !expiry.Contains('/'))
                return false;

            var parts = expiry.Split('/');
            if (parts.Length != 2)
                return false;

            if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
                return false;

            if (month < 1 || month > 12)
                return false;

            var currentYear = DateTime.Now.Year % 100;
            var currentMonth = DateTime.Now.Month;

            if (year < currentYear || (year == currentYear && month < currentMonth))
                return false;

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
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
