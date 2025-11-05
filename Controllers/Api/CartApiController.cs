using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CartApiController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartApiController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("add")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                await _cartService.AddToCart(request.SessionId, request.ProductId, request.VariationId, request.Quantity);
                return Ok(new { success = true, message = "Produto adicionado ao carrinho" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{sessionId}")]
        [ProducesResponseType(typeof(Cart), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetCart(string sessionId)
        {
            try
            {
                var cart = _cartService.GetCart(sessionId);
                if (cart == null)
                    return NotFound(new { error = "Carrinho não encontrado" });

                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{sessionId}/items/{itemId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult RemoveFromCart(string sessionId, int itemId)
        {
            try
            {
                _cartService.RemoveItem(sessionId, itemId);
                return Ok(new { success = true, message = "Item removido do carrinho" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class AddToCartRequest
    {

        public string SessionId { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public int? VariationId { get; set; }
    }
}
