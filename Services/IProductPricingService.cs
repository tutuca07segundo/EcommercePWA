using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface IProductPricingService
    {
        Task<ProductWithPricing> ApplyDiscountsAsync(Product product);
        Task<List<ProductWithPricing>> ApplyDiscountsToProductsAsync(List<Product> products);
        Task<decimal> CalculateItemPriceAsync(Product product, int? variationId);
    }
}
