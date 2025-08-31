using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface IMercadoPagoService
    {
        Task<MercadoPagoPreferenceResponse?> CreatePreferenceAsync(MercadoPagoPreferenceRequest request);
        Task<MercadoPagoPaymentDetails?> GetPaymentDetailsAsync(string paymentId);
        Task<bool> ProcessWebhookAsync(MercadoPagoPaymentNotification notification);
        string GetCheckoutUrl(string preferenceId);
    }
}
