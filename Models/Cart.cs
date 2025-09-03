using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class Cart
    {
        public int Id { get; set; }
        
        public int? UserId { get; set; } // Null para usuários não logados
        public User? User { get; set; }
        
        [StringLength(100)]
        public string SessionId { get; set; } = string.Empty; // Para usuários não logados
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        
        // Cupom aplicado
        [StringLength(50)]
        public string? CouponCode { get; set; }
        public decimal? DiscountAmount { get; set; }
        
        // Relacionamentos
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        
        // Propriedades calculadas
        public decimal Subtotal => Items.Sum(item => item.Total);
        public decimal Total => Subtotal - (DiscountAmount ?? 0);
        public int ItemCount => Items.Sum(item => item.Quantity);
    }
}
