using EcommercePWA.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace EcommercePWA.Services
{
    public class MercadoPagoService : IMercadoPagoService
    {
        private readonly HttpClient _httpClient;
        private readonly MercadoPagoSettings _settings;
        private readonly ILogger<MercadoPagoService> _logger;
        private readonly IOrderService _orderService;
        private const string BaseUrl = "https://api.mercadopago.com";

        public MercadoPagoService(
            HttpClient httpClient, 
            IOptions<MercadoPagoSettings> settings,
            ILogger<MercadoPagoService> logger,
            IOrderService orderService)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            _orderService = orderService;
            
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.AccessToken}");
            _httpClient.DefaultRequestHeaders.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
        }

        public async Task<MercadoPagoPreferenceResponse?> CreatePreferenceAsync(MercadoPagoPreferenceRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{BaseUrl}/checkout/preferences", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var preference = JsonConvert.DeserializeObject<MercadoPagoPreferenceResponse>(responseContent);
                    
                    _logger.LogInformation($"Preferência criada com sucesso: {preference?.Id}");
                    return preference;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro ao criar preferência: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar preferência no Mercado Pago");
                return null;
            }
        }

        public async Task<MercadoPagoPaymentDetails?> GetPaymentDetailsAsync(string paymentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/v1/payments/{paymentId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var payment = JsonConvert.DeserializeObject<MercadoPagoPaymentDetails>(content);
                    return payment;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Erro ao buscar detalhes do pagamento: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao buscar pagamento {paymentId}");
                return null;
            }
        }

        public async Task<bool> ProcessWebhookAsync(MercadoPagoPaymentNotification notification)
        {
            try
            {
                _logger.LogInformation($"=== Webhook Recebido ===");
                _logger.LogInformation($"Tipo: {notification.Type}");
                _logger.LogInformation($"Action: {notification.Action}");
                _logger.LogInformation($"ID: {notification.Id}");
                _logger.LogInformation($"Live Mode: {notification.LiveMode}");
                
                if (notification.Type != "payment")
                {
                    _logger.LogInformation($"Webhook ignorado - tipo não é 'payment': {notification.Type}");
                    return true;
                }

                if (string.IsNullOrEmpty(notification.Data?.Id))
                {
                    _logger.LogError("Webhook sem ID de pagamento válido");
                    return false;
                }

                _logger.LogInformation($"Buscando detalhes do pagamento ID: {notification.Data.Id}");
                var paymentDetails = await GetPaymentDetailsAsync(notification.Data.Id);
                if (paymentDetails == null)
                {
                    _logger.LogError($"Falha ao buscar detalhes do pagamento: {notification.Data.Id}");
                    return false;
                }
                
                _logger.LogInformation($"Detalhes do pagamento - Status: {paymentDetails.Status}");
                _logger.LogInformation($"Status Detail: {paymentDetails.StatusDetail}");
                _logger.LogInformation($"Referência externa: {paymentDetails.ExternalReference}");
                _logger.LogInformation($"Valor: {paymentDetails.TransactionAmount} {paymentDetails.CurrencyId}");

                // Buscar pedido pela referência externa
                if (string.IsNullOrEmpty(paymentDetails.ExternalReference))
                {
                    _logger.LogError("Referência externa vazia no pagamento");
                    return false;
                }

                if (int.TryParse(paymentDetails.ExternalReference, out int orderId))
                {
                    _logger.LogInformation($"Buscando pedido com ID: {orderId}");
                    var order = await _orderService.GetOrderById(orderId);
                    if (order != null)
                    {
                        _logger.LogInformation($"Pedido encontrado. Status atual: {order.Status}");
                        
                        // Atualizar status do pedido baseado no status do pagamento
                        var (newStatus, paymentStatus) = paymentDetails.Status switch
                        {
                            "approved" => ("Confirmado", "Pago"),
                            "pending" => ("Aguardando Pagamento", "Pendente"),
                            "rejected" => ("Cancelado", "Rejeitado"),
                            "cancelled" => ("Cancelado", "Cancelado"),
                            "refunded" => ("Cancelado", "Reembolsado"),
                            _ => (order.Status, order.PaymentStatus)
                        };

                        _logger.LogInformation($"Novo status: {newStatus}, Payment Status: {paymentStatus}");

                        if (newStatus != order.Status || paymentStatus != order.PaymentStatus)
                        {
                            _logger.LogInformation($"Atualizando pedido {orderId}: Status '{order.Status}' -> '{newStatus}', Payment '{order.PaymentStatus}' -> '{paymentStatus}'");
                            
                            // Atualizar tanto o status do pedido quanto o status do pagamento
                            await _orderService.UpdateOrderStatus(orderId, newStatus);
                            await _orderService.UpdatePaymentStatus(orderId, paymentStatus);
                            
                            _logger.LogInformation($"Pedido {orderId} atualizado com sucesso");
                            
                            // Verificar se a atualização foi persistida
                            var updatedOrder = await _orderService.GetOrderById(orderId);
                            _logger.LogInformation($"Verificação: Status={updatedOrder.Status}, PaymentStatus={updatedOrder.PaymentStatus}");
                        }
                        else
                        {
                            _logger.LogInformation($"Pedido {orderId} já está atualizado");
                        }

                        return true;
                    }
                    else
                    {
                        _logger.LogError($"Pedido não encontrado: {orderId}");
                        return false;
                    }
                }
                else
                {
                    _logger.LogError($"Referência externa inválida: {paymentDetails.ExternalReference}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook do Mercado Pago");
                return false;
            }
        }

        public string GetCheckoutUrl(string preferenceId)
        {
            return _settings.SandboxMode 
                ? $"https://sandbox.mercadopago.com.br/checkout/v1/redirect?pref_id={preferenceId}"
                : $"https://www.mercadopago.com.br/checkout/v1/redirect?pref_id={preferenceId}";
        }
    }
}
