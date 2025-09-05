using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Services;
using EcommercePWA.Models;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    [RequireAdmin]
    public class CategoryPromotionController : Controller
    {
        private readonly ICategoryPromotionService _categoryPromotionService;

        public CategoryPromotionController(ICategoryPromotionService categoryPromotionService)
        {
            _categoryPromotionService = categoryPromotionService;
        }

        // GET: CategoryPromotion
        public async Task<IActionResult> Index()
        {
            var promotions = await _categoryPromotionService.GetAllAsync();
            return View(promotions);
        }

        // GET: CategoryPromotion/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var promotion = await _categoryPromotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }
            return View(promotion);
        }

        // GET: CategoryPromotion/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
            return View();
        }

        // POST: CategoryPromotion/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryPromotion promotion)
        {
            if (ModelState.IsValid)
            {
                // Validar que pelo menos um tipo de desconto foi definido
                if (!promotion.DiscountAmount.HasValue && !promotion.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Você deve definir um valor fixo ou um percentual de desconto.");
                    ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
                    return View(promotion);
                }

                // Validar que não foram definidos ambos os tipos
                if (promotion.DiscountAmount.HasValue && promotion.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Defina apenas um tipo de desconto: valor fixo OU percentual.");
                    ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
                    return View(promotion);
                }

                // Validar datas
                if (promotion.ValidFrom >= promotion.ValidUntil)
                {
                    ModelState.AddModelError("ValidUntil", "A data de fim deve ser posterior à data de início.");
                    ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
                    return View(promotion);
                }

                await _categoryPromotionService.CreateAsync(promotion);
                TempData["SuccessMessage"] = "Promoção criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
            return View(promotion);
        }

        // GET: CategoryPromotion/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var promotion = await _categoryPromotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
            return View(promotion);
        }

        // POST: CategoryPromotion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryPromotion promotion)
        {
            if (id != promotion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Validar que pelo menos um tipo de desconto foi definido
                if (!promotion.DiscountAmount.HasValue && !promotion.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Você deve definir um valor fixo ou um percentual de desconto.");
                    ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
                    return View(promotion);
                }

                // Validar que não foram definidos ambos os tipos
                if (promotion.DiscountAmount.HasValue && promotion.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Defina apenas um tipo de desconto: valor fixo OU percentual.");
                    ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
                    return View(promotion);
                }

                // Validar datas
                if (promotion.ValidFrom >= promotion.ValidUntil)
                {
                    ModelState.AddModelError("ValidUntil", "A data de fim deve ser posterior à data de início.");
                    ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
                    return View(promotion);
                }

                await _categoryPromotionService.UpdateAsync(promotion);
                TempData["SuccessMessage"] = "Promoção atualizada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
            return View(promotion);
        }

        // GET: CategoryPromotion/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _categoryPromotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }
            return View(promotion);
        }

        // POST: CategoryPromotion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _categoryPromotionService.DeleteAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Promoção excluída com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao excluir a promoção.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: CategoryPromotion/Toggle/5
        public async Task<IActionResult> Toggle(int id)
        {
            var promotion = await _categoryPromotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            promotion.IsActive = !promotion.IsActive;
            await _categoryPromotionService.UpdateAsync(promotion);

            TempData["SuccessMessage"] = $"Promoção {(promotion.IsActive ? "ativada" : "desativada")} com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
