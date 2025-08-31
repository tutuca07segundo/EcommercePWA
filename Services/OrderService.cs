using EcommercePWA.Models;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Order> CreateOrder(Order order)
        {
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await CreateNotification(order.Id, "Pedido Confirmado",
                    $"Seu pedido {order.OrderNumber} foi confirmado e está sendo processado.", "Success");

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido");
                throw;
            }
        }

        public async Task<Order?> GetOrderById(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.ProductVariation)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetOrderByNumber(string orderNumber)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<List<Order>> GetOrdersByUserId(int userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllOrders()
        {
            return await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                throw new ArgumentException("Pedido não encontrado");

            order.Status = status;
            if (status == "Shipped") order.ShippedDate = DateTime.Now;
            if (status == "Delivered") order.DeliveredDate = DateTime.Now;

            await _context.SaveChangesAsync();

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
            var notification = new OrderNotification
            {
                OrderId = orderId,
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.Now,
                IsRead = false
            };
            _context.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<OrderNotification>> GetNotificationsByOrderId(int orderId)
        {
            return await _context.Set<OrderNotification>()
                .Where(n => n.OrderId == orderId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<OrderNotification>> GetUnreadNotificationsByUserId(int userId)
        {
            return await _context.Set<OrderNotification>()
                .Include(n => n.Order)
                .Where(n => !n.IsRead && n.Order.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkNotificationAsRead(int notificationId)
        {
            var notification = await _context.Set<OrderNotification>().FirstOrDefaultAsync(n => n.Id == notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePaymentStatus(int orderId, string paymentStatus)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null)
            {
                order.PaymentStatus = paymentStatus;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Status de pagamento do pedido {orderId} atualizado para: {paymentStatus}");
            }
        }

        public async Task<Order> UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Pedido {order.Id} atualizado com sucesso");
            return order;
        }
    }
}
