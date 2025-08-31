using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string toEmail, string subject, string htmlBody);
        Task<bool> SendSimpleAsync(string toEmail, string subject, string body);
        Task<bool> SendOrderConfirmationAsync(string toEmail, Order order);
    }
}
