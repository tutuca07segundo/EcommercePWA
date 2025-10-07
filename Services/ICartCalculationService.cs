using EcommercePWA.Models;

namespace EcommercePWA.Services
{
    public interface ICartCalculationService
    {
        CartTotals CalculateTotals(Cart cart);
        decimal CalculateTaxAmount(decimal subtotal);
        decimal CalculateFinalTotal(decimal subtotal, decimal? discountAmount, decimal taxAmount, decimal? shippingCost);
    }
}
