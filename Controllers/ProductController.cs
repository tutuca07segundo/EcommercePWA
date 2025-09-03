using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;
using EcommercePWA.Attributes;

namespace EcommercePWA.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IDiscountService _discountService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, IDiscountService discountService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _discountService = discountService;
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

                // Apply discount to main product
                var discount = await _discountService.GetActiveDiscountForProductAsync(product.Id);
                if (discount != null)
                {
                    product.DiscountedPrice = _discountService.CalculateDiscountedPrice(product.Price, discount);
                    product.HasDiscount = true;
                }

                var relatedProducts = await _productService.GetRelatedProductsAsync(product, 4);
                
                // Apply discounts to related products
                foreach (var relatedProduct in relatedProducts)
                {
                    var relatedDiscount = await _discountService.GetActiveDiscountForProductAsync(relatedProduct.Id);
                    if (relatedDiscount != null)
                    {
                        relatedProduct.DiscountedPrice = _discountService.CalculateDiscountedPrice(relatedProduct.Price, relatedDiscount);
                        relatedProduct.HasDiscount = true;
                    }
                }

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
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
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
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            try
            {
                if (ModelState.IsValid)
                {
                    await _productService.CreateProductAsync(product);
                    TempData["SuccessMessage"] = "Produto criado com sucesso!";
                    return RedirectToAction("Products", "Admin");
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
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
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
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
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
                    return RedirectToAction("Products", "Admin");
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
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
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
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Produto removido com sucesso!";
                    return RedirectToAction("Products", "Admin");
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao remover produto.";
                    return RedirectToAction("Products", "Admin");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover produto: {ProductId}", id);
                TempData["ErrorMessage"] = "Erro ao remover produto.";
                return RedirectToAction("Products", "Admin");
            }
        }

        // GET: /Product/EditSelector
        [RequireAuth]
        public async Task<IActionResult> EditSelector(int? id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            var products = await _productService.GetAllProductsAsync();
            Product? selectedProduct = null;
            if (id.HasValue)
            {
                selectedProduct = await _productService.GetProductByIdAsync(id.Value);
            }
            var categories = await _productService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.Products = products;
            return View("EditSelector", selectedProduct ?? new Product());
        }

        // POST: /Product/EditSelector
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> EditSelector(Product product)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            if (ModelState.IsValid)
            {
                await _productService.UpdateProductAsync(product);
                TempData["SuccessMessage"] = "Produto atualizado com sucesso!";
                return RedirectToAction("EditSelector", new { id = product.Id });
            }
            var products = await _productService.GetAllProductsAsync();
            var categories = await _productService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.Products = products;
            return View("EditSelector", product);
        }

        // POST: /Product/AddVariation
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> AddVariation(int productId, string name, string? colorHex, decimal? priceModifier, bool isAvailable = true)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            try
            {
                var variation = new ProductVariation
                {
                    ProductId = productId,
                    Name = name,
                    Type = "Color",
                    ColorHex = colorHex ?? string.Empty,
                    PriceModifier = priceModifier,
                    IsAvailable = isAvailable
                };
                await _productService.CreateProductVariationAsync(variation);
                TempData["SuccessMessage"] = "Variação adicionada com sucesso!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar variação ao produto {ProductId}", productId);
                TempData["ErrorMessage"] = "Erro ao adicionar variação";
            }
            return RedirectToAction("Edit", new { id = productId });
        }

        // POST: /Product/ToggleVariation
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> ToggleVariation(int productId, int variationId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            try
            {
                var variations = await _productService.GetProductVariationsAsync(productId);
                var variation = variations.FirstOrDefault(v => v.Id == variationId);
                if (variation != null)
                {
                    variation.IsAvailable = !variation.IsAvailable;
                    await _productService.UpdateProductVariationAsync(variation);
                    TempData["SuccessMessage"] = "Disponibilidade da variação atualizada";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alternar variação {VariationId}", variationId);
                TempData["ErrorMessage"] = "Erro ao atualizar variação";
            }
            return RedirectToAction("Edit", new { id = productId });
        }

        // POST: /Product/DeleteVariation
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> DeleteVariation(int productId, int variationId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();
            try
            {
                await _productService.DeleteProductVariationAsync(variationId);
                TempData["SuccessMessage"] = "Variação removida";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover variação {VariationId}", variationId);
                TempData["ErrorMessage"] = "Erro ao remover variação";
            }
            return RedirectToAction("Edit", new { id = productId });
        }
    }
}
