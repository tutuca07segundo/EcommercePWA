using System.ComponentModel.DataAnnotations;

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
        
        [StringLength(100)]
        public string ImageUrl { get; set; } = string.Empty;
        
        public int StockQuantity { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Periférico, Peça, etc.
        
        public List<string> Tags { get; set; } = new List<string>();
        
        public decimal Rating { get; set; } = 0;
        
        public int ReviewCount { get; set; } = 0;
    }
}
