using EcommercePWA.Models;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Services
{
    public class TrackingService : ITrackingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TrackingService> _logger;

        public TrackingService(ApplicationDbContext context, ILogger<TrackingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TrackingViewModel?> GetOrderTrackingAsync(string orderNumber)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                if (order == null)
                    return null;

                return CreateTrackingViewModel(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar rastreamento do pedido: {OrderNumber}", orderNumber);
                return null;
            }
        }

        public async Task<TrackingViewModel?> GetOrderTrackingByIdAsync(int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    return null;

                return CreateTrackingViewModel(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar rastreamento do pedido: {OrderId}", orderId);
                return null;
            }
        }

        private TrackingViewModel CreateTrackingViewModel(Order order)
        {
            var trackingSteps = GetTrackingSteps(order.Status, order.OrderDate, order.ShippedDate, order.DeliveredDate);
            var currentStepIndex = GetCurrentStepIndex(order.Status);

            return new TrackingViewModel
            {
                Order = order,
                TrackingSteps = trackingSteps,
                CurrentStatus = order.Status,
                CurrentStepIndex = currentStepIndex,
                EstimatedDeliveryDate = CalculateEstimatedDeliveryDate(order.OrderDate, order.Status),
                TrackingCode = GenerateTrackingCode(order.OrderNumber)
            };
        }

        public List<TrackingStep> GetTrackingSteps(string currentStatus, DateTime orderDate, DateTime? shippedDate, DateTime? deliveredDate)
        {
            var steps = new List<TrackingStep>
            {
                new TrackingStep
                {
                    Status = "Pending",
                    Title = "Pedido Realizado",
                    Description = "Seu pedido foi recebido e está sendo processado",
                    Icon = "fas fa-shopping-cart",
                    CompletedDate = orderDate,
                    IsCompleted = true,
                    Color = "success"
                },
                new TrackingStep
                {
                    Status = "Processing",
                    Title = "Pagamento Confirmado",
                    Description = "Pagamento aprovado e pedido em preparação",
                    Icon = "fas fa-credit-card",
                    Color = "info"
                },
                new TrackingStep
                {
                    Status = "Shipped",
                    Title = "Produto Enviado",
                    Description = "Seu pedido foi despachado e está a caminho",
                    Icon = "fas fa-truck",
                    CompletedDate = shippedDate,
                    Color = "warning"
                },
                new TrackingStep
                {
                    Status = "Delivered",
                    Title = "Entregue",
                    Description = "Pedido entregue com sucesso",
                    Icon = "fas fa-check-circle",
                    CompletedDate = deliveredDate,
                    Color = "success"
                }
            };

            var statusOrder = new[] { "Pending", "Processing", "Shipped", "Delivered" };
            var currentIndex = Array.IndexOf(statusOrder, currentStatus);

            for (int i = 0; i < steps.Count; i++)
            {
                if (i <= currentIndex)
                {
                    steps[i].IsCompleted = true;
                    if (i == currentIndex)
                    {
                        steps[i].IsActive = true;
                    }
                }
            }

            if (currentStatus == "Processing" && steps[1].CompletedDate == null)
            {
                steps[1].CompletedDate = DateTime.Now;
            }

            return steps;
        }

        private int GetCurrentStepIndex(string status)
        {
            return status switch
            {
                "Pending" => 0,
                "Processing" => 1,
                "Shipped" => 2,
                "Delivered" => 3,
                _ => 0
            };
        }

        public DateTime? CalculateEstimatedDeliveryDate(DateTime orderDate, string status)
        {
            return status switch
            {
                "Pending" => orderDate.AddDays(7),
                "Processing" => orderDate.AddDays(5),
                "Shipped" => orderDate.AddDays(2),
                "Delivered" => null,
                _ => orderDate.AddDays(7)
            };
        }

        public string GenerateTrackingCode(string orderNumber)
        {

            var hash = orderNumber.GetHashCode().ToString("X");
            return $"KB{hash.Substring(0, Math.Min(6, hash.Length)).ToUpper()}";
        }
    }
}
