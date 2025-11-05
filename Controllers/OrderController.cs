using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Services;
using EcommercePWA.Models;
using EcommercePWA.Attributes;
using EcommercePWA.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ApplicationDbContext context, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _context = context;
            _logger = logger;
        }

        [RequireAuth]
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [RequireAuth]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Edit(int id, string status)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            try
            {
                await _orderService.UpdateOrderStatus(id, status);
                TempData["SuccessMessage"] = "Status do pedido atualizado";
                return RedirectToAction("Orders", "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do pedido {OrderId}", id);
                TempData["ErrorMessage"] = "Erro ao atualizar status";
                return RedirectToAction("Orders", "Admin");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> UpdateOrder(int id, string status, string paymentStatus, 
            string shippingAddress, string shippingCity, string shippingZipCode, string shippingCountry, string? notes)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            
            try
            {
                var order = await _orderService.GetOrderById(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Pedido não encontrado";
                    return RedirectToAction("Orders", "Admin");
                }

                order.Status = status;
                order.PaymentStatus = paymentStatus;
                order.ShippingAddress = shippingAddress;
                order.ShippingCity = shippingCity;
                order.ShippingZipCode = shippingZipCode;
                order.ShippingCountry = shippingCountry;
                order.Notes = notes;

                if (status == "Shipped" && order.ShippedDate == null)
                {
                    order.ShippedDate = DateTime.Now;
                }
                else if (status == "Delivered" && order.DeliveredDate == null)
                {
                    order.DeliveredDate = DateTime.Now;
                }

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                await _orderService.CreateNotification(order.Id, "Pedido Atualizado", 
                    $"Status do pedido alterado para: {status}", "Info");

                TempData["SuccessMessage"] = "Pedido atualizado com sucesso!";
                return RedirectToAction("Details", new { id = order.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pedido {OrderId}", id);
                TempData["ErrorMessage"] = "Erro ao atualizar pedido";
                return RedirectToAction("Edit", new { id });
            }
        }

        [RequireAuth]
        public async Task<IActionResult> Process(int id, string status = "Processing")
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            try
            {
                var order = await _orderService.GetOrderById(id);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Pedido não encontrado";
                    return RedirectToAction("Orders", "Admin");
                }

                await _orderService.UpdateOrderStatus(id, status);

                if (status == "Processing" && order.PaymentStatus == "Pending")
                {
                    order.PaymentStatus = "Paid";
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                }
                
                var statusMessage = status switch
                {
                    "Processing" => "Pagamento confirmado e pedido em processamento",
                    "Shipped" => "Pedido enviado para entrega",
                    "Delivered" => "Pedido entregue com sucesso",
                    "Cancelled" => "Pedido cancelado",
                    _ => $"Status atualizado para {status}"
                };
                
                TempData["SuccessMessage"] = statusMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pedido {OrderId}", id);
                TempData["ErrorMessage"] = "Erro ao processar pedido";
            }
            return RedirectToAction("Orders", "Admin");
        }

        [RequireAuth]
        public async Task<IActionResult> Reports()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var topProducts = await _context.OrderItems
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new 
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(50)
                .ToListAsync();
            
            var reportItems = topProducts.Select(x => new TopProductReportItem
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                TotalQuantity = x.TotalQuantity,
                TotalRevenue = x.TotalRevenue
            }).ToList();
            
            return View("Reports", reportItems);
        }

        [RequireAuth]
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userRole != "Admin")
            {
                _logger.LogWarning("Tentativa de acesso não autorizado à lista de todos os pedidos. UserId: {UserId}, Role: {Role}", userId, userRole);
                TempData["ErrorMessage"] = "❌ Acesso negado! Você não tem permissão para visualizar todos os pedidos.";

                return RedirectToAction("MyOrders", "Order");
            }
            
            var orders = await _orderService.GetAllOrders();
            return View("~/Views/Admin/Orders.cshtml", orders);
        }

        [RequireAuth]
        public async Task<IActionResult> MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                _logger.LogWarning("Tentativa de acesso a MyOrders sem UserId na sessão");
                return RedirectToAction("Login", "Auth");
            }

            try
            {

                var orders = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                    .Where(o => o.UserId == userId.Value) // CRÍTICO: Apenas pedidos do usuário atual
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                _logger.LogInformation("Usuário {UserId} acessou seus pedidos. Total encontrado: {Count}", userId.Value, orders.Count);
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos do usuário {UserId}", userId.Value);
                TempData["ErrorMessage"] = "Erro ao carregar seus pedidos. Tente novamente.";
                return View(new List<Order>());
            }
        }

        [RequireAuth]
        public async Task<IActionResult> MyOrderDetails(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                _logger.LogWarning("Tentativa de acesso a MyOrderDetails sem UserId na sessão");
                return RedirectToAction("Login", "Auth");
            }

            try
            {

                var order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId.Value); // CRÍTICO: Verificar UserId

                if (order == null)
                {
                    _logger.LogWarning("Usuário {UserId} tentou acessar pedido {OrderId} que não lhe pertence ou não existe", userId.Value, id);
                    TempData["ErrorMessage"] = "❌ Pedido não encontrado ou você não tem permissão para visualizá-lo.";
                    return RedirectToAction("MyOrders");
                }

                _logger.LogInformation("Usuário {UserId} acessou detalhes do pedido {OrderId}", userId.Value, id);
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar detalhes do pedido {OrderId} para usuário {UserId}", id, userId.Value);
                TempData["ErrorMessage"] = "Erro ao carregar detalhes do pedido.";
                return RedirectToAction("MyOrders");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> BulkAction(int[] orderIds, string action)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            
            if (orderIds == null || !orderIds.Any())
            {
                TempData["ErrorMessage"] = "Nenhum pedido selecionado";
                return RedirectToAction("Orders", "Admin");
            }

            try
            {
                var processedCount = 0;
                
                if (action == "Delete")
                {

                    foreach (var orderId in orderIds)
                    {
                        try
                        {
                            var order = await _context.Orders
                                .Include(o => o.Items)
                                .FirstOrDefaultAsync(o => o.Id == orderId);
                            
                            if (order != null)
                            {
                                _context.OrderItems.RemoveRange(order.Items);
                                _context.Orders.Remove(order);
                                processedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao excluir pedido {OrderId}", orderId);
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"{processedCount} pedido(s) excluído(s) com sucesso";
                }
                else
                {

                    foreach (var orderId in orderIds)
                    {
                        try
                        {
                            await _orderService.UpdateOrderStatus(orderId, action);
                            processedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao processar pedido {OrderId} na ação em lote", orderId);
                        }
                    }
                    
                    TempData["SuccessMessage"] = $"{processedCount} pedido(s) processado(s) com sucesso";
                }
                
                if (processedCount < orderIds.Length)
                {
                    TempData["WarningMessage"] = $"{orderIds.Length - processedCount} pedido(s) não puderam ser processados";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na ação em lote");
                TempData["ErrorMessage"] = "Erro ao processar ação em lote";
            }

            return RedirectToAction("Orders", "Admin");
        }

        [RequireAuth]
        public async Task<IActionResult> Search(string query, string status, string paymentStatus, DateTime? startDate, DateTime? endDate)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            
            var orders = await _orderService.GetAllOrders();
            
            if (!string.IsNullOrEmpty(query))
            {
                orders = orders.Where(o => 
                    o.OrderNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    o.User.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    o.User.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status).ToList();
            }
            
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                orders = orders.Where(o => o.PaymentStatus == paymentStatus).ToList();
            }
            
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate.Value).ToList();
            }
            
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate.Value.AddDays(1)).ToList();
            }
            
            ViewBag.Query = query;
            ViewBag.Status = status;
            ViewBag.PaymentStatus = paymentStatus;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            
            return View("Index", orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Pedido não encontrado";
                    return RedirectToAction("Orders", "Admin");
                }

                _context.OrderItems.RemoveRange(order.Items);

                _context.Orders.Remove(order);
                
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Pedido #{order.Id} excluído com sucesso";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir pedido {OrderId}", id);
                TempData["ErrorMessage"] = "Erro ao excluir pedido";
            }

            return RedirectToAction("Orders", "Admin");
        }
    }

    public class OrderSearchViewModel
    {
        public string? Query { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}

