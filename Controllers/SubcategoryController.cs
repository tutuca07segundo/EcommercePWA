using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommercePWA.Data;
using EcommercePWA.Models;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    [RequireAuth]
    public class SubcategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubcategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var subcategories = await _context.Subcategories
                .Include(s => s.Category)
                .OrderBy(s => s.Category.Name)
                .ThenBy(s => s.DisplayOrder)
                .ToListAsync();
            return View(subcategories);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subcategory = await _context.Subcategories
                .Include(s => s.Category)
                .Include(s => s.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (subcategory == null)
            {
                return NotFound();
            }

            return View(subcategory);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            Console.WriteLine("Categorias disponíveis:");
            foreach (var cat in categories)
            {
                Console.WriteLine($"ID: {cat.Id}, Nome: {cat.Name}");
            }
            
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subcategory subcategory)
        {

            Console.WriteLine($"CategoryId recebido: {subcategory.CategoryId}");
            Console.WriteLine($"Name recebido: {subcategory.Name}");
            Console.WriteLine($"Description recebido: {subcategory.Description}");
            Console.WriteLine($"IsActive recebido: {subcategory.IsActive}");
            Console.WriteLine($"DisplayOrder recebido: {subcategory.DisplayOrder}");

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        Console.WriteLine($"Campo: {modelState.Key}, Erro: {error.ErrorMessage}");
                    }
                }
            }

            ModelState.Remove("Category");
            
            if (ModelState.IsValid)
            {
                _context.Add(subcategory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Subcategoria criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(subcategory);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subcategory = await _context.Subcategories.FindAsync(id);
            if (subcategory == null)
            {
                return NotFound();
            }
            
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(subcategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subcategory subcategory)
        {
            if (id != subcategory.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Erro de validação Edit: {error.ErrorMessage}");
                }
            }

            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subcategory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Subcategoria atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubcategoryExists(subcategory.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(subcategory);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subcategory = await _context.Subcategories
                .Include(s => s.Category)
                .Include(s => s.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (subcategory == null)
            {
                return NotFound();
            }

            return View(subcategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subcategory = await _context.Subcategories
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (subcategory != null)
            {

                _context.Subcategories.Remove(subcategory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Subcategoria removida com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SubcategoryExists(int id)
        {
            return _context.Subcategories.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var subcategories = await _context.Subcategories
                .Where(s => s.CategoryId == categoryId && s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToListAsync();
            
            return Json(subcategories);
        }
    }
}
