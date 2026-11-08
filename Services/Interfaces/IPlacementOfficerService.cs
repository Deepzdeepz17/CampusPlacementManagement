
using CampusPlacementManagement.DTOs;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IPlacementOfficerService
    {
        Task<PlacementOfficerResponseDTO> RegisterAsync(
            RegisterPlacementOfficerDTO placementOfficerDTO);

        Task<List<PlacementOfficerResponseDTO>> GetAllAsync();

        Task<PlacementOfficerResponseDTO?> GetByIdAsync(
            int placementOfficerId);

        Task<PlacementOfficerResponseDTO?> GetByUserIdAsync(
            string userId);

        Task<List<PlacementOfficerResponseDTO>> GetPendingAsync();

        Task<bool> ApproveAsync(
            int placementOfficerId,
            string approvedByUserId);

        Task<bool> RejectAsync(int placementOfficerId);

        Task<bool> SuspendAsync(int placementOfficerId);
    }
}
