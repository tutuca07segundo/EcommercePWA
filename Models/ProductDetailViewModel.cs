namespace EcommercePWA.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;
        public List<ProductVariation> Variations { get; set; } = new List<ProductVariation>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Product> RelatedProducts { get; set; } = new List<Product>();
        public Review? UserReview { get; set; }
        public bool IsInWishlist { get; set; }
        public int? SelectedVariationId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
