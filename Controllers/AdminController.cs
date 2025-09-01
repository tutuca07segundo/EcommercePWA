using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;
using EcommercePWA.Attributes;

//https://32ded4e54472.ngrok-free.app --> link do ngrok
//Cartão aprovado: 4509 9535 6623 3704
//CVV: 123
//Validade: 11/25
//Nome: APRO

//Dashboard: /Admin
//Produtos: /Admin/Products
//Cupons: /Admin/Coupons
//Pedidos: /Admin/Orders
//Usuários: /Admin/Users
//Categorias: /Admin/Categories
//taskkill /F /IM dotnet.exe ---> finaliza processo que ja esta rodando

// ✅ Cartão negado 4000 0000 0000 0002  (Visa - sempre negado)
// CVV: 123
// Validade: 12/26
// Nome: Teste Negado

// ✅ Cartão aprovado 4509 9535 6623 3704  (Visa - Mercado Pago)
// CVV: 123
// Validade: 11/25
// Nome: APRO

namespace EcommercePWA.Controllers
{
    [RequireAdmin]
    public class AdminController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICouponService _couponService;
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IProductService productService,
            ICouponService couponService,
            IOrderService orderService,
            IAuthService authService,
            ILogger<AdminController> logger)
        {
            _productService = productService;
            _couponService = couponService;
            _orderService = orderService;
            _authService = authService;
            _logger = logger;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                var orders = await _orderService.GetAllOrders();
                
                var users = await _authService.GetAllUsersAsync();
                var dashboard = new AdminDashboardViewModel
                {
                    TotalProducts = products.Count,
                    TotalCoupons = _couponService.GetAllCoupons().Count,
                    TotalOrders = orders.Count,
                    TotalUsers = users.Count,
                    RecentOrders = orders.Take(5).ToList(),
                    RecentProducts = products.Take(5).ToList()
                };

                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dashboard administrativo");
                return View("Error");
            }
        }

        // GET: /Admin/Products
        public async Task<IActionResult> Products()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        // GET: /Admin/Coupons
        public IActionResult Coupons()
        {
            var coupons = _couponService.GetAllCoupons();
            return View(coupons);
        }

        // GET: /Admin/Orders
        public async Task<IActionResult> Orders()
        {
            var orders = await _orderService.GetAllOrders();
            return View(orders);
        }

        // GET: /Admin/Users
        public IActionResult Users()
        {
            var users = _authService.GetAllUsersAsync().Result;
            return View(users);
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _productService.GetAllCategoriesAsync();
            return View(categories);
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCoupons { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Product> RecentProducts { get; set; } = new List<Product>();
    }
}
