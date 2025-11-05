using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class Discount
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome do Desconto")]
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
        
        [Required(ErrorMessage = "A data de início é obrigatória")]
        [Display(Name = "Válido de")]
        public DateTime ValidFrom { get; set; } = DateTime.Now;
        
        [Required(ErrorMessage = "A data de fim é obrigatória")]
        [Display(Name = "Válido até")]
        public DateTime ValidUntil { get; set; } = DateTime.Now.AddDays(30);
        
        [Display(Name = "Ativo")]
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public int? CreatedByUserId { get; set; }

        public List<ProductDiscount> ProductDiscounts { get; set; } = new List<ProductDiscount>();
    }
    
    public class ProductDiscount
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public int DiscountId { get; set; }
        public Discount Discount { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
