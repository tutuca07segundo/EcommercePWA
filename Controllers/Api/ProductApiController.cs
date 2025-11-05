using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{
    /// <summary>
    /// API de Produtos - Catálogo e Categorias
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductApiController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Lista produtos com filtros opcionais
        /// </summary>
        /// <param name="search">Termo de busca (nome, descrição, marca)</param>
        /// <param name="categoryId">ID da categoria para filtrar</param>
        /// <param name="sortBy">Ordenação: name, price, rating</param>
        /// <returns>Lista de produtos</returns>
        /// <response code="200">Lista de produtos retornada com sucesso</response>
        /// <response code="400">Erro ao buscar produtos</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), 200)]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] string sortBy = "name")
        {
            try
            {
                var products = search != null 
                    ? await _productService.SearchProductsAsync(search)
                    : await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém detalhes de um produto específico
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>Dados completos do produto</returns>
        /// <response code="200">Produto encontrado</response>
        /// <response code="404">Produto não encontrado</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Product), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(new { error = "Produto não encontrado" });

                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lista todas as categorias de produtos
        /// </summary>
        /// <returns>Lista de categorias</returns>
        /// <response code="200">Lista de categorias retornada com sucesso</response>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(IEnumerable<Category>), 200)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _productService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
