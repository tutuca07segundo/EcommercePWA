using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;

    public HomeController(ILogger<HomeController> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public IActionResult Index(string searchTerm = "", string category = "", string sortOrder = "name")
    {
        var viewModel = new ProductViewModel
        {
            Categories = _productService.GetAllCategories(),
            SearchTerm = searchTerm,
            SelectedCategory = category,
            SortOrder = sortOrder
        };

        // Filtrar produtos
        var products = _productService.GetAllProducts();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            products = _productService.SearchProducts(searchTerm);
        }
        
        if (!string.IsNullOrWhiteSpace(category))
        {
            products = _productService.GetProductsByCategory(category);
        }

        // Ordenar produtos
        viewModel.Products = _productService.SortProducts(products, sortOrder);

        return View(viewModel);
    }

    public IActionResult Search(string q, string category = "", string sortOrder = "name")
    {
        return RedirectToAction("Index", new { searchTerm = q, category, sortOrder });
    }

    public IActionResult Category(string name, string sortOrder = "name")
    {
        return RedirectToAction("Index", new { category = name, sortOrder });
    }

    public IActionResult Sort(string order)
    {
        var searchTerm = Request.Query["searchTerm"].ToString();
        var category = Request.Query["category"].ToString();
        
        return RedirectToAction("Index", new { searchTerm, category, sortOrder = order });
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
}
