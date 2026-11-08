
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.Models
{
    public class CompanyHR
    {
        public int CompanyHRId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int CompanyId { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;

        public Company Company { get; set; } = null!;

        public ApplicationUser? Approver { get; set; }
    }
}
