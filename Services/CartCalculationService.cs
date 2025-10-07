using EcommercePWA.Models;
using EcommercePWA.Constants;

namespace EcommercePWA.Services
{
    public class CartCalculationService : ICartCalculationService
    {
        private readonly ILogger<CartCalculationService> _logger;

        public CartCalculationService(ILogger<CartCalculationService> logger)
        {
            _logger = logger;
        }

        public CartTotals CalculateTotals(Cart cart)
        {
            try
            {
                var subtotal = cart.Subtotal;
                var discountAmount = cart.DiscountAmount ?? 0;
                var shippingCost = cart.ShippingCost ?? 0;
                var taxAmount = CalculateTaxAmount(subtotal);
                var finalTotal = CalculateFinalTotal(subtotal, discountAmount, taxAmount, shippingCost);

                return new CartTotals
                {
                    Subtotal = subtotal,
                    TaxAmount = taxAmount,
                    DiscountAmount = discountAmount,
                    ShippingCost = shippingCost,
                    FinalTotal = finalTotal,
                    CouponCode = cart.CouponCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular totais do carrinho");
                return new CartTotals
                {
                    Subtotal = cart.Subtotal,
                    FinalTotal = cart.Subtotal
                };
            }
        }

        public decimal CalculateTaxAmount(decimal subtotal)
        {
            return subtotal * BusinessConstants.TAX_RATE;
        }

        public decimal CalculateFinalTotal(decimal subtotal, decimal? discountAmount, decimal taxAmount, decimal? shippingCost)
        {
            return subtotal - (discountAmount ?? 0) + taxAmount + (shippingCost ?? 0);
        }
    }
}
