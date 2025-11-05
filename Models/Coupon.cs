using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O código é obrigatório")]
        [StringLength(50, ErrorMessage = "O código deve ter no máximo 50 caracteres")]
        [Display(Name = "Código")]
        public string Code { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(100, ErrorMessage = "A descrição deve ter no máximo 100 caracteres")]
        [Display(Name = "Descrição")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Valor do Desconto (R$)")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor deve ser maior ou igual a zero")]
        public decimal DiscountAmount { get; set; } // Valor fixo do desconto
        
        [Display(Name = "Percentual de Desconto (%)")]
        [Range(0, 100, ErrorMessage = "O percentual deve estar entre 0 e 100")]
        public decimal? DiscountPercentage { get; set; } // Percentual de desconto
        
        [Display(Name = "Valor Mínimo do Pedido (R$)")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor deve ser maior ou igual a zero")]
        public decimal? MinimumOrderAmount { get; set; } // Valor mínimo do pedido
        
        [Display(Name = "Máximo de Usos")]
        [Range(0, int.MaxValue, ErrorMessage = "O máximo de usos deve ser maior ou igual a zero")]
        public int? MaximumUses { get; set; } // Número máximo de usos (0 = ilimitado)
        
        public int CurrentUses { get; set; } = 0;
        
        [Required(ErrorMessage = "A data de início é obrigatória")]
        [Display(Name = "Válido de")]
        public DateTime ValidFrom { get; set; }
        
        [Required(ErrorMessage = "A data de fim é obrigatória")]
        [Display(Name = "Válido até")]
        public DateTime ValidUntil { get; set; }
        
        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public int? CreatedByUserId { get; set; } // Admin que criou o cupom

        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
