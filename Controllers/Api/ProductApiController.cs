using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{

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
