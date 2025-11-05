using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class CategoryPromotion
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome da Promoção")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres")]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }
        
        [Display(Name = "Valor do Desconto (R$)")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor deve ser maior ou igual a zero")]
        public decimal? DiscountAmount { get; set; } // Valor fixo do desconto
        
        [Display(Name = "Percentual de Desconto (%)")]
        [Range(0, 100, ErrorMessage = "O percentual deve estar entre 0 e 100")]
        public decimal? DiscountPercentage { get; set; } // Percentual de desconto
        
        [Required(ErrorMessage = "A categoria é obrigatória")]
        [Display(Name = "Categoria")]
        public string CategoryName { get; set; } = string.Empty; // Nome da categoria (ex: "Headset", "Mouse", etc.)
        
        [Required(ErrorMessage = "A data de início é obrigatória")]
        [Display(Name = "Válido de")]
        public DateTime ValidFrom { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "A data de fim é obrigatória")]
        [Display(Name = "Válido até")]
        public DateTime ValidUntil { get; set; } = DateTime.Now.AddDays(30);
        
        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Prioridade")]
        [Range(1, 10, ErrorMessage = "A prioridade deve estar entre 1 e 10")]
        public int Priority { get; set; } = 1; // Para casos onde há múltiplas promoções
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public int? CreatedByUserId { get; set; }

        public bool IsValidNow()
        {
            var now = DateTime.Now;
            return IsActive && now >= ValidFrom && now <= ValidUntil;
        }

        public decimal CalculateDiscount(decimal originalPrice)
        {
            if (!IsValidNow()) return 0;
            
            if (DiscountAmount.HasValue)
            {
                return Math.Min(DiscountAmount.Value, originalPrice);
            }
            
            if (DiscountPercentage.HasValue)
            {
                return originalPrice * (DiscountPercentage.Value / 100);
            }
            
            return 0;
        }
    }
}
