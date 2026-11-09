
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.DTOs;

public class JobTargetCollegeDTO
{
    [Required]
    [MinLength(1)]
    public List<int> CollegeIds { get; set; } = new();
}
