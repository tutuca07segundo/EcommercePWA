using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        
        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public int? ProductVariationId { get; set; }
        public ProductVariation? ProductVariation { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        public decimal UnitPrice { get; set; }
        
        public decimal Total => UnitPrice * Quantity;
        
        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}
