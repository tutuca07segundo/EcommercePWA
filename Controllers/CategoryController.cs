using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Services;
using EcommercePWA.Models;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IProductService productService, ILogger<CategoryController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: /Category/Details/5
        [RequireAuth]
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var categories = await _productService.GetAllCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();
            var products = await _productService.GetProductsByCategoryAsync(category.Name);
            ViewBag.ProductCount = products.Count;
            return View(category);
        }

        // GET: /Category/Create
        [RequireAuth]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            return View(new Category());
        }

        // POST: /Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Create([Bind("Name,Description,IconClass,Color,IsActive,DisplayOrder")] Category category)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            if (!ModelState.IsValid) return View(category);
            try
            {
                await _productService.CreateCategoryAsync(category);
                TempData["SuccessMessage"] = "Categoria criada com sucesso!";
                return RedirectToAction("Categories", "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar categoria");
                ModelState.AddModelError(string.Empty, "Erro ao criar categoria");
                return View(category);
            }
        }

        // GET: /Category/Edit/5
        [RequireAuth]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var categories = await _productService.GetAllCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: /Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IconClass,Color,IsActive,DisplayOrder")] Category category)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            if (id != category.Id) return NotFound();
            if (!ModelState.IsValid) return View(category);
            try
            {
                await _productService.UpdateCategoryAsync(category);
                TempData["SuccessMessage"] = "Categoria atualizada com sucesso!";
                return RedirectToAction("Categories", "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar categoria {CategoryId}", id);
                ModelState.AddModelError(string.Empty, "Erro ao atualizar categoria");
                return View(category);
            }
        }

        // GET: /Category/Delete/5
        [RequireAuth]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var categories = await _productService.GetAllCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: /Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            try
            {
                await _productService.DeleteCategoryAsync(id);
                TempData["SuccessMessage"] = "Categoria excluída com sucesso!";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Categoria possui produtos vinculados {CategoryId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Categories", "Admin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir categoria {CategoryId}", id);
                TempData["ErrorMessage"] = "Erro ao excluir categoria";
                return RedirectToAction("Categories", "Admin");
            }
            return RedirectToAction("Categories", "Admin");
        }
    }
}


