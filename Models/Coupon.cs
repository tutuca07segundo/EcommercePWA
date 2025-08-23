using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;
        
        public decimal DiscountAmount { get; set; } // Valor fixo do desconto
        
        public decimal? DiscountPercentage { get; set; } // Percentual de desconto
        
        public decimal? MinimumOrderAmount { get; set; } // Valor mínimo do pedido
        
        public int? MaximumUses { get; set; } // Número máximo de usos
        
        public int CurrentUses { get; set; } = 0;
        
        public DateTime ValidFrom { get; set; }
        
        public DateTime ValidUntil { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public int? CreatedByUserId { get; set; } // Admin que criou o cupom
        
        // Relacionamentos
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
