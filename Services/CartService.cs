using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public class CartService : ICartService
    {
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        private readonly Dictionary<string, Cart> _carts = new Dictionary<string, Cart>();
        private int _nextCartId = 1;
        private int _nextCartItemId = 1;

        public CartService(IProductService productService, ICouponService couponService)
        {
            _productService = productService;
            _couponService = couponService;
        }

        public Cart GetCart(string sessionId)
        {
            if (!_carts.ContainsKey(sessionId))
            {
                _carts[sessionId] = new Cart
                {
                    Id = _nextCartId++,
                    SessionId = sessionId,
                    CreatedAt = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    Items = new List<CartItem>()
                };
            }
            return _carts[sessionId];
        }

        public void AddToCart(string sessionId, int productId, int? variationId, int quantity)
        {
            var cart = GetCart(sessionId);
            var product = _productService.GetProductById(productId);
            
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
                    Id = _nextCartItemId++,
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
        }

        public void UpdateQuantity(string sessionId, int itemId, int quantity)
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
                item.Quantity = quantity;
                cart.LastUpdated = DateTime.Now;
            }
        }

        public void RemoveItem(string sessionId, int itemId)
        {
            var cart = GetCart(sessionId);
            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            
            if (item != null)
            {
                cart.Items.Remove(item);
                cart.LastUpdated = DateTime.Now;
            }
        }

        public void ClearCart(string sessionId)
        {
            var cart = GetCart(sessionId);
            cart.Items.Clear();
            cart.LastUpdated = DateTime.Now;
        }

        public CouponValidationResult ApplyCoupon(string sessionId, string couponCode)
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

            return new CouponValidationResult
            {
                IsValid = true,
                Message = $"Cupom aplicado com sucesso! Desconto: R$ {discountAmount:N2}",
                DiscountAmount = discountAmount
            };
        }

        public void RemoveCoupon(string sessionId)
        {
            var cart = GetCart(sessionId);
            // Implementar remoção de cupom se necessário
            cart.LastUpdated = DateTime.Now;
        }

        private decimal GetItemPrice(Product product, int? variationId)
        {
            if (variationId.HasValue)
            {
                var variation = product.Variations.FirstOrDefault(v => v.Id == variationId);
                if (variation?.PriceModifier.HasValue == true)
                {
                    return product.Price + variation.PriceModifier.Value;
                }
            }
            return product.Price;
        }
    }
}
