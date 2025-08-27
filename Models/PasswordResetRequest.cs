using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class PasswordResetRequest
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6)]
        public string Code { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool Used { get; set; }
    }
}


