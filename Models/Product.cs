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
        [Range(0.01, 999999999)]
        public decimal Price { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "A marca é obrigatória")]
        [StringLength(50)]
        public string Brand { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
        
        public int StockQuantity { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Periférico, Peça, etc.
        
        // Propriedades para cálculo de frete
        [Range(0.01, 50000)] // peso em gramas (máximo 50kg)
        public decimal Weight { get; set; } = 200m; // 200g padrão (mais realista para periféricos)
        
        [Range(1, 200)] // largura em cm
        public int Width { get; set; } = 15; // 15cm padrão
        
        [Range(1, 200)] // altura em cm  
        public int Height { get; set; } = 10; // 10cm padrão
        
        [Range(1, 200)] // comprimento em cm
        public int Length { get; set; } = 20; // 20cm padrão
        
        public List<string> Tags { get; set; } = new List<string>();
        
        
        // Novos campos para variações de cor
        public virtual List<ProductVariation> Variations { get; set; } = new();
        
        // Navigation properties
        public virtual List<ProductDiscount> ProductDiscounts { get; set; } = new();
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
