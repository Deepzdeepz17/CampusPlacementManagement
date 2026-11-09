
using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.DTOs;

public class JobResponseDTO
{
    public int JobId { get; set; }

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public int CreatedByHRId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string JobDescription { get; set; } = string.Empty;

    public string JobType { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public decimal? SalaryPackage { get; set; }

    public decimal? MinimumCGPA { get; set; }

    public int? MaximumBacklogs { get; set; }

    public DateTime ApplicationDeadline { get; set; }

    public bool IsOpenToAllColleges { get; set; }

    public JobStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<int> TargetCollegeIds { get; set; } = new();
}
