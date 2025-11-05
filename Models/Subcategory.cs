using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class Subcategory
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Selecione uma categoria")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria válida")]
        public int CategoryId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; }

        public Category? Category { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
