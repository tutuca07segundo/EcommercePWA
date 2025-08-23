using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        public DateTime? ShippedDate { get; set; }
        
        public DateTime? DeliveredDate { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled
        
        [Required]
        [StringLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ShippingCity { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string ShippingZipCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ShippingCountry { get; set; } = string.Empty;
        
        public decimal Subtotal { get; set; }
        
        public decimal ShippingCost { get; set; }
        
        public decimal TaxAmount { get; set; }
        
        public decimal DiscountAmount { get; set; }
        
        public decimal Total { get; set; }
        
        [StringLength(50)]
        public string? CouponCode { get; set; }
        
        [StringLength(100)]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Relacionamentos
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public List<OrderNotification> Notifications { get; set; } = new List<OrderNotification>();
    }
}
