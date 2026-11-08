
using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.DTOs
{
    public class CompanyResponseDTO
    {
        public int CompanyId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string CompanyEmail { get; set; } = string.Empty;

        public string? Website { get; set; }

        public string Industry { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string? Description { get; set; }

        public AccountStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
