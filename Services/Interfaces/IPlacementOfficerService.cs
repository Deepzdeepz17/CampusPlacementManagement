
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IPlacementOfficerService
    {
        Task<PlacementOfficer> RegisterAsync(
            RegisterPlacementOfficerDTO placementOfficerDTO);

        Task<List<PlacementOfficer>> GetAllAsync();

        Task<PlacementOfficer?> GetByIdAsync(int placementOfficerId);

        Task<PlacementOfficer?> GetByUserIdAsync(string userId);

        Task<List<PlacementOfficer>> GetPendingAsync();

        Task<bool> ApproveAsync(
            int placementOfficerId,
            string approvedByUserId);

        Task<bool> RejectAsync(int placementOfficerId);

        Task<bool> SuspendAsync(int placementOfficerId);
    }
}
