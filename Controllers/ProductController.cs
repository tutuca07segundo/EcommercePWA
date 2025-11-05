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
        private readonly ICategoryPromotionService _categoryPromotionService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, IDiscountService discountService, ICategoryPromotionService categoryPromotionService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _discountService = discountService;
            _categoryPromotionService = categoryPromotionService;
            _logger = logger;
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                await ApplyBestDiscount(product);

                var relatedProducts = await _productService.GetRelatedProductsAsync(product, 4);

                foreach (var relatedProduct in relatedProducts)
                {
                    await ApplyBestDiscount(relatedProduct);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Category,Brand,ImageUrl,StockQuantity,Type,Tags,Weight,Width,Height,Length")] Product product)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Category,Brand,ImageUrl,StockQuantity,Type,Tags,Weight,Width,Height,Length")] Product product)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAuth]
        public async Task<IActionResult> EditSelector([Bind("Id,Name,Description,Price,Category,Brand,ImageUrl,StockQuantity,Type,Tags,Weight,Width,Height,Length")] Product product)
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

        private async Task ApplyBestDiscount(Product product)
        {
            decimal bestDiscountedPrice = product.Price;
            bool hasDiscount = false;

            var productDiscount = await _discountService.GetActiveDiscountForProductAsync(product.Id);
            if (productDiscount != null)
            {
                var productDiscountedPrice = _discountService.CalculateDiscountedPrice(product.Price, productDiscount);
                if (productDiscountedPrice < bestDiscountedPrice)
                {
                    bestDiscountedPrice = productDiscountedPrice;
                    hasDiscount = true;
                }
            }

            var categoryDiscount = await _categoryPromotionService.CalculateCategoryDiscountAsync(product.Category, product.Price);
            if (categoryDiscount > 0)
            {
                var categoryDiscountedPrice = product.Price - categoryDiscount;
                if (categoryDiscountedPrice < bestDiscountedPrice)
                {
                    bestDiscountedPrice = categoryDiscountedPrice;
                    hasDiscount = true;
                }
            }

            if (hasDiscount)
            {
                product.DiscountedPrice = bestDiscountedPrice;
                product.HasDiscount = true;
            }
            else
            {
                product.DiscountedPrice = null;
                product.HasDiscount = false;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBrandsByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return Json(new List<string>());
                }

                var brands = await _productService.GetBrandsByCategoryAsync(category);
                return Json(brands);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar marcas da categoria: {Category}", category);
                return Json(new List<string>());
            }
        }
    }
}
