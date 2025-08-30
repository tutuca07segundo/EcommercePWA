using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    public class TrackingController : Controller
    {
        private readonly ITrackingService _trackingService;
        private readonly ILogger<TrackingController> _logger;

        public TrackingController(ITrackingService trackingService, ILogger<TrackingController> logger)
        {
            _trackingService = trackingService;
            _logger = logger;
        }

        // GET: /Tracking
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Tracking/Order/{orderNumber}
        public async Task<IActionResult> Order(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
            {
                TempData["ErrorMessage"] = "Número do pedido é obrigatório";
                return RedirectToAction("Index");
            }

            try
            {
                var tracking = await _trackingService.GetOrderTrackingAsync(orderNumber);
                
                if (tracking == null)
                {
                    TempData["ErrorMessage"] = "Pedido não encontrado";
                    return RedirectToAction("Index");
                }

                // Verificar se o usuário pode visualizar este pedido
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue && tracking.Order.UserId != userId.Value)
                {
                    var userRole = HttpContext.Session.GetString("UserRole");
                    if (userRole != "Admin")
                    {
                        TempData["ErrorMessage"] = "Você não tem permissão para visualizar este pedido";
                        return RedirectToAction("Index");
                    }
                }

                return View("Details", tracking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar rastreamento do pedido: {OrderNumber}", orderNumber);
                TempData["ErrorMessage"] = "Erro ao buscar informações do pedido";
                return RedirectToAction("Index");
            }
        }

        // GET: /Tracking/Details/{orderId}
        [RequireAuth]
        public async Task<IActionResult> Details(int orderId)
        {
            try
            {
                var tracking = await _trackingService.GetOrderTrackingByIdAsync(orderId);
                
                if (tracking == null)
                {
                    TempData["ErrorMessage"] = "Pedido não encontrado";
                    return RedirectToAction("Index");
                }

                // Verificar se o usuário pode visualizar este pedido
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId.HasValue && tracking.Order.UserId != userId.Value)
                {
                    var userRole = HttpContext.Session.GetString("UserRole");
                    if (userRole != "Admin")
                    {
                        TempData["ErrorMessage"] = "Você não tem permissão para visualizar este pedido";
                        return RedirectToAction("Index");
                    }
                }

                return View(tracking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar rastreamento do pedido: {OrderId}", orderId);
                TempData["ErrorMessage"] = "Erro ao buscar informações do pedido";
                return RedirectToAction("Index");
            }
        }

        // POST: /Tracking/Search
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
            {
                TempData["ErrorMessage"] = "Digite o número do pedido";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Order", new { orderNumber = orderNumber.Trim() });
        }

        // GET: /Tracking/MyOrders - Lista pedidos do usuário logado
        [RequireAuth]
        public async Task<IActionResult> MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Aqui você pode implementar um método no service para buscar todos os pedidos do usuário
                // Por enquanto, vamos redirecionar para a página de pedidos do usuário
                return RedirectToAction("Index", "Order");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos do usuário: {UserId}", userId);
                TempData["ErrorMessage"] = "Erro ao carregar seus pedidos";
                return RedirectToAction("Index");
            }
        }
    }
}
