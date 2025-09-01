using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

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
        [JsonProperty("external_reference")]
        public string ExternalReference { get; set; } = string.Empty;
        
        [JsonProperty("items")]
        public List<MercadoPagoItem> Items { get; set; } = new();
        
        [JsonProperty("back_urls")]
        public MercadoPagoUrls BackUrls { get; set; } = new();
        
        [JsonProperty("auto_return")]
        public string AutoReturn { get; set; } = "approved";
    }

    public class MercadoPagoItem
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonProperty("picture_url")]
        public string PictureUrl { get; set; } = string.Empty;
        
        [JsonProperty("category_id")]
        public string CategoryId { get; set; } = string.Empty;
        
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        
        [JsonProperty("currency_id")]
        public string CurrencyId { get; set; } = "BRL";
        
        [JsonProperty("unit_price")]
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
        [JsonProperty("success")]
        public string Success { get; set; } = string.Empty;
        
        [JsonProperty("failure")]
        public string Failure { get; set; } = string.Empty;
        
        [JsonProperty("pending")]
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
        public string Id { get; set; } = string.Empty;
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
