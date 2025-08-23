using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrder(Order order);
        Task<Order?> GetOrderById(int id);
        Task<Order?> GetOrderByNumber(string orderNumber);
        Task<List<Order>> GetOrdersByUserId(int userId);
        Task<List<Order>> GetAllOrders();
        Task<Order> UpdateOrderStatus(int orderId, string status);
        Task<OrderNotification> CreateNotification(int orderId, string title, string message, string type);
        Task<List<OrderNotification>> GetNotificationsByOrderId(int orderId);
        Task<List<OrderNotification>> GetUnreadNotificationsByUserId(int userId);
        Task MarkNotificationAsRead(int notificationId);
    }
}
