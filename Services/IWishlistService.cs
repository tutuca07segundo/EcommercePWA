using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface IWishlistService
    {
        Task<List<WishlistItem>> GetUserWishlistAsync(int userId);
        Task<bool> AddToWishlistAsync(int userId, int productId, int? variationId = null);
        Task<bool> RemoveFromWishlistAsync(int userId, int productId, int? variationId = null);
        Task<bool> IsInWishlistAsync(int userId, int productId, int? variationId = null);
        Task<int> GetWishlistCountAsync(int userId);
        Task<bool> ClearWishlistAsync(int userId);
        Task<WishlistItem?> GetWishlistItemAsync(int userId, int productId, int? variationId = null);
    }
}
