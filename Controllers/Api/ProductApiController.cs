using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{
    /// <summary>
    /// API para gerenciamento de produtos do e-commerce Kaboom
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
        /// Obtém todos os produtos do catálogo
        /// </summary>
        /// <param name="search">Termo de busca opcional</param>
        /// <param name="categoryId">ID da categoria para filtrar</param>
        /// <param name="sortBy">Ordenação: price_asc, price_desc, name, rating, date</param>
        /// <returns>Lista de produtos</returns>
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
        /// Obtém um produto específico por ID
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>Detalhes do produto</returns>
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
        /// Obtém as categorias disponíveis
        /// </summary>
        /// <returns>Lista de categorias</returns>
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
