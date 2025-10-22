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

        // Propriedades adicionais para Mercado Pago
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string AddressNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        
        // Informações de pagamento
        [Required(ErrorMessage = "Método de pagamento é obrigatório")]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string CardNumber { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string CardName { get; set; } = string.Empty;
        
        [StringLength(5)]
        public string CardExpiry { get; set; } = string.Empty;
        
        [StringLength(4)]
        public string CardCvv { get; set; } = string.Empty;
        
        // PIX Information
        public string? PixCode { get; set; }
        public string? PixQRCodeBase64 { get; set; }
        public DateTime? PixExpirationTime { get; set; }
        public bool ShowPixQrCode { get; set; } = false;
        
        // Opções
        public bool SavePaymentInfo { get; set; } = false;
        public bool SaveShippingInfo { get; set; } = false;
        public string? Notes { get; set; }
    }
}
