namespace EcommercePWA.Services
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string toEmail, string subject, string htmlBody);
        Task<bool> SendSimpleAsync(string toEmail, string subject, string body);
    }
}


