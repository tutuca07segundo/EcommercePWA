namespace EcommercePWA.Models
{

    public class ProductWithPricing
    {
        public Product Product { get; set; } = new();
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public bool HasDiscount => DiscountAmount.HasValue && DiscountAmount > 0;
        public decimal? DiscountPercentage { get; set; }
        public string? DiscountSource { get; set; } // "Product", "Category", etc.
    }
}
