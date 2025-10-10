namespace EcommercePWA.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;
        public List<ProductVariation> Variations { get; set; } = new List<ProductVariation>();
        public List<Product> RelatedProducts { get; set; } = new List<Product>();
        public bool IsInWishlist { get; set; }
        public int? SelectedVariationId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
