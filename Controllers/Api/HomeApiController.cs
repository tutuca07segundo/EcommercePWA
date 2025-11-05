using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HomeApiController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<HomeApiController> _logger;

        public HomeApiController(IProductService productService, ILogger<HomeApiController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpPost("filter-products")]
        [ProducesResponseType(typeof(FilteredProductsResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> FilterProducts([FromBody] ProductFilterModel filter)
        {
            try
            {
                var result = await _productService.FilterProductsAsync(filter);
                
                return Ok(new FilteredProductsResponse
                {
                    Success = true,
                    Products = result.Products.Select(p => new ProductSummary
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        DiscountedPrice = p.DiscountedPrice,
                        HasDiscount = p.HasDiscount,
                        ImageUrl = p.ImageUrl ?? "/images/placeholder.jpg",
                        Category = p.Category,
                        Brand = p.Brand,
                        Type = p.Type
                    }),
                    TotalCount = result.TotalCount,
                    CurrentPage = result.CurrentPage,
                    TotalPages = result.TotalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao filtrar produtos");
                return BadRequest(new { success = false, message = "Erro interno do servidor" });
            }
        }

        [HttpGet("filter-options")]
        [ProducesResponseType(typeof(FilterOptionsResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetFilterOptions()
        {
            try
            {
                var options = await _productService.GetFilterOptionsAsync();
                return Ok(new FilterOptionsResponse
                {
                    Success = true,
                    Categories = options.AvailableCategories,
                    Brands = options.AvailableBrands,
                    MinPrice = options.MinPrice,
                    MaxPrice = options.MaxPrice,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter opções de filtro");
                return BadRequest(new { success = false, message = "Erro interno do servidor" });
            }
        }

        [HttpGet("search-suggestions")]
        [ProducesResponseType(typeof(SearchSuggestionsResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] string q, [FromQuery] int maxResults = 5)
        {
            try
            {
                var suggestions = await _productService.GetSearchSuggestionsAsync(q, maxResults);
                return Ok(new SearchSuggestionsResponse
                {
                    Success = true,
                    Suggestions = suggestions.Select(s => new ProductSuggestion
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Category = s.Category,
                        Brand = s.Brand,
                        Price = s.Price,
                        ImageUrl = s.ImageUrl ?? "/images/placeholder.jpg",
                        Type = s.Type
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter sugestões de busca");
                return BadRequest(new { success = false, suggestions = new List<object>() });
            }
        }
    }

    #region Response Models

    public class FilteredProductsResponse
    {

        public bool Success { get; set; }

        public IEnumerable<ProductSummary> Products { get; set; } = new List<ProductSummary>();

        public int TotalCount { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }
    }

    public class ProductSummary
    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? DiscountedPrice { get; set; }

        public bool HasDiscount { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public decimal Rating { get; set; }

        public int ReviewCount { get; set; }

        public string Type { get; set; } = string.Empty;
    }

    public class FilterOptionsResponse
    {

        public bool Success { get; set; }

        public IEnumerable<string> Categories { get; set; } = new List<string>();

        public IEnumerable<string> Brands { get; set; } = new List<string>();

        public decimal MinPrice { get; set; }

        public decimal MaxPrice { get; set; }

        public IEnumerable<decimal> Ratings { get; set; } = new List<decimal>();
    }

    public class SearchSuggestionsResponse
    {

        public bool Success { get; set; }

        public IEnumerable<ProductSuggestion> Suggestions { get; set; } = new List<ProductSuggestion>();
    }

    public class ProductSuggestion
    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;
    }

    #endregion
}
