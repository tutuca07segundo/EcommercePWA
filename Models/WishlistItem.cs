using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public int? ProductVariationId { get; set; }
        public ProductVariation? ProductVariation { get; set; }
        
        public DateTime AddedAt { get; set; } = DateTime.Now;
        
        [StringLength(200)]
        public string? Notes { get; set; } // Notas pessoais do usuário
    }
}
