
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.DTOs
{
    public class RegisterCompanyDTO
    {
        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string CompanyEmail { get; set; } = string.Empty;

        [MaxLength(255)]
        [Url]
        public string? Website { get; set; }

        [Required]
        [MaxLength(100)]
        public string Industry { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
