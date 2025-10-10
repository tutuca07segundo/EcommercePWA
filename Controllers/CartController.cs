using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;
using EcommercePWA.Attributes;
using EcommercePWA.Constants;
using System.Text.Json;

namespace EcommercePWA.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly IProductPricingService _productPricingService;
        private readonly ICartCalculationService _cartCalculationService;
        private readonly IMelhorEnvioService _melhorEnvioService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService, 
            IProductService productService,
            IProductPricingService productPricingService,
            ICartCalculationService cartCalculationService,
            IMelhorEnvioService melhorEnvioService, 
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _productPricingService = productPricingService;
            _cartCalculationService = cartCalculationService;
            _melhorEnvioService = melhorEnvioService;
            _logger = logger;
        }

        [RequireAuth]
        public async Task<IActionResult> Index()
        {
            try
            {
                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);
                var cartItems = await LoadCartItemsWithPricingAsync(cart.Items);
                var viewModel = await BuildCartViewModelAsync(cart, cartItems);
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar carrinho");
                return View(CreateEmptyCartViewModel());
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
                var totals = _cartCalculationService.CalculateTotals(cart);
                
                return Json(new {
                    success = true,
                    itemId = request.ItemId,
                    itemTotal = item?.Total ?? 0,
                    subtotal = totals.Subtotal,
                    shippingCost = totals.ShippingCost,
                    taxAmount = totals.TaxAmount,
                    discountAmount = totals.DiscountAmount,
                    total = totals.FinalTotal,
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
                var totals = _cartCalculationService.CalculateTotals(cart);
                
                return Json(new { 
                    success = true, 
                    message = "Item removido do carrinho",
                    cartItemCount = cart.ItemCount,
                    subtotal = totals.Subtotal,
                    shippingCost = totals.ShippingCost,
                    taxAmount = totals.TaxAmount,
                    discountAmount = totals.DiscountAmount,
                    total = totals.FinalTotal,
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
                    var totals = _cartCalculationService.CalculateTotals(cart);
                    
                    return Json(new { 
                        success = true, 
                        message = result.Message,
                        discountAmount = result.DiscountAmount,
                        subtotal = totals.Subtotal,
                        shippingCost = totals.ShippingCost,
                        taxAmount = totals.TaxAmount,
                        total = totals.FinalTotal,
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
                var totals = _cartCalculationService.CalculateTotals(cart);
                var viewModel = new CartViewModel
                {
                    Items = cart.Items,
                    Subtotal = totals.Subtotal,
                    ShippingCost = totals.ShippingCost,
                    TaxAmount = totals.TaxAmount,
                    DiscountAmount = null,
                    CouponCode = null,
                    CouponMessage = null,
                    IsCouponValid = false,
                    Total = totals.FinalTotal
                };

                return Json(new { 
                    success = true, 
                    message = "Cupom removido com sucesso!",
                    subtotal = totals.Subtotal,
                    shippingCost = totals.ShippingCost,
                    taxAmount = totals.TaxAmount,
                    discountAmount = 0,
                    total = totals.FinalTotal
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao calcular frete: " + ex.Message });
            }
        }

        // POST: /Cart/CalculateShipping
        [HttpPost]
        public async Task<IActionResult> CalculateShipping([FromBody] CalculateShippingRequest request)
        {
            try
            {
                var sessionId = GetSessionId();
                var cart = _cartService.GetCart(sessionId);
                
                // Preparar produtos para cálculo de frete
                var shippingProducts = new List<ShippingProduct>();
                
                // Se foi chamado da página de detalhes (com productId específico)
                if (request.ProductId.HasValue)
                {
                    var product = await _productService.GetProductByIdAsync(request.ProductId.Value);
                    if (product != null)
                    {
                        var quantity = request.Quantity ?? 1;
                        var unitPrice = product.HasDiscount && product.DiscountedPrice.HasValue ? 
                                       product.DiscountedPrice.Value : product.Price;
                        
                        shippingProducts.Add(new ShippingProduct
                        {
                            Id = product.Id,
                            Weight = product.Weight,
                            Width = product.Width,
                            Height = product.Height,
                            Length = product.Length,
                            Quantity = quantity,
                            Value = unitPrice
                        });
                    }
                }
                else
                {
                    // Lógica original para carrinho
                    if (!cart.Items.Any())
                    {
                        return Json(new { success = false, message = "Carrinho vazio" });
                    }

                    foreach (var item in cart.Items)
                    {
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            shippingProducts.Add(new ShippingProduct
                            {
                                Id = product.Id,
                                Weight = product.Weight,
                                Width = product.Width,
                                Height = product.Height,
                                Length = product.Length,
                                Quantity = item.Quantity,
                                Value = item.UnitPrice
                            });
                        }
                    }
                }

                if (!shippingProducts.Any())
                {
                    return Json(new { success = false, message = "Nenhum produto encontrado para cálculo" });
                }

                // Validar CEP
                var isValidCep = await _melhorEnvioService.ValidateCepAsync(request.Cep);
                if (!isValidCep)
                {
                    return Json(new { success = false, message = "CEP inválido" });
                }

                // Calcular frete via Melhor Envio
                var shippingRequest = new ShippingCalculationRequest
                {
                    ToCep = request.Cep,
                    Products = shippingProducts
                };

                var quotes = await _melhorEnvioService.CalculateShippingAsync(shippingRequest);
                
                if (quotes.Any())
                {
                    return Json(new { 
                        success = true, 
                        quotes = quotes.Select(q => new {
                            id = q.Id,
                            name = q.Name,
                            company = q.Company,
                            price = q.Price,
                            deliveryTime = q.DeliveryTime,
                            deliveryRange = q.DeliveryRange
                        }),
                        // Incluir dados do carrinho para manter cupom
                        subtotal = cart.Subtotal,
                        discountAmount = cart.DiscountAmount ?? 0,
                        couponCode = cart.CouponCode,
                        taxAmount = _cartCalculationService.CalculateTaxAmount(cart.Subtotal),
                        message = "Opções de frete calculadas com sucesso!"
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Não foi possível calcular o frete para este CEP" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular frete");
                return Json(new { success = false, message = "Erro ao calcular frete. Tente novamente." });
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


        private async Task<List<CartItem>> LoadCartItemsWithPricingAsync(List<CartItem> items)
        {
            var cartItems = new List<CartItem>();
            
            foreach (var item in items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    item.Product = product;
                    item.UnitPrice = await _productPricingService.CalculateItemPriceAsync(product, item.ProductVariationId);
                    
                    if (item.ProductVariationId.HasValue)
                    {
                        item.ProductVariation = product.Variations.FirstOrDefault(v => v.Id == item.ProductVariationId);
                    }
                }
                cartItems.Add(item);
            }
            
            return cartItems;
        }

        private async Task<CartViewModel> BuildCartViewModelAsync(Cart cart, List<CartItem> cartItems)
        {
            var totals = _cartCalculationService.CalculateTotals(cart);
            var couponMessage = BuildCouponMessage(cart);
            
            return new CartViewModel
            {
                Cart = cart,
                Items = cartItems,
                Subtotal = totals.Subtotal,
                ShippingCost = totals.ShippingCost,
                TaxAmount = totals.TaxAmount,
                Total = totals.FinalTotal,
                CouponCode = cart.CouponCode,
                DiscountAmount = cart.DiscountAmount,
                CouponMessage = couponMessage,
                IsCouponValid = !string.IsNullOrEmpty(cart.CouponCode)
            };
        }

        private static string BuildCouponMessage(Cart cart)
        {
            if (!string.IsNullOrEmpty(cart.CouponCode) && cart.DiscountAmount.HasValue)
            {
                return $"Cupom {cart.CouponCode} aplicado! Desconto: R$ {cart.DiscountAmount.Value:N2}";
            }
            return string.Empty;
        }

        private static CartViewModel CreateEmptyCartViewModel()
        {
            return new CartViewModel
            {
                Cart = new Cart { Items = new List<CartItem>() },
                Items = new List<CartItem>(),
                Subtotal = 0,
                ShippingCost = 0,
                TaxAmount = 0,
                Total = 0
            };
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

        // POST: /Cart/SetShipping
        [HttpPost]
        // [RequireAuth] - Temporariamente removido para permitir frete sem login
        public IActionResult SetShipping([FromBody] SetShippingRequest request)
        {
            try
            {
                var sessionId = GetSessionId();
                
                // Usar o CartService para salvar o frete
                _cartService.SetShippingCost(sessionId, request.ShippingCost);
                
                var cart = _cartService.GetCart(sessionId);

                // Recalcular total
                var totals = _cartCalculationService.CalculateTotals(cart);

                return Json(new { 
                    success = true, 
                    message = "Frete selecionado com sucesso!",
                    shippingCost = totals.ShippingCost,
                    subtotal = totals.Subtotal,
                    taxAmount = totals.TaxAmount,
                    discountAmount = totals.DiscountAmount,
                    couponCode = cart.CouponCode,
                    total = totals.FinalTotal
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao definir frete");
                return Json(new { success = false, message = "Erro ao definir frete" });
            }
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
        public int? ProductId { get; set; }
        public int? VariationId { get; set; }
        public int? Quantity { get; set; }
    }

    public class SetShippingRequest
    {
        public decimal ShippingCost { get; set; }
    }

    public class CepInfo
    {
        public string Cep { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
    }
}
