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

        // GET: /Order/Details/5
        [RequireAuth]
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // GET: /Order/Edit/5
        [RequireAuth]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: /Order/Edit/5
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

        // POST: /Order/Process/5?status=Shipped
        [RequireAuth]
        public async Task<IActionResult> Process(int id, string status = "Processing")
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            try
            {
                await _orderService.UpdateOrderStatus(id, status);
                TempData["SuccessMessage"] = $"Pedido atualizado para {status}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pedido {OrderId}", id);
                TempData["ErrorMessage"] = "Erro ao processar pedido";
            }
            return RedirectToAction("Orders", "Admin");
        }

        // GET: /Order
        // Relatórios: mais vendidos
        [RequireAuth]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var topProducts = await _context.OrderItems
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new TopProductReportItem
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(50)
                .ToListAsync();
            return View(topProducts);
        }
    }

    public class TopProductReportItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}


