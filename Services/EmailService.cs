using System.Net;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using RestSharp;
using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody)
        {
            return await SendSimpleAsync(toEmail, subject, htmlBody);
        }

        public async Task<bool> SendOrderConfirmationAsync(string toEmail, Order order)
        {
            try
            {
                var subject = $"Kaboom - Confirmação do Pedido #{order.OrderNumber}";
                var htmlBody = GenerateOrderConfirmationHtml(order);
                
                return await SendAsync(toEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar confirmação do pedido para {Email}", toEmail);
                return false;
            }
        }

        private string GenerateOrderConfirmationHtml(Order order)
        {
            var html = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                        .content {{ padding: 30px; }}
                        .order-info {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .item {{ border-bottom: 1px solid #eee; padding: 15px 0; }}
                        .item:last-child {{ border-bottom: none; }}
                        .total {{ background: #e8f5e8; padding: 15px; border-radius: 8px; font-weight: bold; color: #28a745; }}
                        .footer {{ background: #333; color: white; padding: 20px; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>⚡ Kaboom</h1>
                            <h2>Pedido Confirmado!</h2>
                            <p>Obrigado por escolher a Kaboom</p>
                        </div>
                        
                        <div class='content'>
                            <div class='order-info'>
                                <h3>Detalhes do Pedido</h3>
                                <p><strong>Número do Pedido:</strong> {order.OrderNumber}</p>
                                <p><strong>Data:</strong> {order.OrderDate:dd/MM/yyyy 'às' HH:mm}</p>
                                <p><strong>Status:</strong> {order.Status}</p>
                                <p><strong>Método de Pagamento:</strong> {order.PaymentMethod}</p>
                            </div>

                            <h3>Itens do Pedido</h3>";

            foreach (var item in order.Items)
            {
                html += $@"
                            <div class='item'>
                                <strong>{item.ProductName}</strong><br>
                                <small>Quantidade: {item.Quantity} | Preço: R$ {item.UnitPrice:N2}</small><br>
                                <strong>Total: R$ {item.Total:N2}</strong>
                            </div>";
            }

            html += $@"
                            <div class='order-info'>
                                <h3>Endereço de Entrega</h3>
                                <p>{order.ShippingAddress}<br>
                                {order.ShippingCity} - {order.ShippingZipCode}<br>
                                {order.ShippingCountry}</p>
                            </div>

                            <div class='total'>
                                <h3>Total do Pedido: R$ {order.Total:N2}</h3>
                            </div>

                            <p style='margin-top: 30px;'>
                                <strong>Próximos passos:</strong><br>
                                • Você receberá atualizações sobre o status do pedido<br>
                                • O rastreamento estará disponível quando o produto for enviado<br>
                                • Em caso de dúvidas, entre em contato conosco
                            </p>
                        </div>
                        
                        <div class='footer'>
                            <p>© 2025 Kaboom - E-commerce de Periféricos e Peças de Computador</p>
                            <p>Este é um email automático, não responda.</p>
                        </div>
                    </div>
                </body>
                </html>";

            return html;
        }

        public async Task<bool> SendSimpleAsync(string toEmail, string subject, string body)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO ENVIO DE E-MAIL ===");
                _logger.LogInformation("Para: {Email}", toEmail);
                _logger.LogInformation("Assunto: {Subject}", subject);

                var result = await SendWithGmailSmtp(toEmail, subject, body);
                if (result)
                {
                    _logger.LogInformation("SUCESSO: E-mail enviado via Gmail SMTP!");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Falha ao enviar e-mail via Gmail SMTP");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar e-mail para {Email}", toEmail);
                return false;
            }
        }

        private async Task<bool> SendWithGmailSmtp(string toEmail, string subject, string body)
        {
            try
            {
                var host = _config["Smtp:Host"];
                var port = _config["Smtp:Port"];
                var user = _config["Smtp:User"];
                var pass = _config["Smtp:Pass"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                {
                    _logger.LogInformation("Configuração SMTP não encontrada no appsettings.json");
                    return false;
                }

                _logger.LogInformation("Tentando envio via SMTP configurado...");
                _logger.LogInformation("Host: {Host}, Port: {Port}, User: {User}", host, port, user);
                
                var portNum = int.Parse(port?.ToString() ?? "587");
                var enableSsl = bool.Parse(_config["Smtp:EnableSsl"]?.ToString() ?? "true");
                
                using var client = new SmtpClient(host, portNum)
                {
                    Credentials = new NetworkCredential(user, pass),
                    EnableSsl = enableSsl,
                    Timeout = 15000
                };

                var fromEmail = _config["Smtp:From"] ?? user;
                var mail = new MailMessage(fromEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = body.Contains("<html>")
                };

                _logger.LogInformation("Enviando mensagem...");
                await client.SendMailAsync(mail);
                _logger.LogInformation("SUCESSO: E-mail enviado via SMTP configurado para {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("SMTP configurado falhou: {Message}", ex.Message);
                return false;
            }
        }
    }
}

