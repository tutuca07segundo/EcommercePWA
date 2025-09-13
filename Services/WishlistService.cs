using EcommercePWA.Data;
using EcommercePWA.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly ApplicationDbContext _context;

        public WishlistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WishlistItem>> GetUserWishlistAsync(int userId)
        {
            return await _context.WishlistItems
                .Include(w => w.Product)
                    .ThenInclude(p => p.Subcategory)
                        .ThenInclude(s => s.Category)
                .Include(w => w.ProductVariation)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();
        }

        public async Task<bool> AddToWishlistAsync(int userId, int productId, int? variationId = null)
        {
            // Verifica se já existe na wishlist
            var existingItem = await GetWishlistItemAsync(userId, productId, variationId);
            if (existingItem != null)
            {
                return false; // Já existe na wishlist
            }

            // Verifica se o produto existe
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            // Se tem variationId, verifica se a variação existe
            if (variationId.HasValue)
            {
                var variation = await _context.ProductVariations
                    .FirstOrDefaultAsync(v => v.Id == variationId && v.ProductId == productId);
                if (variation == null)
                {
                    return false;
                }
            }

            var wishlistItem = new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                ProductVariationId = variationId,
                AddedAt = DateTime.Now
            };

            _context.WishlistItems.Add(wishlistItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, int productId, int? variationId = null)
        {
            var item = await GetWishlistItemAsync(userId, productId, variationId);
            if (item == null)
            {
                return false;
            }

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsInWishlistAsync(int userId, int productId, int? variationId = null)
        {
            var item = await GetWishlistItemAsync(userId, productId, variationId);
            return item != null;
        }

        public async Task<int> GetWishlistCountAsync(int userId)
        {
            return await _context.WishlistItems
                .CountAsync(w => w.UserId == userId);
        }

        public async Task<bool> ClearWishlistAsync(int userId)
        {
            var items = await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (items.Any())
            {
                _context.WishlistItems.RemoveRange(items);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<WishlistItem?> GetWishlistItemAsync(int userId, int productId, int? variationId = null)
        {
            return await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && 
                                         w.ProductId == productId && 
                                         w.ProductVariationId == variationId);
        }
    }
}
