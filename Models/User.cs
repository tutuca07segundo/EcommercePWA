using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Role { get; set; } = "User"; // "User" ou "Admin"
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; } = true;

        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
}
