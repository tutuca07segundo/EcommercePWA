using System.Text;
using System.Text.Json;
using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public class MelhorEnvioService : IMelhorEnvioService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MelhorEnvioService> _logger;
        private readonly string _apiToken;
        private readonly string _companyCep;
        private readonly string _baseUrl = "https://sandbox.melhorenvio.com.br/api/v2/me";

        public MelhorEnvioService(HttpClient httpClient, ILogger<MelhorEnvioService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiToken = configuration["MelhorEnvio:ApiToken"] ?? "";
            _companyCep = configuration["MelhorEnvio:CompanyCep"] ?? "01310-100"; // CEP da empresa
            
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "EcommercePWA/1.0");
        }

        public async Task<List<ShippingQuote>> CalculateShippingAsync(ShippingCalculationRequest request)
        {
            try
            {
                // Usar CEP da empresa se FromCep estiver vazio ou for o padrão
                var fromCep = string.IsNullOrEmpty(request.FromCep) || request.FromCep == "01310-100" 
                    ? _companyCep 
                    : request.FromCep;

                _logger.LogInformation($"Calculando frete de {fromCep} para {request.ToCep}");

                var payload = new
                {
                    from = new { postal_code = fromCep },
                    to = new { postal_code = request.ToCep },
                    products = request.Products.Select(p => new
                    {
                        id = p.Id.ToString(),
                        width = p.Width,
                        height = p.Height,
                        length = p.Length,
                        weight = p.Weight / 1000, // converter para kg
                        insurance_value = p.Value,
                        quantity = p.Quantity
                    }).ToArray()
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"Enviando requisição para API Melhor Envio...");
                _logger.LogInformation($"Payload: {json}");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/shipment/calculate", content);
                
                _logger.LogInformation($"Status da API: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var quotes = JsonSerializer.Deserialize<List<MelhorEnvioQuote>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });

                    return quotes?.Select(q => new ShippingQuote
                    {
                        Id = q.Id,
                        Name = q.Name,
                        Company = q.Company?.Name ?? "",
                        Price = q.Price,
                        DeliveryTime = q.DeliveryTime,
                        DeliveryRange = q.DeliveryRange,
                        Error = q.Error,
                        ErrorMessage = q.ErrorMessage ?? ""
                    }).ToList() ?? new List<ShippingQuote>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro na API Melhor Envio: {response.StatusCode} - {errorContent}");
                    return GetFallbackQuotes(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular frete via Melhor Envio");
                return GetFallbackQuotes(request);
            }
        }

        public async Task<bool> ValidateCepAsync(string cep)
        {
            try
            {
                var cleanCep = cep.Replace("-", "").Replace(".", "");
                var response = await _httpClient.GetAsync($"https://viacep.com.br/ws/{cleanCep}/json/");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return !content.Contains("\"erro\"");
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar CEP");
                return false;
            }
        }

        public async Task<TrackingInfo?> TrackPackageAsync(string trackingCode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/shipment/tracking?code={trackingCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var tracking = JsonSerializer.Deserialize<MelhorEnvioTracking>(content);
                    
                    return new TrackingInfo
                    {
                        Status = tracking?.Status ?? "",
                        Description = tracking?.Description ?? "",
                        Date = tracking?.Date ?? DateTime.Now,
                        Location = tracking?.Location ?? ""
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao rastrear encomenda");
                return null;
            }
        }

        private List<ShippingQuote> GetFallbackQuotes(ShippingCalculationRequest request)
        {
            // Fallback com cotações simuladas REALISTAS baseadas no peso total
            var totalWeight = request.Products.Sum(p => p.Weight * p.Quantity); // em gramas
            var totalValue = request.Products.Sum(p => p.Value * p.Quantity);

            _logger.LogInformation($"Fallback: Peso total = {totalWeight}g, Valor total = R$ {totalValue:N2}");

            var quotes = new List<ShippingQuote>();

            // PAC simulado - preços realistas
            quotes.Add(new ShippingQuote
            {
                Id = 1,
                Name = "PAC",
                Company = "Correios",
                Price = CalculateFallbackPrice(totalWeight, totalValue, 1.0m),
                DeliveryTime = 8,
                DeliveryRange = "8 a 12 dias úteis"
            });

            // SEDEX simulado - preços realistas
            quotes.Add(new ShippingQuote
            {
                Id = 2,
                Name = "SEDEX",
                Company = "Correios",
                Price = CalculateFallbackPrice(totalWeight, totalValue, 1.8m),
                DeliveryTime = 3,
                DeliveryRange = "3 a 5 dias úteis"
            });

            return quotes;
        }

        private decimal CalculateFallbackPrice(decimal totalWeightGrams, decimal totalValue, decimal multiplier)
        {
            // Cálculo REALISTA de frete
            // Base: R$ 0.015 por grama (R$ 15 por kg) - mais próximo da realidade
            var basePrice = (totalWeightGrams * 0.015m);
            
            // Seguro: apenas 0.5% do valor (não 2%)
            var valueInsurance = totalValue * 0.005m;
            
            // Preço mínimo de R$ 12 (mais realista que R$ 15)
            var calculatedPrice = (basePrice + valueInsurance) * multiplier;
            
            _logger.LogInformation($"Fallback calculado: Base={basePrice:N2}, Seguro={valueInsurance:N2}, Multiplicador={multiplier}, Total={calculatedPrice:N2}");
            
            return Math.Max(12m, Math.Min(calculatedPrice, 150m)); // Máximo de R$ 150 para evitar absurdos
        }
    }

    // Classes auxiliares para deserialização
    public class MelhorEnvioQuote
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int DeliveryTime { get; set; }
        public string DeliveryRange { get; set; } = "";
        public bool Error { get; set; }
        public string? ErrorMessage { get; set; }
        public MelhorEnvioCompany? Company { get; set; }
    }

    public class MelhorEnvioCompany
    {
        public string Name { get; set; } = "";
    }

    public class MelhorEnvioTracking
    {
        public string Status { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Date { get; set; }
        public string Location { get; set; } = "";
    }
}
