using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public class OrderService : IOrderService
    {
        private readonly List<Order> _orders;
        private readonly List<OrderNotification> _notifications;
        private int _nextOrderId = 1;
        private int _nextNotificationId = 1;

        public OrderService()
        {
            _orders = new List<Order>();
            _notifications = new List<OrderNotification>();
        }

        public async Task<Order> CreateOrder(Order order)
        {
            await Task.Delay(100); // Simular operação assíncrona
            
            order.Id = _nextOrderId++;
            _orders.Add(order);
            
            // Criar notificação de confirmação
            await CreateNotification(order.Id, "Pedido Confirmado", 
                $"Seu pedido {order.OrderNumber} foi confirmado e está sendo processado.", "Success");
            
            return order;
        }

        public async Task<Order?> GetOrderById(int id)
        {
            await Task.Delay(50);
            return _orders.FirstOrDefault(o => o.Id == id);
        }

        public async Task<Order?> GetOrderByNumber(string orderNumber)
        {
            await Task.Delay(50);
            return _orders.FirstOrDefault(o => o.OrderNumber.Equals(orderNumber, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<Order>> GetOrdersByUserId(int userId)
        {
            await Task.Delay(50);
            return _orders.Where(o => o.UserId == userId).OrderByDescending(o => o.OrderDate).ToList();
        }

        public async Task<List<Order>> GetAllOrders()
        {
            await Task.Delay(50);
            return _orders.OrderByDescending(o => o.OrderDate).ToList();
        }

        public async Task<Order> UpdateOrderStatus(int orderId, string status)
        {
            await Task.Delay(100);
            
            var order = await GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("Pedido não encontrado");

            order.Status = status;
            
            // Criar notificação de mudança de status
            string message = status switch
            {
                "Processing" => "Seu pedido está sendo processado.",
                "Shipped" => "Seu pedido foi enviado!",
                "Delivered" => "Seu pedido foi entregue!",
                "Cancelled" => "Seu pedido foi cancelado.",
                _ => $"Status do pedido atualizado para: {status}"
            };

            await CreateNotification(orderId, $"Status Atualizado: {status}", message, "Info");
            
            return order;
        }

        public async Task<OrderNotification> CreateNotification(int orderId, string title, string message, string type)
        {
            await Task.Delay(50);
            
            var notification = new OrderNotification
            {
                Id = _nextNotificationId++,
                OrderId = orderId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            
            _notifications.Add(notification);
            return notification;
        }

        public async Task<List<OrderNotification>> GetNotificationsByOrderId(int orderId)
        {
            await Task.Delay(50);
            return _notifications.Where(n => n.OrderId == orderId).OrderByDescending(n => n.CreatedAt).ToList();
        }

        public async Task<List<OrderNotification>> GetUnreadNotificationsByUserId(int userId)
        {
            await Task.Delay(50);
            // Buscar notificações não lidas dos pedidos do usuário
            var userOrderIds = _orders.Where(o => o.UserId == userId).Select(o => o.Id);
            return _notifications.Where(n => userOrderIds.Contains(n.OrderId) && !n.IsRead)
                               .OrderByDescending(n => n.CreatedAt).ToList();
        }

        public async Task MarkNotificationAsRead(int notificationId)
        {
            await Task.Delay(50);
            
            var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }
        }
    }
}
