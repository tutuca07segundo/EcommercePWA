using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface ITrackingService
    {
        Task<TrackingViewModel?> GetOrderTrackingAsync(string orderNumber);
        Task<TrackingViewModel?> GetOrderTrackingByIdAsync(int orderId);
        List<TrackingStep> GetTrackingSteps(string currentStatus, DateTime orderDate, DateTime? shippedDate, DateTime? deliveredDate);
        DateTime? CalculateEstimatedDeliveryDate(DateTime orderDate, string status);
        string GenerateTrackingCode(string orderNumber);
    }
}
