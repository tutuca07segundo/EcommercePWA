using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, IProductService productService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _logger = logger;
        }

        // GET: /Cart
        [RequireAuth]
        public IActionResult Index()
        {
            var sessionId = GetSessionId();
            var cart = _cartService.GetCart(sessionId);
            
            var viewModel = new CartViewModel
            {
                Cart = cart,
                Items = cart.Items,
                Subtotal = cart.Subtotal,
                ShippingCost = CalculateShippingCost(cart),
                TaxAmount = CalculateTaxAmount(cart.Subtotal),
                Total = cart.Total + CalculateShippingCost(cart) + CalculateTaxAmount(cart.Subtotal)
            };

            return View(viewModel);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        [RequireAuth]
        public IActionResult AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                var sessionId = GetSessionId();
                var product = _productService.GetProductById(request.ProductId);
                
                if (product == null)
                {
                    return Json(new { success = false, message = "Produto não encontrado" });
                }

                if (request.VariationId.HasValue)
                {
                    var variation = product.Variations.FirstOrDefault(v => v.Id == request.VariationId && v.IsAvailable);
                    if (variation == null)
                    {
                        return Json(new { success = false, message = "Variação não disponível" });
                    }
                }

                _cartService.AddToCart(sessionId, request.ProductId, request.VariationId, request.Quantity);
                
                var cart = _cartService.GetCart(sessionId);
                
                return Json(new { 
                    success = true, 
                    message = "Produto adicionado ao carrinho",
                    cartItemCount = cart.ItemCount,
                    cartTotal = cart.Total.ToString("C")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar produto ao carrinho");
                return Json(new { success = false, message = "Erro ao adicionar produto ao carrinho" });
            }
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [RequireAuth]
        public IActionResult UpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            try
            {
                var sessionId = GetSessionId();
                _cartService.UpdateQuantity(sessionId, request.ItemId, request.Quantity);
                
                var cart = _cartService.GetCart(sessionId);
                
                return Json(new { 
                    success = true, 
                    cartTotal = cart.Total.ToString("C"),
                    itemTotal = cart.Items.FirstOrDefault(i => i.Id == request.ItemId)?.Total.ToString("C")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar quantidade");
                return Json(new { success = false, message = "Erro ao atualizar quantidade" });
            }
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        [RequireAuth]
        public IActionResult RemoveItem([FromBody] RemoveItemRequest request)
        {
            try
            {
                var sessionId = GetSessionId();
                _cartService.RemoveItem(sessionId, request.ItemId);
                
                var cart = _cartService.GetCart(sessionId);
                
                return Json(new { 
                    success = true, 
                    message = "Item removido do carrinho",
                    cartItemCount = cart.ItemCount,
                    cartTotal = cart.Total.ToString("C")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover item");
                return Json(new { success = false, message = "Erro ao remover item" });
            }
        }

        // POST: /Cart/ApplyCoupon
        [HttpPost]
        [RequireAuth]
        public IActionResult ApplyCoupon([FromBody] ApplyCouponRequest request)
        {
            try
            {
                var sessionId = GetSessionId();
                var result = _cartService.ApplyCoupon(sessionId, request.CouponCode);
                
                if (result.IsValid)
                {
                    var cart = _cartService.GetCart(sessionId);
                    var viewModel = new CartViewModel
                    {
                        Cart = cart,
                        Items = cart.Items,
                        Subtotal = cart.Subtotal,
                        ShippingCost = CalculateShippingCost(cart),
                        TaxAmount = CalculateTaxAmount(cart.Subtotal),
                        Total = cart.Total + CalculateShippingCost(cart) + CalculateTaxAmount(cart.Subtotal),
                        CouponCode = request.CouponCode,
                        DiscountAmount = result.DiscountAmount,
                        CouponMessage = result.Message,
                        IsCouponValid = true
                    };
                    
                    return PartialView("_CartSummary", viewModel);
                }
                else
                {
                    return Json(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aplicar cupom");
                return Json(new { success = false, message = "Erro ao aplicar cupom" });
            }
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
            // Lógica simples de frete baseada no valor do pedido
            if (cart.Total >= 200)
                return 0; // Frete grátis para pedidos acima de R$ 200
            else if (cart.Total >= 100)
                return 15; // R$ 15 para pedidos entre R$ 100 e R$ 200
            else
                return 25; // R$ 25 para pedidos abaixo de R$ 100
        }

        private decimal CalculateTaxAmount(decimal subtotal)
        {
            // Imposto simples de 10%
            return subtotal * 0.10m;
        }
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int? VariationId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateQuantityRequest
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveItemRequest
    {
        public int ItemId { get; set; }
    }

    public class ApplyCouponRequest
    {
        public string CouponCode { get; set; } = string.Empty;
    }
}
