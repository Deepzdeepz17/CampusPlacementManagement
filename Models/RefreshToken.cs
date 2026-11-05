using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAtUtc { get; set; }

        public DateTime? RevokedAtUtc { get; set; }

        [MaxLength(64)]
        public string? ReplacedByTokenHash { get; set; }

        // Navigation property
        public ApplicationUser User { get; set; } = null!;
    }
}