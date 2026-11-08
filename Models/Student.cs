
using CampusPlacementManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int CollegeId { get; set; }

        public int PlacementOfficerId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RegisterNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Degree { get; set; } = string.Empty;

        [Required]
        public int GraduationYear { get; set; }

        [Required]
        public decimal CGPA { get; set; }

        [Required]
        public int ActiveBacklogs { get; set; }

        [MaxLength(500)]
        public string? ResumeUrl { get; set; }

        public AccountStatus Status { get; set; }
            = AccountStatus.Pending;

        public DateTime? ApprovedAt { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;

        public College College { get; set; } = null!;

        public PlacementOfficer PlacementOfficer { get; set; } = null!;
    }
}
