using EcommercePWA.Models;
using EcommercePWA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EcommercePWA.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class MercadoPagoWebhookController : ControllerBase
    {
        private readonly IMercadoPagoService _mercadoPagoService;
        private readonly ILogger<MercadoPagoWebhookController> _logger;
        private readonly string _webhookSecret;

        public MercadoPagoWebhookController(
            IMercadoPagoService mercadoPagoService,
            IOptions<MercadoPagoSettings> settings,
            ILogger<MercadoPagoWebhookController> logger)
        {
            _mercadoPagoService = mercadoPagoService;
            _logger = logger;
            _webhookSecret = settings.Value.WebhookSecret;
        }

        [HttpGet]
        [ProducesResponseType(typeof(string), 200)]
        public IActionResult TestWebhook()
        {
            _logger.LogInformation("Webhook GET - Health check do Mercado Pago");
            return Ok("Webhook endpoint funcionando");
        }

        [HttpPost("ReceiveWebhook")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ReceiveWebhook()
        {
            try
            {
                _logger.LogInformation("=== Webhook MercadoPago Recebido ===");

                foreach (var header in Request.Headers)
                {
                    _logger.LogInformation($"Header: {header.Key} = {header.Value}");
                }

                using var reader = new StreamReader(Request.Body, Encoding.UTF8);
                var requestBody = await reader.ReadToEndAsync();
                
                _logger.LogInformation($"Webhook Body: {requestBody}");

                if (Request.Headers.TryGetValue("x-signature", out var signatureHeader))
                {
                    var signature = signatureHeader.ToString();
                    _logger.LogInformation($"Assinatura recebida: {signature}");

                    if (!string.IsNullOrEmpty(_webhookSecret) && _webhookSecret != "your_webhook_secret_here")
                    {
                        var signatureValue = signature.Contains('=') ? signature.Split('=')[1] : signature;
                        if (!IsValidSignature(requestBody, signatureValue, _webhookSecret))
                        {
                            _logger.LogWarning("Assinatura de webhook inválida");
                            return Unauthorized("Assinatura inválida");
                        }
                        _logger.LogInformation("Assinatura validada com sucesso");
                    }
                    else
                    {
                        _logger.LogInformation("Validação de assinatura desabilitada para desenvolvimento");
                    }
                }
                else
                {
                    _logger.LogWarning("Webhook recebido sem assinatura x-signature");
                }

                var notification = JsonSerializer.Deserialize<MercadoPagoPaymentNotification>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (notification == null)
                {
                    _logger.LogError("Falha ao desserializar notificação do Mercado Pago");
                    return BadRequest("Formato de notificação inválido");
                }

                _logger.LogInformation($"Notificação processada: Tipo={notification.Type}, Action={notification.Action}, ID={notification.Id}");

                var success = await _mercadoPagoService.ProcessWebhookAsync(notification);
                if (!success)
                {
                    _logger.LogError($"Falha ao processar notificação {notification.Id}");
                    return StatusCode(500, "Erro ao processar notificação");
                }

                _logger.LogInformation($"Webhook processado com sucesso: {notification.Id}");
                return Ok("Webhook processado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook do Mercado Pago");
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        private static bool IsValidSignature(string payload, string signature, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var calculatedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return calculatedSignature == signature.ToLower();
        }
    }
}
