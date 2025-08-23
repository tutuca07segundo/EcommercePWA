using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class Review
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsVerifiedPurchase { get; set; } = false;
        
        public bool IsApproved { get; set; } = true;
        
        public int? HelpfulVotes { get; set; } = 0;
        
        public int? UnhelpfulVotes { get; set; } = 0;
    }
}
