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

        public async Task<IActionResult> Index()
        {
            var promotions = await _categoryPromotionService.GetAllAsync();
            return View(promotions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var promotion = await _categoryPromotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }
            return View(promotion);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryPromotion promotion)
        {
            if (ModelState.IsValid)
            {

                if (!promotion.DiscountAmount.HasValue && !promotion.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Você deve definir um valor fixo ou um percentual de desconto.");
                    ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
                    return View(promotion);
                }

                if (promotion.DiscountAmount.HasValue && promotion.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Defina apenas um tipo de desconto: valor fixo OU percentual.");
                    ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
                    return View(promotion);
                }

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

                if (!promotion.DiscountAmount.HasValue && !promotion.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Você deve definir um valor fixo ou um percentual de desconto.");
                    ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
                    return View(promotion);
                }

                if (promotion.DiscountAmount.HasValue && promotion.DiscountPercentage.HasValue)
                {
                    ModelState.AddModelError("", "Defina apenas um tipo de desconto: valor fixo OU percentual.");
                    ViewBag.Categories = await _categoryPromotionService.GetAvailableCategoriesAsync();
                    return View(promotion);
                }

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

        public async Task<IActionResult> Delete(int id)
        {
            var promotion = await _categoryPromotionService.GetByIdAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }
            return View(promotion);
        }

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
