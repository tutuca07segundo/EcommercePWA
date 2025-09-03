using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface IDiscountService
    {
        Task<List<Discount>> GetAllDiscountsAsync();
        Task<Discount?> GetDiscountByIdAsync(int id);
        Task<Discount> CreateDiscountAsync(Discount discount);
        Task<Discount> UpdateDiscountAsync(Discount discount);
        Task<bool> DeleteDiscountAsync(int id);
        Task<bool> AddProductToDiscountAsync(int discountId, int productId);
        Task<bool> RemoveProductFromDiscountAsync(int discountId, int productId);
        Task<List<Product>> GetProductsForDiscountAsync(int discountId);
        Task<List<Product>> GetAvailableProductsForDiscountAsync(int discountId);
        Task<Discount?> GetActiveDiscountForProductAsync(int productId);
        decimal CalculateDiscountedPrice(decimal originalPrice, Discount discount);
    }
}
