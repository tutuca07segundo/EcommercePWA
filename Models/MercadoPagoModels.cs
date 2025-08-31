using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class MercadoPagoSettings
    {
        public string PublicKey { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public bool SandboxMode { get; set; } = true;
    }

    public class MercadoPagoPreferenceRequest
    {
        public string ExternalReference { get; set; } = string.Empty;
        public List<MercadoPagoItem> Items { get; set; } = new();
        public MercadoPagoPayer Payer { get; set; } = new();
        public MercadoPagoUrls BackUrls { get; set; } = new();
        public string AutoReturn { get; set; } = "approved";
        public string PaymentMethods { get; set; } = "credit_card,debit_card,pix";
        public int Expires { get; set; } = 30; // minutos
    }

    public class MercadoPagoItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PictureUrl { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string CurrencyId { get; set; } = "BRL";
        public decimal UnitPrice { get; set; }
    }

    public class MercadoPagoPayer
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public MercadoPagoPhone Phone { get; set; } = new();
        public MercadoPagoIdentification Identification { get; set; } = new();
        public MercadoPagoAddress Address { get; set; } = new();
    }

    public class MercadoPagoPhone
    {
        public string AreaCode { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
    }

    public class MercadoPagoIdentification
    {
        public string Type { get; set; } = "CPF";
        public string Number { get; set; } = string.Empty;
    }

    public class MercadoPagoAddress
    {
        public string StreetName { get; set; } = string.Empty;
        public string StreetNumber { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }

    public class MercadoPagoUrls
    {
        public string Success { get; set; } = string.Empty;
        public string Failure { get; set; } = string.Empty;
        public string Pending { get; set; } = string.Empty;
    }

    public class MercadoPagoPreferenceResponse
    {
        public string Id { get; set; } = string.Empty;
        public string InitPoint { get; set; } = string.Empty;
        public string SandboxInitPoint { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string ExternalReference { get; set; } = string.Empty;
    }

    public class MercadoPagoPaymentNotification
    {
        public string Action { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public MercadoPagoPaymentData Data { get; set; } = new();
        public DateTime DateCreated { get; set; }
        public long Id { get; set; }
        public bool LiveMode { get; set; }
        public string Type { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class MercadoPagoPaymentData
    {
        public string Id { get; set; } = string.Empty;
    }

    public class MercadoPagoPaymentDetails
    {
        public long Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDetail { get; set; } = string.Empty;
        public string ExternalReference { get; set; } = string.Empty;
        public string PaymentMethodId { get; set; } = string.Empty;
        public string PaymentTypeId { get; set; } = string.Empty;
        public decimal TransactionAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public string CurrencyId { get; set; } = "BRL";
        public DateTime DateCreated { get; set; }
        public DateTime? DateApproved { get; set; }
        public MercadoPagoPayer Payer { get; set; } = new();
    }
}
