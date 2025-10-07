using System.ComponentModel.DataAnnotations;
using EcommercePWA.Constants;

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
        [StringLength(BusinessConstants.MAX_COUPON_CODE_LENGTH)]
        public string? CouponCode { get; set; }
        public decimal? DiscountAmount { get; set; }
        
        // Frete selecionado
        public decimal? ShippingCost { get; set; }
        
        // Relacionamentos
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        
        // Propriedades calculadas
        public decimal Subtotal => Items.Sum(item => item.Total);
        public decimal Total => Subtotal - (DiscountAmount ?? 0) + (ShippingCost ?? 0) + (Subtotal * BusinessConstants.TAX_RATE);
        public int ItemCount => Items.Sum(item => item.Quantity);
    }
}
