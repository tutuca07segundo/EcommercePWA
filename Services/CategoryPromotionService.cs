using EcommercePWA.Data;
using EcommercePWA.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommercePWA.Services
{
    public class CategoryPromotionService : ICategoryPromotionService
    {
        private readonly ApplicationDbContext _context;

        public CategoryPromotionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryPromotion>> GetAllAsync()
        {
            return await _context.CategoryPromotions
                .OrderByDescending(cp => cp.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<CategoryPromotion>> GetActivePromotionsAsync()
        {
            var now = DateTime.Now;
            return await _context.CategoryPromotions
                .Where(cp => cp.IsActive && cp.ValidFrom <= now && cp.ValidUntil >= now)
                .OrderBy(cp => cp.Priority)
                .ToListAsync();
        }

        public async Task<CategoryPromotion?> GetByIdAsync(int id)
        {
            return await _context.CategoryPromotions
                .FirstOrDefaultAsync(cp => cp.Id == id);
        }

        public async Task<List<CategoryPromotion>> GetPromotionsByCategoryAsync(string categoryName)
        {
            var now = DateTime.Now;
            return await _context.CategoryPromotions
                .Where(cp => cp.CategoryName.ToLower() == categoryName.ToLower() 
                          && cp.IsActive 
                          && cp.ValidFrom <= now 
                          && cp.ValidUntil >= now)
                .OrderBy(cp => cp.Priority)
                .ToListAsync();
        }

        public async Task<CategoryPromotion> CreateAsync(CategoryPromotion promotion)
        {
            promotion.CreatedAt = DateTime.Now;
            _context.CategoryPromotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<CategoryPromotion> UpdateAsync(CategoryPromotion promotion)
        {
            _context.CategoryPromotions.Update(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var promotion = await GetByIdAsync(id);
            if (promotion == null)
                return false;

            _context.CategoryPromotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateCategoryDiscountAsync(string categoryName, decimal originalPrice)
        {
            var promotions = await GetPromotionsByCategoryAsync(categoryName);
            
            if (!promotions.Any())
                return 0;

            // Pega a promoção com maior prioridade (menor número = maior prioridade)
            var bestPromotion = promotions.First();
            
            return bestPromotion.CalculateDiscount(originalPrice);
        }

        public async Task<List<string>> GetAvailableCategoriesAsync()
        {
            // Busca todas as categorias únicas dos produtos
            var categories = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return categories;
        }
    }
}
