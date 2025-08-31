using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{
    /// <summary>
    /// API para gerenciamento do carrinho de compras
    /// </summary>
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

        /// <summary>
        /// Adiciona um produto ao carrinho
        /// </summary>
        /// <param name="request">Dados do item para adicionar</param>
        /// <returns>Status da operação</returns>
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

        /// <summary>
        /// Obtém os itens do carrinho por sessão
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <returns>Itens do carrinho</returns>
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

        /// <summary>
        /// Remove um item do carrinho
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="itemId">ID do item do carrinho</param>
        /// <returns>Status da operação</returns>
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

    /// <summary>
    /// Modelo para requisição de adicionar ao carrinho
    /// </summary>
    public class AddToCartRequest
    {
        /// <summary>ID da sessão</summary>
        public string SessionId { get; set; } = string.Empty;
        
        /// <summary>ID do produto</summary>
        public int ProductId { get; set; }
        
        /// <summary>Quantidade do produto</summary>
        public int Quantity { get; set; }
        
        /// <summary>ID da variação do produto (opcional)</summary>
        public int? VariationId { get; set; }
    }
}
