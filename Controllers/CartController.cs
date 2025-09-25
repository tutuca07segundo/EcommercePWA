using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;
using EcommercePWA.Attributes;
using System.Text.Json;

namespace EcommercePWA.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly IDiscountService _discountService;
        private readonly ICouponService _couponService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, IProductService productService, IDiscountService discountService, ICouponService couponService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _discountService = discountService;
            _couponService = couponService;
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
                        
                        // Apply discount to product
                        var discount = await _discountService.GetActiveDiscountForProductAsync(product.Id);
                        decimal basePrice = product.Price;
                        
                        if (discount != null)
                        {
                            basePrice = _discountService.CalculateDiscountedPrice(product.Price, discount);
                            product.DiscountedPrice = basePrice;
                            product.HasDiscount = true;
                        }
                        
                        if (item.ProductVariationId.HasValue)
                        {
                            item.ProductVariation = product.Variations.FirstOrDefault(v => v.Id == item.ProductVariationId);
                            // Atualizar o preço unitário com base na variação e desconto
                            if (item.ProductVariation?.PriceModifier.HasValue == true)
                            {
                                item.UnitPrice = basePrice + item.ProductVariation.PriceModifier.Value;
                            }
                            else
                            {
                                item.UnitPrice = basePrice;
                            }
                        }
                        else
                        {
                            item.UnitPrice = basePrice;
                        }
                    }
                    cartItems.Add(item);
                }
                
                // Usar dados do cupom persistidos no carrinho
                string couponMessage = "";
                if (!string.IsNullOrEmpty(cart.CouponCode) && cart.DiscountAmount.HasValue)
                {
                    couponMessage = $"Cupom {cart.CouponCode} aplicado! Desconto: R$ {cart.DiscountAmount.Value:N2}";
                }
                
                var shippingCost = CalculateShippingCost(cart);
                var taxAmount = CalculateTaxAmount(cart.Subtotal);
                var finalTotal = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount;
                
                var viewModel = new CartViewModel
                {
                    Cart = cart,
                    Items = cartItems,
                    Subtotal = cart.Subtotal,
                    ShippingCost = shippingCost,
                    TaxAmount = taxAmount,
                    Total = finalTotal,
                    CouponCode = cart.CouponCode,
                    DiscountAmount = cart.DiscountAmount,
                    CouponMessage = couponMessage,
                    IsCouponValid = !string.IsNullOrEmpty(cart.CouponCode)
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

                // O cupom já foi recalculado automaticamente no CartService
                
                var shippingCost = CalculateShippingCost(cart);
                var taxAmount = CalculateTaxAmount(cart.Subtotal);
                var finalTotal = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount;
                
                return Json(new {
                    success = true,
                    itemId = request.ItemId,
                    itemTotal = item?.Total ?? 0,
                    subtotal = cart.Subtotal,
                    shippingCost = shippingCost,
                    taxAmount = taxAmount,
                    discountAmount = cart.DiscountAmount ?? 0,
                    total = finalTotal,
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
                
                // Calcular valores para retorno
                var shippingCost = CalculateShippingCost(cart);
                var taxAmount = CalculateTaxAmount(cart.Subtotal);
                var finalTotal = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount;
                
                return Json(new { 
                    success = true, 
                    message = "Item removido do carrinho",
                    cartItemCount = cart.ItemCount,
                    subtotal = cart.Subtotal,
                    shippingCost = shippingCost,
                    taxAmount = taxAmount,
                    discountAmount = cart.DiscountAmount ?? 0,
                    total = finalTotal,
                    couponCode = cart.CouponCode
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
                    var finalTotal = cart.Subtotal - (cart.DiscountAmount ?? 0) + shippingCost + taxAmount;
                    
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
                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);
                if (cart == null)
                {
                    return Json(new { success = false, message = "Carrinho não encontrado" });
                }

                // Remover cupom da sessão
                HttpContext.Session.Remove("AppliedCoupon");
                _cartService.RemoveCoupon(sessionId);

                // Recalcular totais sem cupom
                var viewModel = new CartViewModel
                {
                    Items = cart.Items,
                    Subtotal = cart.Items.Sum(i => i.Total),
                    ShippingCost = CalculateShippingCostFallback(cart),
                    TaxAmount = CalculateTaxAmount(cart.Items.Sum(i => i.Total)),
                    DiscountAmount = null,
                    CouponCode = null,
                    CouponMessage = null,
                    IsCouponValid = false
                };

                viewModel.Total = viewModel.Subtotal + viewModel.ShippingCost + viewModel.TaxAmount - (viewModel.DiscountAmount ?? 0);

                return Json(new { 
                    success = true, 
                    message = "Cupom removido com sucesso!",
                    subtotal = viewModel.Subtotal,
                    shippingCost = viewModel.ShippingCost,
                    taxAmount = viewModel.TaxAmount,
                    discountAmount = 0,
                    total = viewModel.Total
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao remover cupom: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CalculateShipping([FromBody] CalculateShippingRequest request)
        {
            try
            {
                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);
                if (cart == null)
                {
                    return Json(new { success = false, message = "Carrinho não encontrado" });
                }

                if (string.IsNullOrEmpty(request.Cep) || request.Cep.Length != 8)
                {
                    return Json(new { success = false, message = "CEP inválido" });
                }

                // Validar CEP via ViaCEP
                var cepInfo = await ValidateCepAsync(request.Cep);
                if (cepInfo == null)
                {
                    return Json(new { success = false, message = "CEP não encontrado" });
                }

                // Calcular frete usando fallback (lógica simples)
                decimal shippingCost = CalculateShippingCostFallback(cart);
                string estimatedDays = "5-10";

                // Salvar CEP na sessão para usar no checkout
                HttpContext.Session.SetString("ShippingCep", request.Cep);
                HttpContext.Session.SetString("ShippingCost", shippingCost.ToString());

                // Recalcular total
                var subtotal = cart.Items.Sum(i => i.Total);
                var taxAmount = CalculateTaxAmount(subtotal);
                
                // Aplicar desconto se houver cupom
                decimal discountAmount = 0;
                var appliedCoupon = HttpContext.Session.GetString("AppliedCoupon");
                if (!string.IsNullOrEmpty(appliedCoupon))
                {
                    var coupon = _couponService.GetCouponByCode(appliedCoupon);
                    if (coupon != null && _couponService.ValidateCoupon(appliedCoupon, subtotal))
                    {
                        if (coupon.DiscountPercentage.HasValue)
                        {
                            discountAmount = subtotal * (coupon.DiscountPercentage.Value / 100);
                        }
                        else if (coupon.DiscountAmount > 0)
                        {
                            discountAmount = coupon.DiscountAmount;
                        }
                    }
                }

                var total = subtotal + shippingCost + taxAmount - discountAmount;

                return Json(new
                {
                    success = true,
                    shippingCost = shippingCost,
                    total = total,
                    city = cepInfo.City,
                    estimatedDays = estimatedDays,
                    message = "Frete calculado com sucesso!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao calcular frete: " + ex.Message });
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

        private async Task<decimal> CalculateShippingCostAsync(Cart cart, string? destinationCep = null)
        {
            try
            {
                // Se não tiver CEP de destino, usar lógica de fallback
                if (string.IsNullOrEmpty(destinationCep))
                {
                    return CalculateShippingCostFallback(cart);
                }

                // Como não temos mais API de frete, usar lógica simples
                return CalculateShippingCostFallback(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular frete via API");
            }

            // Fallback para lógica simples
            return CalculateShippingCostFallback(cart);
        }

        private decimal CalculateShippingCost(Cart cart)
        {
            return CalculateShippingCostFallback(cart);
        }

        private decimal CalculateShippingCostFallback(Cart cart)
        {
            // Lógica simples de frete baseada no valor do pedido
            if (cart.Total >= 5000)
                return 0; // Frete grátis para pedidos acima de R$ 200
            else if (cart.Total >= 3000)
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

        private async Task<CepInfo?> ValidateCepAsync(string cep)
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"https://viacep.com.br/ws/{cep}/json/");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var cepData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(content);
                    
                    // Verificar se o CEP existe (ViaCEP retorna erro: true quando CEP não existe)
                    if (content.Contains("\"erro\""))
                    {
                        return null;
                    }
                    
                    var cepInfo = System.Text.Json.JsonSerializer.Deserialize<CepInfo>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    
                    return new CepInfo
                    {
                        Cep = cep,
                        City = GetJsonProperty(content, "localidade") ?? "Cidade não encontrada",
                        State = GetJsonProperty(content, "uf") ?? "",
                        District = GetJsonProperty(content, "bairro") ?? ""
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar CEP via ViaCEP");
            }
            
            return null;
        }

        private string? GetJsonProperty(string json, string propertyName)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.TryGetProperty(propertyName, out var property))
                {
                    return property.GetString();
                }
            }
            catch
            {
                // Ignorar erros de parsing
            }
            return null;
        }

        private string GetCartId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return userId.HasValue ? $"user_{userId.Value}" : GetSessionId();
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

    public class CalculateShippingRequest
    {
        public string Cep { get; set; } = string.Empty;
    }

    public class CepInfo
    {
        public string Cep { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
    }
}
