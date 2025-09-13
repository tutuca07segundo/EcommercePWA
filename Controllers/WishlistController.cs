using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Services;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    [RequireAuth]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly ICartService _cartService;

        public WishlistController(IWishlistService wishlistService, ICartService cartService)
        {
            _wishlistService = wishlistService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var wishlistItems = await _wishlistService.GetUserWishlistAsync(userId.Value);
            return View(wishlistItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int? variationId = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Usuário não autenticado" });
            }

            var success = await _wishlistService.AddToWishlistAsync(userId.Value, productId, variationId);
            
            if (success)
            {
                var count = await _wishlistService.GetWishlistCountAsync(userId.Value);
                return Json(new { success = true, message = "Produto adicionado aos favoritos!", count = count });
            }
            else
            {
                return Json(new { success = false, message = "Produto já está nos favoritos ou não foi encontrado" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId, int? variationId = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Usuário não autenticado" });
            }

            var success = await _wishlistService.RemoveFromWishlistAsync(userId.Value, productId, variationId);
            
            if (success)
            {
                var count = await _wishlistService.GetWishlistCountAsync(userId.Value);
                return Json(new { success = true, message = "Produto removido dos favoritos!", count = count });
            }
            else
            {
                return Json(new { success = false, message = "Produto não encontrado nos favoritos" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int? variationId = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Usuário não autenticado" });
            }

            try
            {
                // Usar sessionId específico para usuário logado
                var sessionId = $"user_{userId.Value}";
                
                // Adiciona ao carrinho
                await _cartService.AddToCart(sessionId, productId, variationId, 1);
                
                // Remove da wishlist
                await _wishlistService.RemoveFromWishlistAsync(userId.Value, productId, variationId);
                
                var wishlistCount = await _wishlistService.GetWishlistCountAsync(userId.Value);
                return Json(new { success = true, message = "Produto adicionado ao carrinho!", wishlistCount = wishlistCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao adicionar produto ao carrinho: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Usuário não autenticado" });
            }

            await _wishlistService.ClearWishlistAsync(userId.Value);
            return Json(new { success = true, message = "Lista de desejos limpa com sucesso!" });
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { count = 0 });
            }

            var count = await _wishlistService.GetWishlistCountAsync(userId.Value);
            return Json(new { count = count });
        }

        [HttpGet]
        public async Task<IActionResult> CheckStatus(int productId, int? variationId = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { isInWishlist = false });
            }

            var isInWishlist = await _wishlistService.IsInWishlistAsync(userId.Value, productId, variationId);
            return Json(new { isInWishlist = isInWishlist });
        }
    }
}
