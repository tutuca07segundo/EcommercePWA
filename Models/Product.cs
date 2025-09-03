using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommercePWA.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Brand { get; set; } = string.Empty;
        
        public int? SubcategoryId { get; set; }
        
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
        
        public int StockQuantity { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Periférico, Peça, etc.
        
        public List<string> Tags { get; set; } = new List<string>();
        
        public decimal Rating { get; set; } = 0;
        
        // Novos campos para variações de cor
        public virtual List<ProductVariation> Variations { get; set; } = new();
        
        // Navigation properties
        public virtual List<Review> Reviews { get; set; } = new();
        public virtual List<ProductDiscount> ProductDiscounts { get; set; } = new();
        public virtual Subcategory? Subcategory { get; set; }
        public virtual List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual List<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        // Computed properties
        public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;
        public int ReviewCount => Reviews.Count;
        
        // Discount properties (not mapped to database)
        [NotMapped]
        public decimal? DiscountedPrice { get; set; }
        
        [NotMapped]
        public bool HasDiscount { get; set; }
    }
}
