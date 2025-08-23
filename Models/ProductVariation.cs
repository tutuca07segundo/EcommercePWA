using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class ProductVariation
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty; // Ex: "Vermelho", "Azul", "Pequeno", "Grande"
        
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // Ex: "Cor", "Tamanho", "Modelo"
        
        [StringLength(7)]
        public string ColorHex { get; set; } = string.Empty; // Para variações de cor
        
        public decimal? PriceModifier { get; set; } // Diferença de preço da variação
        
        public int StockQuantity { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        [StringLength(100)]
        public string ImageUrl { get; set; } = string.Empty; // Imagem específica da variação
        
        public int DisplayOrder { get; set; }
    }
}
