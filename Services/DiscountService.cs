using Microsoft.EntityFrameworkCore;
using EcommercePWA.Data;
using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiscountService> _logger;

        public DiscountService(ApplicationDbContext context, ILogger<DiscountService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Discount>> GetAllDiscountsAsync()
        {
            return await _context.Discounts
                .Include(d => d.ProductDiscounts)
                .ThenInclude(pd => pd.Product)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<Discount?> GetDiscountByIdAsync(int id)
        {
            return await _context.Discounts
                .Include(d => d.ProductDiscounts)
                .ThenInclude(pd => pd.Product)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Discount> CreateDiscountAsync(Discount discount)
        {
            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();
            return discount;
        }

        public async Task<Discount> UpdateDiscountAsync(Discount discount)
        {
            _context.Discounts.Update(discount);
            await _context.SaveChangesAsync();
            return discount;
        }

        public async Task<bool> DeleteDiscountAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return false;

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Product>> GetProductsForDiscountAsync(int discountId)
        {
            return await _context.ProductDiscounts
                .Where(pd => pd.DiscountId == discountId)
                .Include(pd => pd.Product)
                .Select(pd => pd.Product)
                .ToListAsync();
        }

        public async Task<List<Product>> GetAvailableProductsAsync(int discountId)
        {
            var productsInDiscount = await _context.ProductDiscounts
                .Where(pd => pd.DiscountId == discountId)
                .Select(pd => pd.ProductId)
                .ToListAsync();

            return await _context.Products
                .Where(p => !productsInDiscount.Contains(p.Id))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<bool> AddProductToDiscountAsync(int discountId, int productId)
        {
            var exists = await _context.ProductDiscounts
                .AnyAsync(pd => pd.DiscountId == discountId && pd.ProductId == productId);

            if (exists) return false;

            var productDiscount = new ProductDiscount
            {
                DiscountId = discountId,
                ProductId = productId
            };

            _context.ProductDiscounts.Add(productDiscount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveProductFromDiscountAsync(int discountId, int productId)
        {
            var productDiscount = await _context.ProductDiscounts
                .FirstOrDefaultAsync(pd => pd.DiscountId == discountId && pd.ProductId == productId);

            if (productDiscount == null) return false;

            _context.ProductDiscounts.Remove(productDiscount);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateDiscountForProductAsync(int productId)
        {
            var now = DateTime.Now;
            var activeDiscounts = await _context.ProductDiscounts
                .Where(pd => pd.ProductId == productId)
                .Include(pd => pd.Discount)
                .Where(pd => pd.Discount.IsActive && 
                           pd.Discount.ValidFrom <= now && 
                           pd.Discount.ValidUntil >= now)
                .Select(pd => pd.Discount)
                .ToListAsync();

            if (!activeDiscounts.Any()) return 0;

            // Pegar o maior desconto disponível
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return 0;

            decimal maxDiscount = 0;
            foreach (var discount in activeDiscounts)
            {
                decimal currentDiscount = 0;
                
                if (discount.DiscountAmount.HasValue)
                {
                    currentDiscount = discount.DiscountAmount.Value;
                }
                else if (discount.DiscountPercentage.HasValue)
                {
                    currentDiscount = product.Price * (discount.DiscountPercentage.Value / 100);
                }

                if (currentDiscount > maxDiscount)
                {
                    maxDiscount = currentDiscount;
                }
            }

            return maxDiscount;
        }

        public async Task<List<Discount>> GetActiveDiscountsAsync()
        {
            var now = DateTime.Now;
            return await _context.Discounts
                .Where(d => d.IsActive && d.ValidFrom <= now && d.ValidUntil >= now)
                .Include(d => d.ProductDiscounts)
                .ThenInclude(pd => pd.Product)
                .ToListAsync();
        }

        public async Task<Discount?> GetActiveDiscountForProductAsync(int productId)
        {
            var now = DateTime.Now;
            var discounts = await _context.Discounts
                .Include(d => d.ProductDiscounts)
                .Where(d => d.IsActive && 
                           d.ValidFrom <= now && 
                           d.ValidUntil >= now &&
                           d.ProductDiscounts.Any(pd => pd.ProductId == productId))
                .ToListAsync();

            // Order on client side to avoid SQLite decimal ordering issues
            return discounts
                .OrderByDescending(d => d.DiscountPercentage ?? 0)
                .ThenByDescending(d => d.DiscountAmount ?? 0)
                .FirstOrDefault();
        }

        public decimal CalculateDiscountedPrice(decimal originalPrice, Discount discount)
        {
            if (discount.DiscountPercentage.HasValue)
            {
                return originalPrice * (1 - discount.DiscountPercentage.Value / 100);
            }
            else if (discount.DiscountAmount.HasValue)
            {
                var discountedPrice = originalPrice - discount.DiscountAmount.Value;
                return discountedPrice > 0 ? discountedPrice : 0;
            }
            return originalPrice;
        }

        public async Task<List<Product>> GetAvailableProductsForDiscountAsync(int discountId)
        {
            var assignedProductIds = await _context.ProductDiscounts
                .Where(pd => pd.DiscountId == discountId)
                .Select(pd => pd.ProductId)
                .ToListAsync();

            return await _context.Products
                .Where(p => !assignedProductIds.Contains(p.Id))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}
