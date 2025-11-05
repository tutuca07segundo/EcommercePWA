using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public class ProductPricingService : IProductPricingService
    {
        private readonly IDiscountService _discountService;
        private readonly ICategoryPromotionService _categoryPromotionService;
        private readonly ILogger<ProductPricingService> _logger;

        public ProductPricingService(
            IDiscountService discountService,
            ICategoryPromotionService categoryPromotionService,
            ILogger<ProductPricingService> logger)
        {
            _discountService = discountService;
            _categoryPromotionService = categoryPromotionService;
            _logger = logger;
        }

        public async Task<ProductWithPricing> ApplyDiscountsAsync(Product product)
        {
            try
            {
                var originalPrice = product.Price;
                var bestDiscountedPrice = originalPrice;
                decimal? discountAmount = null;
                string? discountSource = null;

                var productDiscount = await _discountService.GetActiveDiscountForProductAsync(product.Id);
                if (productDiscount != null)
                {
                    var productDiscountedPrice = _discountService.CalculateDiscountedPrice(originalPrice, productDiscount);
                    if (productDiscountedPrice < bestDiscountedPrice)
                    {
                        bestDiscountedPrice = productDiscountedPrice;
                        discountAmount = originalPrice - productDiscountedPrice;
                        discountSource = "Product";
                    }
                }

                var categoryDiscountAmount = await _categoryPromotionService.CalculateCategoryDiscountAsync(product.Category, originalPrice);
                if (categoryDiscountAmount > 0)
                {
                    var categoryDiscountedPrice = originalPrice - categoryDiscountAmount;
                    if (categoryDiscountedPrice < bestDiscountedPrice)
                    {
                        bestDiscountedPrice = categoryDiscountedPrice;
                        discountAmount = categoryDiscountAmount;
                        discountSource = "Category";
                    }
                }

                product.DiscountedPrice = discountAmount.HasValue ? bestDiscountedPrice : null;
                product.HasDiscount = discountAmount.HasValue;

                return new ProductWithPricing
                {
                    Product = product,
                    OriginalPrice = originalPrice,
                    FinalPrice = bestDiscountedPrice,
                    DiscountAmount = discountAmount,
                    DiscountSource = discountSource,
                    DiscountPercentage = discountAmount.HasValue ? (discountAmount.Value / originalPrice) * 100 : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aplicar descontos ao produto {ProductId}", product.Id);
                return new ProductWithPricing
                {
                    Product = product,
                    OriginalPrice = product.Price,
                    FinalPrice = product.Price
                };
            }
        }

        public async Task<List<ProductWithPricing>> ApplyDiscountsToProductsAsync(List<Product> products)
        {
            var tasks = products.Select(ApplyDiscountsAsync);
            return (await Task.WhenAll(tasks)).ToList();
        }

        public async Task<decimal> CalculateItemPriceAsync(Product product, int? variationId)
        {
            try
            {
                var productWithPricing = await ApplyDiscountsAsync(product);
                var basePrice = productWithPricing.FinalPrice;

                if (variationId.HasValue)
                {
                    var variation = product.Variations.FirstOrDefault(v => v.Id == variationId);
                    if (variation?.PriceModifier.HasValue == true)
                    {
                        return basePrice + variation.PriceModifier.Value;
                    }
                }

                return basePrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular preço do item para produto {ProductId}", product.Id);
                return product.Price;
            }
        }
    }
}
