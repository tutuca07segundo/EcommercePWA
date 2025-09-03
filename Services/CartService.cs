using EcommercePWA.Models;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        private readonly IDiscountService _discountService;
        private readonly ILogger<CartService> _logger;

        public CartService(ApplicationDbContext context, IProductService productService, ICouponService couponService, IDiscountService discountService, ILogger<CartService> logger)
        {
            _context = context;
            _productService = productService;
            _couponService = couponService;
            _discountService = discountService;
            _logger = logger;
        }

        public Cart GetCart(string sessionId)
        {
            try
            {
                var cart = _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Variations)
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.ProductVariation)
                    .FirstOrDefault(c => c.SessionId == sessionId);

                if (cart == null)
                {
                    // Extrair userId da sessionId se for um usuário logado
                    int? userId = null;
                    if (sessionId.StartsWith("user_"))
                    {
                        if (int.TryParse(sessionId.Replace("user_", ""), out int parsedUserId))
                        {
                            userId = parsedUserId;
                        }
                    }

                    cart = new Cart
                    {
                        SessionId = sessionId,
                        UserId = userId,
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now,
                        Items = new List<CartItem>()
                    };
                    _context.Carts.Add(cart);
                    _context.SaveChanges();
                }
                else
                {
                    // Atualizar preços com base nas variações
                    UpdateCartPrices(cart);
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter carrinho para sessão {SessionId}", sessionId);
                return new Cart
                {
                    SessionId = sessionId,
                    CreatedAt = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    Items = new List<CartItem>()
                };
            }
        }

        public async Task AddToCart(string sessionId, int productId, int? variationId, int quantity)
        {
            try
            {
                var cart = GetCart(sessionId);
                var product = await _productService.GetProductByIdAsync(productId);
                
                if (product == null)
                    throw new ArgumentException("Produto não encontrado");

                if (variationId.HasValue)
                {
                    var variation = product.Variations.FirstOrDefault(v => v.Id == variationId && v.IsAvailable);
                    if (variation == null)
                        throw new ArgumentException("Variação não disponível");
                }

                // Verificar se o item já existe no carrinho
                var existingItem = cart.Items.FirstOrDefault(i => 
                    i.ProductId == productId && i.ProductVariationId == variationId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.UnitPrice = GetItemPrice(product, variationId);
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        ProductVariationId = variationId,
                        Quantity = quantity,
                        UnitPrice = GetItemPrice(product, variationId),
                        AddedAt = DateTime.Now
                    };
                    cart.Items.Add(cartItem);
                }

                cart.LastUpdated = DateTime.Now;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar produto {ProductId} ao carrinho", productId);
                throw;
            }
        }

        public void UpdateQuantity(string sessionId, int itemId, int quantity)
        {
            try
            {
                var cart = GetCart(sessionId);
                var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
                
                if (item == null)
                    throw new ArgumentException("Item não encontrado no carrinho");

                if (quantity <= 0)
                {
                    RemoveItem(sessionId, itemId);
                }
                else
                {
                    // Obter o produto e a variação para garantir que o preço esteja correto
                    var product = _productService.GetProductByIdAsync(item.ProductId).Result;
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
                    
                    item.Quantity = quantity;
                    cart.LastUpdated = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar quantidade do item {ItemId}", itemId);
                throw;
            }
        }

        public void RemoveItem(string sessionId, int itemId)
        {
            try
            {
                var cart = GetCart(sessionId);
                var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
                
                if (item != null)
                {
                    cart.Items.Remove(item);
                    cart.LastUpdated = DateTime.Now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover item {ItemId} do carrinho", itemId);
                throw;
            }
        }

        public void ClearCart(string sessionId)
        {
            try
            {
                var cart = GetCart(sessionId);
                cart.Items.Clear();
                cart.LastUpdated = DateTime.Now;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar carrinho da sessão {SessionId}", sessionId);
                throw;
            }
        }

        public CouponValidationResult ApplyCoupon(string sessionId, string couponCode)
        {
            try
            {
                var cart = GetCart(sessionId);
                var coupon = _couponService.GetCouponByCode(couponCode);
                
                if (coupon == null)
                {
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = "Cupom não encontrado"
                    };
                }

                if (!coupon.IsActive)
                {
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = "Cupom inativo"
                    };
                }

                if (DateTime.Now < coupon.ValidFrom || DateTime.Now > coupon.ValidUntil)
                {
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = "Cupom fora da validade"
                    };
                }

                if (coupon.MaximumUses.HasValue && coupon.CurrentUses >= coupon.MaximumUses)
                {
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = "Cupom esgotado"
                    };
                }

                if (coupon.MinimumOrderAmount.HasValue && cart.Total < coupon.MinimumOrderAmount.Value)
                {
                    return new CouponValidationResult
                    {
                        IsValid = false,
                        Message = $"Valor mínimo do pedido: R$ {coupon.MinimumOrderAmount.Value:N2}"
                    };
                }

                decimal discountAmount = 0;
                if (coupon.DiscountPercentage.HasValue)
                {
                    discountAmount = cart.Total * (coupon.DiscountPercentage.Value / 100);
                }
                else
                {
                    discountAmount = coupon.DiscountAmount;
                }

                // Persistir cupom no carrinho
                cart.CouponCode = couponCode;
                cart.DiscountAmount = discountAmount;
                cart.LastUpdated = DateTime.Now;
                _context.SaveChanges();

                return new CouponValidationResult
                {
                    IsValid = true,
                    Message = $"Cupom aplicado com sucesso! Desconto: R$ {discountAmount:N2}",
                    DiscountAmount = discountAmount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aplicar cupom {CouponCode}", couponCode);
                return new CouponValidationResult
                {
                    IsValid = false,
                    Message = "Erro ao aplicar cupom"
                };
            }
        }

        public void RemoveCoupon(string sessionId)
        {
            try
            {
                var cart = GetCart(sessionId);
                cart.CouponCode = null;
                cart.DiscountAmount = null;
                cart.LastUpdated = DateTime.Now;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover cupom da sessão {SessionId}", sessionId);
                throw;
            }
        }

        private async Task<decimal> GetItemPriceAsync(Product product, int? variationId)
        {
            // Apply discount to base price
            var discount = await _discountService.GetActiveDiscountForProductAsync(product.Id);
            decimal basePrice = product.Price;
            
            if (discount != null)
            {
                basePrice = _discountService.CalculateDiscountedPrice(product.Price, discount);
            }
            
            // Apply variation modifier to discounted price
            if (variationId.HasValue)
            {
                var variation = product.Variations.FirstOrDefault(v => v.Id == variationId);
                if (variation?.PriceModifier.HasValue == true)
                {
                    return basePrice + variation.PriceModifier.Value;
                }
            }
            return basePrice;
        }

        private decimal GetItemPrice(Product product, int? variationId)
        {
            // Synchronous version for backward compatibility
            var discount = _discountService.GetActiveDiscountForProductAsync(product.Id).Result;
            decimal basePrice = product.Price;
            
            if (discount != null)
            {
                basePrice = _discountService.CalculateDiscountedPrice(product.Price, discount);
            }
            
            if (variationId.HasValue)
            {
                var variation = product.Variations.FirstOrDefault(v => v.Id == variationId);
                if (variation?.PriceModifier.HasValue == true)
                {
                    return basePrice + variation.PriceModifier.Value;
                }
            }
            return basePrice;
        }

        private void UpdateCartPrices(Cart cart)
        {
            bool needsSave = false;
            
            foreach (var item in cart.Items)
            {
                if (item.Product != null)
                {
                    decimal newPrice = GetItemPrice(item.Product, item.ProductVariationId);
                    if (item.UnitPrice != newPrice)
                    {
                        item.UnitPrice = newPrice;
                        needsSave = true;
                    }
                }
            }
            
            if (needsSave)
            {
                cart.LastUpdated = DateTime.Now;
                _context.SaveChanges();
            }
        }
    }
}
