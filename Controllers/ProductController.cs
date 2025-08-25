using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: /Product/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var relatedProducts = await _productService.GetRelatedProductsAsync(product, 4);
                var variations = await _productService.GetProductVariationsAsync(id);

                var viewModel = new ProductDetailViewModel
                {
                    Product = product,
                    RelatedProducts = relatedProducts,
                    Variations = variations
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do produto: {ProductId}", id);
                return NotFound();
            }
        }

        // GET: /Product/Category/name
        public async Task<IActionResult> Category(string name, string sortOrder = "name")
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(name);
                var sortedProducts = _productService.SortProducts(products, sortOrder);
                var categories = await _productService.GetAllCategoriesAsync();
                var category = categories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                var viewModel = new ProductViewModel
                {
                    Products = sortedProducts,
                    Categories = categories,
                    SelectedCategory = name,
                    SortOrder = sortOrder,
                    SearchTerm = ""
                };

                ViewData["CategoryName"] = category?.Name ?? name;
                ViewData["CategoryDescription"] = category?.Description ?? "";

                return View("~/Views/Home/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar categoria: {CategoryName}", name);
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Product/Search
        public async Task<IActionResult> Search(string q, string category = "", string sortOrder = "name")
        {
            try
            {
                var products = await _productService.SearchProductsAsync(q);
                if (!string.IsNullOrWhiteSpace(category))
                {
                    products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                var sortedProducts = _productService.SortProducts(products, sortOrder);
                var categories = await _productService.GetAllCategoriesAsync();

                var viewModel = new ProductViewModel
                {
                    Products = sortedProducts,
                    Categories = categories,
                    SearchTerm = q,
                    SelectedCategory = category,
                    SortOrder = sortOrder
                };

                return View("~/Views/Home/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar busca: {SearchTerm}", q);
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Product/Create
        [RequireAuth]
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(new Product());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar formulário de criação");
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Category,Brand,ImageUrl,StockQuantity,Type,Tags")] Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _productService.CreateProductAsync(product);
                    TempData["SuccessMessage"] = "Produto criado com sucesso!";
                    return RedirectToAction("Index", "Home");
                }

                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar produto");
                ModelState.AddModelError("", "Erro ao criar produto. Tente novamente.");
                
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
        }

        // GET: /Product/Edit/{id}
        [RequireAuth]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar formulário de edição: {ProductId}", id);
                return NotFound();
            }
        }

        // POST: /Product/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Category,Brand,ImageUrl,StockQuantity,Type,Tags")] Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    await _productService.UpdateProductAsync(product);
                    TempData["SuccessMessage"] = "Produto atualizado com sucesso!";
                    return RedirectToAction("Details", new { id = product.Id });
                }

                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar produto: {ProductId}", id);
                ModelState.AddModelError("", "Erro ao atualizar produto. Tente novamente.");
                
                var categories = await _productService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
        }

        // GET: /Product/Delete/{id}
        [RequireAuth]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar formulário de exclusão: {ProductId}", id);
                return NotFound();
            }
        }

        // POST: /Product/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Produto removido com sucesso!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao remover produto.";
                    return RedirectToAction("Details", new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover produto: {ProductId}", id);
                TempData["ErrorMessage"] = "Erro ao remover produto.";
                return RedirectToAction("Details", new { id });
            }
        }
    }
}
