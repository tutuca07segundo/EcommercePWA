using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{

    /// <summary>
    /// API de Carrinho de Compras
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
        /// <param name="request">Dados do produto (ID, quantidade, variação)</param>
        /// <returns>Confirmação de adição</returns>
        /// <response code="200">Produto adicionado com sucesso</response>
        /// <response code="400">Erro ao adicionar produto</response>
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
        /// Obtém o carrinho de compras
        /// </summary>
        /// <param name="sessionId">ID da sessão do usuário</param>
        /// <returns>Dados completos do carrinho</returns>
        /// <response code="200">Carrinho encontrado</response>
        /// <response code="404">Carrinho não encontrado</response>
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
        /// <param name="itemId">ID do item a remover</param>
        /// <returns>Confirmação de remoção</returns>
        /// <response code="200">Item removido com sucesso</response>
        /// <response code="404">Item não encontrado</response>
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
    /// Modelo de requisição para adicionar ao carrinho
    /// </summary>
    public class AddToCartRequest
    {
        /// <summary>
        /// ID da sessão do usuário
        /// </summary>
        /// <example>abc123xyz</example>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// ID do produto
        /// </summary>
        /// <example>1</example>
        public int ProductId { get; set; }

        /// <summary>
        /// Quantidade do produto
        /// </summary>
        /// <example>2</example>
        public int Quantity { get; set; }

        /// <summary>
        /// ID da variação (opcional - cor, tamanho, etc.)
        /// </summary>
        /// <example>5</example>
        public int? VariationId { get; set; }
    }
}
