using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{
    /// <summary>
    /// API para funcionalidades da página inicial e busca avançada
    /// </summary>
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

        /// <summary>
        /// Filtra produtos com critérios avançados
        /// </summary>
        /// <param name="filter">Critérios de filtro</param>
        /// <returns>Produtos filtrados com paginação</returns>
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
                        Rating = p.Rating,
                        ReviewCount = p.ReviewCount,
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

        /// <summary>
        /// Obtém opções disponíveis para filtros
        /// </summary>
        /// <returns>Categorias, marcas, faixas de preço e avaliações disponíveis</returns>
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
                    Ratings = options.AvailableRatings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter opções de filtro");
                return BadRequest(new { success = false, message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém sugestões de busca em tempo real
        /// </summary>
        /// <param name="q">Termo de busca</param>
        /// <param name="maxResults">Número máximo de resultados (padrão: 5)</param>
        /// <returns>Lista de sugestões de produtos</returns>
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

    /// <summary>
    /// Resposta para produtos filtrados
    /// </summary>
    public class FilteredProductsResponse
    {
        /// <summary>Status da operação</summary>
        public bool Success { get; set; }
        
        /// <summary>Lista de produtos</summary>
        public IEnumerable<ProductSummary> Products { get; set; } = new List<ProductSummary>();
        
        /// <summary>Total de produtos encontrados</summary>
        public int TotalCount { get; set; }
        
        /// <summary>Página atual</summary>
        public int CurrentPage { get; set; }
        
        /// <summary>Total de páginas</summary>
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Resumo do produto para listagens
    /// </summary>
    public class ProductSummary
    {
        /// <summary>ID do produto</summary>
        public int Id { get; set; }
        
        /// <summary>Nome do produto</summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>Preço original</summary>
        public decimal Price { get; set; }
        
        /// <summary>Preço com desconto (se houver)</summary>
        public decimal? DiscountedPrice { get; set; }
        
        /// <summary>Indica se tem desconto</summary>
        public bool HasDiscount { get; set; }
        
        /// <summary>URL da imagem</summary>
        public string ImageUrl { get; set; } = string.Empty;
        
        /// <summary>Categoria</summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>Marca</summary>
        public string Brand { get; set; } = string.Empty;
        
        /// <summary>Avaliação média</summary>
        public decimal Rating { get; set; }
        
        /// <summary>Número de avaliações</summary>
        public int ReviewCount { get; set; }
        
        /// <summary>Tipo do produto</summary>
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta para opções de filtro
    /// </summary>
    public class FilterOptionsResponse
    {
        /// <summary>Status da operação</summary>
        public bool Success { get; set; }
        
        /// <summary>Categorias disponíveis</summary>
        public IEnumerable<string> Categories { get; set; } = new List<string>();
        
        /// <summary>Marcas disponíveis</summary>
        public IEnumerable<string> Brands { get; set; } = new List<string>();
        
        /// <summary>Preço mínimo</summary>
        public decimal MinPrice { get; set; }
        
        /// <summary>Preço máximo</summary>
        public decimal MaxPrice { get; set; }
        
        /// <summary>Avaliações disponíveis</summary>
        public IEnumerable<decimal> Ratings { get; set; } = new List<decimal>();
    }

    /// <summary>
    /// Resposta para sugestões de busca
    /// </summary>
    public class SearchSuggestionsResponse
    {
        /// <summary>Status da operação</summary>
        public bool Success { get; set; }
        
        /// <summary>Lista de sugestões</summary>
        public IEnumerable<ProductSuggestion> Suggestions { get; set; } = new List<ProductSuggestion>();
    }

    /// <summary>
    /// Sugestão de produto para busca
    /// </summary>
    public class ProductSuggestion
    {
        /// <summary>ID do produto</summary>
        public int Id { get; set; }
        
        /// <summary>Nome do produto</summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>Categoria</summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>Marca</summary>
        public string Brand { get; set; } = string.Empty;
        
        /// <summary>Preço</summary>
        public decimal Price { get; set; }
        
        /// <summary>URL da imagem</summary>
        public string ImageUrl { get; set; } = string.Empty;
        
        /// <summary>Tipo do produto</summary>
        public string Type { get; set; } = string.Empty;
    }

    #endregion
}
