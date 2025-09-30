using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{
    /// <summary>
    /// API para gerenciamento da lista de desejos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WishlistApiController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly ILogger<WishlistApiController> _logger;

        public WishlistApiController(IWishlistService wishlistService, ILogger<WishlistApiController> logger)
        {
            _wishlistService = wishlistService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os itens da lista de desejos do usuário
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Lista de itens da wishlist</returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(WishlistResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetWishlist(int userId)
        {
            try
            {
                var items = await _wishlistService.GetUserWishlistAsync(userId);
                return Ok(new WishlistResponse
                {
                    Success = true,
                    Items = items.Select(item => new WishlistItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product?.Name ?? "",
                        ProductPrice = item.Product?.Price ?? 0,
                        ProductImageUrl = item.Product?.ImageUrl ?? "/images/placeholder.jpg",
                        ProductVariationId = item.ProductVariationId,
                        VariationName = item.ProductVariation?.Name ?? "",
                        AddedAt = item.AddedAt
                    }),
                    Count = items.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter wishlist do usuário {UserId}", userId);
                return BadRequest(new { success = false, message = "Erro ao obter lista de desejos" });
            }
        }

        /// <summary>
        /// Adiciona um produto à lista de desejos
        /// </summary>
        /// <param name="request">Dados do item para adicionar</param>
        /// <returns>Status da operação</returns>
        [HttpPost("add")]
        [ProducesResponseType(typeof(WishlistActionResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistRequest request)
        {
            try
            {
                await _wishlistService.AddToWishlistAsync(request.UserId, request.ProductId, request.VariationId);
                return Ok(new WishlistActionResponse
                {
                    Success = true,
                    Message = "Produto adicionado à lista de desejos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar produto {ProductId} à wishlist do usuário {UserId}", 
                    request.ProductId, request.UserId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Remove um produto da lista de desejos
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="productId">ID do produto</param>
        /// <param name="variationId">ID da variação (opcional)</param>
        /// <returns>Status da operação</returns>
        [HttpDelete("remove")]
        [ProducesResponseType(typeof(WishlistActionResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RemoveFromWishlist([FromQuery] int userId, [FromQuery] int productId, [FromQuery] int? variationId = null)
        {
            try
            {
                await _wishlistService.RemoveFromWishlistAsync(userId, productId, variationId);
                return Ok(new WishlistActionResponse
                {
                    Success = true,
                    Message = "Produto removido da lista de desejos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover produto {ProductId} da wishlist do usuário {UserId}", 
                    productId, userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Verifica se um produto está na lista de desejos
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="productId">ID do produto</param>
        /// <param name="variationId">ID da variação (opcional)</param>
        /// <returns>Status se está na wishlist</returns>
        [HttpGet("check")]
        [ProducesResponseType(typeof(WishlistCheckResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> IsInWishlist([FromQuery] int userId, [FromQuery] int productId, [FromQuery] int? variationId = null)
        {
            try
            {
                var isInWishlist = await _wishlistService.IsInWishlistAsync(userId, productId, variationId);
                return Ok(new WishlistCheckResponse
                {
                    Success = true,
                    IsInWishlist = isInWishlist
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar wishlist para produto {ProductId} do usuário {UserId}", 
                    productId, userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Limpa toda a lista de desejos do usuário
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Status da operação</returns>
        [HttpDelete("clear/{userId}")]
        [ProducesResponseType(typeof(WishlistActionResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ClearWishlist(int userId)
        {
            try
            {
                await _wishlistService.ClearWishlistAsync(userId);
                return Ok(new WishlistActionResponse
                {
                    Success = true,
                    Message = "Lista de desejos limpa com sucesso"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar wishlist do usuário {UserId}", userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Obtém contagem de itens na wishlist
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Número de itens na wishlist</returns>
        [HttpGet("count/{userId}")]
        [ProducesResponseType(typeof(WishlistCountResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetWishlistCount(int userId)
        {
            try
            {
                var count = await _wishlistService.GetWishlistCountAsync(userId);
                return Ok(new WishlistCountResponse
                {
                    Success = true,
                    Count = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter contagem da wishlist do usuário {UserId}", userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    #region Request/Response Models

    /// <summary>
    /// Requisição para adicionar à wishlist
    /// </summary>
    public class AddToWishlistRequest
    {
        /// <summary>ID do usuário</summary>
        public int UserId { get; set; }
        
        /// <summary>ID do produto</summary>
        public int ProductId { get; set; }
        
        /// <summary>ID da variação (opcional)</summary>
        public int? VariationId { get; set; }
    }

    /// <summary>
    /// Resposta da wishlist
    /// </summary>
    public class WishlistResponse
    {
        /// <summary>Status da operação</summary>
        public bool Success { get; set; }
        
        /// <summary>Lista de itens</summary>
        public IEnumerable<WishlistItemDto> Items { get; set; } = new List<WishlistItemDto>();
        
        /// <summary>Contagem de itens</summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// DTO do item da wishlist
    /// </summary>
    public class WishlistItemDto
    {
        /// <summary>ID do item da wishlist</summary>
        public int Id { get; set; }
        
        /// <summary>ID do produto</summary>
        public int ProductId { get; set; }
        
        /// <summary>Nome do produto</summary>
        public string ProductName { get; set; } = string.Empty;
        
        /// <summary>Preço do produto</summary>
        public decimal ProductPrice { get; set; }
        
        /// <summary>URL da imagem do produto</summary>
        public string ProductImageUrl { get; set; } = string.Empty;
        
        /// <summary>ID da variação (se houver)</summary>
        public int? ProductVariationId { get; set; }
        
        /// <summary>Nome da variação</summary>
        public string VariationName { get; set; } = string.Empty;
        
        /// <summary>Data de adição</summary>
        public DateTime AddedAt { get; set; }
    }

    /// <summary>
    /// Resposta de ação na wishlist
    /// </summary>
    public class WishlistActionResponse
    {
        /// <summary>Status da operação</summary>
        public bool Success { get; set; }
        
        /// <summary>Mensagem de retorno</summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta de verificação da wishlist
    /// </summary>
    public class WishlistCheckResponse
    {
        /// <summary>Status da operação</summary>
        public bool Success { get; set; }
        
        /// <summary>Se está na wishlist</summary>
        public bool IsInWishlist { get; set; }
    }

    /// <summary>
    /// Resposta de contagem da wishlist
    /// </summary>
    public class WishlistCountResponse
    {
        /// <summary>Status da operação</summary>
        public bool Success { get; set; }
        
        /// <summary>Contagem de itens</summary>
        public int Count { get; set; }
    }

    #endregion
}
