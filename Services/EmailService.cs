using System.Net;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;
using RestSharp;

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

        public async Task<bool> SendSimpleAsync(string toEmail, string subject, string body)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO ENVIO DE E-MAIL ===");
                _logger.LogInformation("Para: {Email}", toEmail);
                _logger.LogInformation("Assunto: {Subject}", subject);

                // ÚNICA TENTATIVA: Gmail SMTP com credenciais reais
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
                    IsBodyHtml = false
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


