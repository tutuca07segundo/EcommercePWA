using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;
    private readonly IDiscountService _discountService;

    public HomeController(ILogger<HomeController> logger, IProductService productService, IDiscountService discountService)
    {
        _logger = logger;
        _productService = productService;
        _discountService = discountService;
    }

    // GET: /Home
    public async Task<IActionResult> Index(string? category, string? searchTerm, string? sortOrder)
    {
        try
        {
            List<Product> products;
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = await _productService.SearchProductsAsync(searchTerm);
            }
            else if (!string.IsNullOrEmpty(category))
            {
                products = await _productService.GetProductsByCategoryAsync(category);
            }
            else
            {
                products = await _productService.GetAllProductsAsync();
            }

            if (!string.IsNullOrEmpty(sortOrder))
            {
                products = _productService.SortProducts(products, sortOrder);
            }

            var categories = await _productService.GetAllCategoriesAsync();

            // Apply discounts to products
            await ApplyDiscountsToProducts(products);

            var viewModel = new ProductViewModel
            {
                Products = products,
                Categories = categories,
                SelectedCategory = category,
                SearchTerm = searchTerm ?? "",
                SortOrder = sortOrder ?? "name"
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar página inicial");
            return View(new ProductViewModel
            {
                Products = new List<Product>(),
                Categories = new List<Category>(),
                SelectedCategory = "",
                SearchTerm = "",
                SortOrder = "name"
            });
        }
    }

    // GET: /Home/Search
    public async Task<IActionResult> Search(string q)
    {
        try
        {
            var products = await _productService.SearchProductsAsync(q);
            var categories = await _productService.GetAllCategoriesAsync();

            // Apply discounts to products
            await ApplyDiscountsToProducts(products);

            var viewModel = new ProductViewModel
            {
                Products = products,
                Categories = categories,
                SearchTerm = q ?? "",
                SortOrder = "name"
            };

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar busca");
            return RedirectToAction("Index");
        }
    }

    // GET: /Home/Category/{name}
    public async Task<IActionResult> Category(string name)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(name);
            var categories = await _productService.GetAllCategoriesAsync();

            // Apply discounts to products
            await ApplyDiscountsToProducts(products);

            var viewModel = new ProductViewModel
            {
                Products = products,
                Categories = categories,
                SelectedCategory = name,
                SortOrder = "name"
            };

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar categoria: {CategoryName}", name);
            return RedirectToAction("Index");
        }
    }

    // GET: /Home/Sort
    public async Task<IActionResult> Sort(string order)
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            var sortedProducts = _productService.SortProducts(products, order);
            var categories = await _productService.GetAllCategoriesAsync();

            // Apply discounts to products
            await ApplyDiscountsToProducts(sortedProducts);

            var viewModel = new ProductViewModel
            {
                Products = sortedProducts,
                Categories = categories,
                SortOrder = order
            };

            return View("Index", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ordenar produtos");
            return RedirectToAction("Index");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task ApplyDiscountsToProducts(List<Product> products)
    {
        foreach (var product in products)
        {
            var discount = await _discountService.GetActiveDiscountForProductAsync(product.Id);
            if (discount != null)
            {
                product.DiscountedPrice = _discountService.CalculateDiscountedPrice(product.Price, discount);
                product.HasDiscount = true;
            }
            else
            {
                product.DiscountedPrice = null;
                product.HasDiscount = false;
            }
        }
    }
}
