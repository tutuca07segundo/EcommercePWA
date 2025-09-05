using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface ICategoryPromotionService
    {
        Task<List<CategoryPromotion>> GetAllAsync();
        Task<List<CategoryPromotion>> GetActivePromotionsAsync();
        Task<CategoryPromotion?> GetByIdAsync(int id);
        Task<List<CategoryPromotion>> GetPromotionsByCategoryAsync(string categoryName);
        Task<CategoryPromotion> CreateAsync(CategoryPromotion promotion);
        Task<CategoryPromotion> UpdateAsync(CategoryPromotion promotion);
        Task<bool> DeleteAsync(int id);
        Task<decimal> CalculateCategoryDiscountAsync(string categoryName, decimal originalPrice);
        Task<List<string>> GetAvailableCategoriesAsync();
    }
}
