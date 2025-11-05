using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    [RequireAdmin]
    public class DiscountController : Controller
    {
        private readonly IDiscountService _discountService;
        private readonly IProductService _productService;
        private readonly ILogger<DiscountController> _logger;

        public DiscountController(IDiscountService discountService, IProductService productService, ILogger<DiscountController> logger)
        {
            _discountService = discountService;
            _productService = productService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var discounts = await _discountService.GetAllDiscountsAsync();
            return View(discounts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var discount = await _discountService.GetDiscountByIdAsync(id);
            if (discount == null)
            {
                return NotFound();
            }
            return View(discount);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Discount discount)
        {
            if (ModelState.IsValid)
            {

                if (!discount.DiscountAmount.HasValue && !discount.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Especifique um valor fixo ou percentual de desconto");
                    return View(discount);
                }

                if (discount.DiscountAmount.HasValue && discount.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Especifique apenas um tipo de desconto (valor fixo OU percentual)");
                    return View(discount);
                }

                discount.CreatedByUserId = HttpContext.Session.GetInt32("UserId");
                await _discountService.CreateDiscountAsync(discount);
                TempData["SuccessMessage"] = "Desconto criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(discount);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var discount = await _discountService.GetDiscountByIdAsync(id);
            if (discount == null)
            {
                return NotFound();
            }
            return View(discount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Discount discount)
        {
            if (id != discount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                if (!discount.DiscountAmount.HasValue && !discount.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Especifique um valor fixo ou percentual de desconto");
                    return View(discount);
                }

                if (discount.DiscountAmount.HasValue && discount.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Especifique apenas um tipo de desconto (valor fixo OU percentual)");
                    return View(discount);
                }

                try
                {
                    await _discountService.UpdateDiscountAsync(discount);
                    TempData["SuccessMessage"] = "Desconto atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar desconto");
                    ModelState.AddModelError("", "Erro ao atualizar desconto");
                }
            }
            return View(discount);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var discount = await _discountService.GetDiscountByIdAsync(id);
            if (discount == null)
            {
                return NotFound();
            }
            return View(discount);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _discountService.DeleteDiscountAsync(id);
                TempData["SuccessMessage"] = "Desconto excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir desconto");
                TempData["ErrorMessage"] = "Erro ao excluir desconto";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ManageProducts(int id)
        {
            var discount = await _discountService.GetDiscountByIdAsync(id);
            if (discount == null)
            {
                return NotFound();
            }

            ViewBag.DiscountId = id;
            ViewBag.DiscountName = discount.Name;
            
            var products = await _discountService.GetProductsForDiscountAsync(id);
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(int discountId, int productId)
        {
            try
            {
                var success = await _discountService.AddProductToDiscountAsync(discountId, productId);
                if (success)
                {
                    return Json(new { success = true, message = "Produto adicionado ao desconto com sucesso!" });
                }
                return Json(new { success = false, message = "Produto já está neste desconto" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar produto ao desconto");
                return Json(new { success = false, message = "Erro ao adicionar produto" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveProduct(int discountId, int productId)
        {
            try
            {
                var success = await _discountService.RemoveProductFromDiscountAsync(discountId, productId);
                if (success)
                {
                    return Json(new { success = true, message = "Produto removido do desconto com sucesso!" });
                }
                return Json(new { success = false, message = "Produto não encontrado neste desconto" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover produto do desconto");
                return Json(new { success = false, message = "Erro ao remover produto" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableProducts(int discountId)
        {
            try
            {
                var products = await _discountService.GetAvailableProductsForDiscountAsync(discountId);
                return Json(products.Select(p => new { id = p.Id, name = p.Name, price = p.Price.ToString("N2") }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter produtos disponíveis");
                return Json(new { success = false, message = "Erro ao carregar produtos" });
            }
        }
    }
}
