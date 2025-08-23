using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface ICartService
    {
        Cart GetCart(string sessionId);
        void AddToCart(string sessionId, int productId, int? variationId, int quantity);
        void UpdateQuantity(string sessionId, int itemId, int quantity);
        void RemoveItem(string sessionId, int itemId);
        void ClearCart(string sessionId);
        CouponValidationResult ApplyCoupon(string sessionId, string couponCode);
        void RemoveCoupon(string sessionId);
    }

    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
    }
}
