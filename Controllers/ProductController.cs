using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

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

        // GET: /Product/Details/5
        public IActionResult Details(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                Variations = product.Variations.Where(v => v.IsAvailable).OrderBy(v => v.DisplayOrder).ToList(),
                Reviews = product.Reviews.Where(r => r.IsApproved).OrderByDescending(r => r.CreatedAt).Take(10).ToList(),
                RelatedProducts = _productService.GetRelatedProducts(product, 4),
                Quantity = 1
            };

            return View(viewModel);
        }

        // GET: /Product/Category/name
        public IActionResult Category(string name, string sortOrder = "name")
        {
            var products = _productService.GetProductsByCategory(name);
            var sortedProducts = _productService.SortProducts(products, sortOrder);
            var category = _productService.GetAllCategories().FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            var viewModel = new ProductViewModel
            {
                Products = sortedProducts,
                Categories = _productService.GetAllCategories(),
                SelectedCategory = name,
                SortOrder = sortOrder,
                SearchTerm = ""
            };

            ViewData["CategoryName"] = category?.Name ?? name;
            ViewData["CategoryDescription"] = category?.Description ?? "";

            return View("~/Views/Home/Index.cshtml", viewModel);
        }

        // GET: /Product/Search
        public IActionResult Search(string q, string category = "", string sortOrder = "name")
        {
            var products = _productService.SearchProducts(q);
            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var sortedProducts = _productService.SortProducts(products, sortOrder);

            var viewModel = new ProductViewModel
            {
                Products = sortedProducts,
                Categories = _productService.GetAllCategories(),
                SearchTerm = q,
                SelectedCategory = category,
                SortOrder = sortOrder
            };

            return View("~/Views/Home/Index.cshtml", viewModel);
        }
    }
}
