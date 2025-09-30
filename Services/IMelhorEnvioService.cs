using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface IMelhorEnvioService
    {
        Task<List<ShippingQuote>> CalculateShippingAsync(ShippingCalculationRequest request);
        Task<bool> ValidateCepAsync(string cep);
        Task<TrackingInfo?> TrackPackageAsync(string trackingCode);
    }

    public class ShippingCalculationRequest
    {
        public string FromCep { get; set; } = "01310-100"; // CEP de origem (padrão SP)
        public string ToCep { get; set; } = string.Empty;
        public List<ShippingProduct> Products { get; set; } = new();
    }

    public class ShippingProduct
    {
        public int Id { get; set; }
        public decimal Weight { get; set; } // em gramas
        public int Width { get; set; } // em cm
        public int Height { get; set; } // em cm
        public int Length { get; set; } // em cm
        public int Quantity { get; set; }
        public decimal Value { get; set; } // valor do produto
    }

    public class ShippingQuote
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DeliveryTime { get; set; } // em dias úteis
        public string DeliveryRange { get; set; } = string.Empty;
        public bool Error { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class TrackingInfo
    {
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
