using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public int? ProductVariationId { get; set; }
        public ProductVariation? ProductVariation { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ProductName { get; set; } = string.Empty;
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public decimal Total => UnitPrice * Quantity;
        
        [StringLength(50)]
        public string? VariationName { get; set; } // Nome da variação selecionada
    }
}
