
using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.DTOs
{
    public class PlacementOfficerResponseDTO
    {
        public int PlacementOfficerId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public int CollegeId { get; set; }

        public string CollegeName { get; set; } = string.Empty;

        public string CollegeCode { get; set; } = string.Empty;

        public string Designation { get; set; } = string.Empty;

        public AccountStatus Status { get; set; }

        public DateTime? ApprovedAt { get; set; }
    }
}
