using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class OrderNotification
    {
        public int Id { get; set; }
        
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Type { get; set; } = "Info"; // Info, Success, Warning, Error
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? ReadAt { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        [StringLength(100)]
        public string? ActionUrl { get; set; } // URL para ação relacionada
    }
}
