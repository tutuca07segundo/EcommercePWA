using Microsoft.AspNetCore.Mvc;
using EcommercePWA.Models;
using EcommercePWA.Services;

namespace EcommercePWA.Controllers.Api
{

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

    public class AddToWishlistRequest
    {

        public int UserId { get; set; }

        public int ProductId { get; set; }

        public int? VariationId { get; set; }
    }

    public class WishlistResponse
    {

        public bool Success { get; set; }

        public IEnumerable<WishlistItemDto> Items { get; set; } = new List<WishlistItemDto>();

        public int Count { get; set; }
    }

    public class WishlistItemDto
    {

        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal ProductPrice { get; set; }

        public string ProductImageUrl { get; set; } = string.Empty;

        public int? ProductVariationId { get; set; }

        public string VariationName { get; set; } = string.Empty;

        public DateTime AddedAt { get; set; }
    }

    public class WishlistActionResponse
    {

        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }

    public class WishlistCheckResponse
    {

        public bool Success { get; set; }

        public bool IsInWishlist { get; set; }
    }

    public class WishlistCountResponse
    {

        public bool Success { get; set; }

        public int Count { get; set; }
    }

    #endregion
}
