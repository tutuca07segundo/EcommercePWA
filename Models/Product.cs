using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcommercePWA.Constants;

namespace EcommercePWA.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(BusinessConstants.MAX_NAME_LENGTH)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(BusinessConstants.MAX_DESCRIPTION_LENGTH)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(typeof(decimal), "0.01", "999999999")]
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
        
        // Propriedades para cálculo de frete
        [Range(typeof(decimal), "0.01", "50000")] // peso em gramas (máximo 50kg)
        public decimal Weight { get; set; } = BusinessConstants.DEFAULT_WEIGHT_GRAMS;
        
        [Range(1, BusinessConstants.MAX_DIMENSION_CM)] // largura em cm
        public int Width { get; set; } = BusinessConstants.DEFAULT_WIDTH_CM;
        
        [Range(1, BusinessConstants.MAX_DIMENSION_CM)] // altura em cm  
        public int Height { get; set; } = BusinessConstants.DEFAULT_HEIGHT_CM;
        
        [Range(1, BusinessConstants.MAX_DIMENSION_CM)] // comprimento em cm
        public int Length { get; set; } = BusinessConstants.DEFAULT_LENGTH_CM;
        
        public List<string> Tags { get; set; } = new List<string>();
        
        
        // Novos campos para variações de cor
        public virtual List<ProductVariation> Variations { get; set; } = new();
        
        // Navigation properties
        public virtual List<ProductDiscount> ProductDiscounts { get; set; } = new();
        public virtual Subcategory? Subcategory { get; set; }
        public virtual List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual List<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

        
        // Discount properties (not mapped to database)
        [NotMapped]
        public decimal? DiscountedPrice { get; set; }
        
        [NotMapped]
        public bool HasDiscount { get; set; }
    }
}
