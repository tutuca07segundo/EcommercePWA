namespace EcommercePWA.Models
{
    public class CartViewModel
    {
        public Cart Cart { get; set; } = null!;
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public string? CouponCode { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string? CouponMessage { get; set; }
        public bool IsCouponValid { get; set; }
    }
}
