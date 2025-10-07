namespace EcommercePWA.Models
{
    /// <summary>
    /// Modelo para encapsular todos os cálculos de totais do carrinho
    /// </summary>
    public class CartTotals
    {
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal FinalTotal { get; set; }
        public string? CouponCode { get; set; }
        public bool HasDiscount => DiscountAmount > 0;
        public bool HasShipping => ShippingCost > 0;
    }
}
