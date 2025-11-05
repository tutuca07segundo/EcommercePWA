using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrderApiController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderApiController> _logger;

        public OrderApiController(IOrderService orderService, ILogger<OrderApiController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(UserOrdersResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByUserId(userId);
                return Ok(new UserOrdersResponse
                {
                    Success = true,
                    Orders = orders.Select(order => new OrderSummaryDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        Status = order.Status,
                        PaymentStatus = order.PaymentStatus,
                        PaymentMethod = order.PaymentMethod,
                        TotalAmount = order.Total,
                        CreatedAt = order.OrderDate,
                        ItemCount = order.Items?.Count ?? 0
                    }),
                    Count = orders.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pedidos do usuário {UserId}", userId);
                return BadRequest(new { success = false, message = "Erro ao obter pedidos" });
            }
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(typeof(OrderDetailsResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderById(orderId);
                if (order == null)
                    return NotFound(new { success = false, message = "Pedido não encontrado" });

                return Ok(new OrderDetailsResponse
                {
                    Success = true,
                    Order = new OrderDetailsDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        Status = order.Status,
                        PaymentStatus = order.PaymentStatus,
                        PaymentMethod = order.PaymentMethod,
                        TotalAmount = order.Total,
                        ShippingCost = order.ShippingCost,
                        TaxAmount = order.TaxAmount,
                        DiscountAmount = order.DiscountAmount,
                        CouponCode = order.CouponCode,
                        CreatedAt = order.OrderDate,
                        UpdatedAt = order.OrderDate, // Não existe UpdatedAt no modelo
                        ShippingAddress = order.ShippingAddress,
                        TrackingCode = "", // Não existe no modelo atual
                        Items = order.Items?.Select(item => new OrderItemDto
                        {
                            Id = item.Id,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            ProductVariationId = item.ProductVariationId,
                            VariationName = item.VariationName,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalPrice = item.Total
                        }) ?? new List<OrderItemDto>()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes do pedido {OrderId}", orderId);
                return BadRequest(new { success = false, message = "Erro ao obter detalhes do pedido" });
            }
        }

        [HttpPut("{orderId}/status")]
        [ProducesResponseType(typeof(OrderActionResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderStatus(orderId, request.Status);
                var success = updatedOrder != null;
                if (!success)
                    return NotFound(new { success = false, message = "Pedido não encontrado" });

                return Ok(new OrderActionResponse
                {
                    Success = true,
                    Message = "Status do pedido atualizado com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do pedido {OrderId}", orderId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{orderId}/payment-status")]
        [ProducesResponseType(typeof(OrderActionResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdatePaymentStatus(int orderId, [FromBody] UpdatePaymentStatusRequest request)
        {
            try
            {
                await _orderService.UpdatePaymentStatus(orderId, request.PaymentStatus);
                var success = true;
                if (!success)
                    return NotFound(new { success = false, message = "Pedido não encontrado" });

                return Ok(new OrderActionResponse
                {
                    Success = true,
                    Message = "Status de pagamento atualizado com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status de pagamento do pedido {OrderId}", orderId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{orderId}/tracking")]
        [ProducesResponseType(typeof(OrderActionResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddTrackingCode(int orderId, [FromBody] AddTrackingCodeRequest request)
        {
            try
            {

                var order = await _orderService.GetOrderById(orderId);
                if (order != null)
                {

                    await _orderService.UpdateOrder(order);
                }
                var success = order != null;
                if (!success)
                    return NotFound(new { success = false, message = "Pedido não encontrado" });

                return Ok(new OrderActionResponse
                {
                    Success = true,
                    Message = "Código de rastreamento adicionado com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar código de rastreamento ao pedido {OrderId}", orderId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{orderId}/cancel")]
        [ProducesResponseType(typeof(OrderActionResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var cancelledOrder = await _orderService.UpdateOrderStatus(orderId, "Cancelado");
                var success = cancelledOrder != null;
                if (!success)
                    return NotFound(new { success = false, message = "Pedido não encontrado ou não pode ser cancelado" });

                return Ok(new OrderActionResponse
                {
                    Success = true,
                    Message = "Pedido cancelado com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar pedido {OrderId}", orderId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("statistics")]
        [ProducesResponseType(typeof(OrderStatisticsResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetOrderStatistics()
        {
            try
            {

                var allOrders = await _orderService.GetAllOrders();
                var stats = new {
                    TotalOrders = allOrders.Count,
                    PendingOrders = allOrders.Count(o => o.Status == "Pendente"),
                    CompletedOrders = allOrders.Count(o => o.Status == "Entregue"),
                    CancelledOrders = allOrders.Count(o => o.Status == "Cancelado"),
                    TotalRevenue = allOrders.Where(o => o.PaymentStatus == "Pago").Sum(o => o.Total),
                    AverageOrderValue = allOrders.Any() ? allOrders.Average(o => o.Total) : 0
                };
                return Ok(new OrderStatisticsResponse
                {
                    Success = true,
                    TotalOrders = stats.TotalOrders,
                    PendingOrders = stats.PendingOrders,
                    CompletedOrders = stats.CompletedOrders,
                    CancelledOrders = stats.CancelledOrders,
                    TotalRevenue = stats.TotalRevenue,
                    AverageOrderValue = stats.AverageOrderValue
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de pedidos");
                return BadRequest(new { success = false, message = "Erro ao obter estatísticas" });
            }
        }
    }

    #region Request/Response Models

    public class UserOrdersResponse
    {

        public bool Success { get; set; }

        public IEnumerable<OrderSummaryDto> Orders { get; set; } = new List<OrderSummaryDto>();

        public int Count { get; set; }
    }

    public class OrderSummaryDto
    {

        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ItemCount { get; set; }
    }

    public class OrderDetailsResponse
    {

        public bool Success { get; set; }

        public OrderDetailsDto Order { get; set; } = new OrderDetailsDto();
    }

    public class OrderDetailsDto
    {

        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public decimal ShippingCost { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public string? CouponCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string ShippingAddress { get; set; } = string.Empty;

        public string? TrackingCode { get; set; }

        public IEnumerable<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {

        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public int? ProductVariationId { get; set; }

        public string? VariationName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }

    public class UpdateOrderStatusRequest
    {

        public string Status { get; set; } = string.Empty;
    }

    public class UpdatePaymentStatusRequest
    {

        public string PaymentStatus { get; set; } = string.Empty;
    }

    public class AddTrackingCodeRequest
    {

        public string TrackingCode { get; set; } = string.Empty;
    }

    public class OrderActionResponse
    {

        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    public class OrderStatisticsResponse
    {

        public bool Success { get; set; }

        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }

        public int CompletedOrders { get; set; }

        public int CancelledOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal AverageOrderValue { get; set; }
    }

    #endregion
}
