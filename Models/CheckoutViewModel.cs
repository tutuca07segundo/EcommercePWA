using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = null!;
        
        // Informações de entrega
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100)]
        public string ShippingName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Endereço é obrigatório")]
        [StringLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Cidade é obrigatória")]
        [StringLength(100)]
        public string ShippingCity { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "CEP é obrigatório")]
        [StringLength(10)]
        public string ShippingZipCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "País é obrigatório")]
        [StringLength(100)]
        public string ShippingCountry { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Telefone é obrigatório")]
        [StringLength(20)]
        public string ShippingPhone { get; set; } = string.Empty;
        
        // Informações de pagamento
        [Required(ErrorMessage = "Método de pagamento é obrigatório")]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Número do cartão é obrigatório")]
        [StringLength(20)]
        public string CardNumber { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Nome no cartão é obrigatório")]
        [StringLength(100)]
        public string CardName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Data de validade é obrigatória")]
        [StringLength(5)]
        public string CardExpiry { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "CVV é obrigatório")]
        [StringLength(4)]
        public string CardCvv { get; set; } = string.Empty;
        
        // Opções
        public bool SavePaymentInfo { get; set; } = false;
        public bool SaveShippingInfo { get; set; } = false;
        public string? Notes { get; set; }
    }
}
