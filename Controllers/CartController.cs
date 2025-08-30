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
        public async Task<IActionResult> Index()
        {
            try
            {
                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);
                
                // Carregar dados completos dos produtos e atualizar preços
                var cartItems = new List<CartItem>();
                foreach (var item in cart.Items)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        item.Product = product;
                        if (item.ProductVariationId.HasValue)
                        {
                            item.ProductVariation = product.Variations.FirstOrDefault(v => v.Id == item.ProductVariationId);
                            // Atualizar o preço unitário com base na variação
                            if (item.ProductVariation?.PriceModifier.HasValue == true)
                            {
                                item.UnitPrice = product.Price + item.ProductVariation.PriceModifier.Value;
                            }
                            else
                            {
                                item.UnitPrice = product.Price;
                            }
                        }
                        else
                        {
                            item.UnitPrice = product.Price;
                        }
                    }
                    cartItems.Add(item);
                }
                
                // Verificar se há cupom aplicado na sessão
                var couponCode = HttpContext.Session.GetString("CouponCode");
                decimal discountAmount = 0;
                string couponMessage = "";
                bool isCouponValid = false;
                
                if (!string.IsNullOrEmpty(couponCode))
                {
                    var couponResult = _cartService.ApplyCoupon(sessionId, couponCode);
                    if (couponResult.IsValid)
                    {
                        discountAmount = couponResult.DiscountAmount;
                        couponMessage = couponResult.Message;
                        isCouponValid = true;
                        // Atualizar sessão com novo valor
                        HttpContext.Session.SetString("DiscountAmount", discountAmount.ToString());
                    }
                    else
                    {
                        // Cupom inválido, remover da sessão
                        HttpContext.Session.Remove("CouponCode");
                        HttpContext.Session.Remove("DiscountAmount");
                    }
                }
                
                var shippingCost = CalculateShippingCost(cart);
                var taxAmount = CalculateTaxAmount(cart.Subtotal);
                var finalTotal = cart.Total + shippingCost + taxAmount - discountAmount;
                
                var viewModel = new CartViewModel
                {
                    Cart = cart,
                    Items = cartItems,
                    Subtotal = cart.Subtotal,
                    ShippingCost = shippingCost,
                    TaxAmount = taxAmount,
                    Total = finalTotal,
                    CouponCode = couponCode,
                    DiscountAmount = discountAmount > 0 ? discountAmount : null,
                    CouponMessage = couponMessage,
                    IsCouponValid = isCouponValid
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar carrinho");
                return View(new CartViewModel
                {
                    Cart = new Cart { Items = new List<CartItem>() },
                    Items = new List<CartItem>(),
                    Subtotal = 0,
                    ShippingCost = 0,
                    TaxAmount = 0,
                    Total = 0
                });
            }
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                // Verificar se o usuário está autenticado
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return Json(new { 
                        success = false, 
                        message = "Usuário não autenticado", 
                        requireLogin = true,
                        returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString
                    });
                }

                var sessionId = GetSessionId();
                var product = await _productService.GetProductByIdAsync(request.ProductId);
                
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

                await _cartService.AddToCart(sessionId, request.ProductId, request.VariationId, request.Quantity);
                
                var cart = _cartService.GetCart(sessionId);
                
                return Json(new { 
                    success = true, 
                    message = "Produto adicionado ao carrinho",
                    cartItemCount = cart.ItemCount,
                    cartTotal = cart.Total.ToString("C"),
                    cartSubtotal = cart.Subtotal.ToString("C"),
                    redirectToCart = true
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
                var item = cart.Items.FirstOrDefault(i => i.Id == request.ItemId);

                // Recuperar cupom aplicado, se houver
                string? couponCode = null;
                if (HttpContext.Session.TryGetValue("CouponCode", out var couponBytes))
                {
                    couponCode = System.Text.Encoding.UTF8.GetString(couponBytes);
                }
                decimal discountAmount = 0;
                if (!string.IsNullOrEmpty(couponCode))
                {
                    var couponResult = _cartService.ApplyCoupon(sessionId, couponCode);
                    if (couponResult.IsValid)
                        discountAmount = couponResult.DiscountAmount;
                }
                
                return Json(new {
                    success = true,
                    itemId = request.ItemId,
                    itemTotal = item?.Total ?? 0,
                    subtotal = cart.Subtotal,
                    shippingCost = CalculateShippingCost(cart),
                    taxAmount = CalculateTaxAmount(cart.Subtotal),
                    discountAmount = discountAmount,
                    total = cart.Total + CalculateShippingCost(cart) + CalculateTaxAmount(cart.Subtotal) - discountAmount,
                    cartItemCount = cart.ItemCount
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
                    // Salvar cupom na sessão
                    HttpContext.Session.SetString("CouponCode", request.CouponCode);
                    HttpContext.Session.SetString("DiscountAmount", result.DiscountAmount.ToString());
                    
                    var cart = _cartService.GetCart(sessionId);
                    var shippingCost = CalculateShippingCost(cart);
                    var taxAmount = CalculateTaxAmount(cart.Subtotal);
                    var finalTotal = cart.Total + shippingCost + taxAmount - result.DiscountAmount;
                    
                    return Json(new { 
                        success = true, 
                        message = result.Message,
                        discountAmount = result.DiscountAmount,
                        subtotal = cart.Subtotal,
                        shippingCost = shippingCost,
                        taxAmount = taxAmount,
                        total = finalTotal,
                        couponCode = request.CouponCode
                    });
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

        // POST: /Cart/RemoveCoupon
        [HttpPost]
        [RequireAuth]
        public IActionResult RemoveCoupon()
        {
            try
            {
                // Remover cupom da sessão
                HttpContext.Session.Remove("CouponCode");
                HttpContext.Session.Remove("DiscountAmount");
                
                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);
                var shippingCost = CalculateShippingCost(cart);
                var taxAmount = CalculateTaxAmount(cart.Subtotal);
                var finalTotal = cart.Total + shippingCost + taxAmount;
                
                return Json(new { 
                    success = true, 
                    message = "Cupom removido com sucesso",
                    subtotal = cart.Subtotal,
                    shippingCost = shippingCost,
                    taxAmount = taxAmount,
                    total = finalTotal
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover cupom");
                return Json(new { success = false, message = "Erro ao remover cupom" });
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

        // GET: /Cart/GetCartCount
        [HttpGet]
        public IActionResult GetCartCount()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return Json(new { success = false, count = 0 });
                }

                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);
                
                return Json(new { 
                    success = true, 
                    count = cart.ItemCount 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter contagem do carrinho");
                return Json(new { success = false, count = 0 });
            }
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

    public class RemoveCouponRequest
    {
        // Classe vazia, apenas para consistência
    }
}
