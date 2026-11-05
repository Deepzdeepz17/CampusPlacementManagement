
using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface IPlacementOfficerRepository
    {
        Task AddAsync(PlacementOfficer placementOfficer);

        Task<List<PlacementOfficer>> GetAllAsync();

        Task<PlacementOfficer?> GetByIdAsync(int placementOfficerId);

        Task<PlacementOfficer?> GetByUserIdAsync(string userId);

        Task<List<PlacementOfficer>> GetPendingAsync();

        Task UpdateAsync(PlacementOfficer placementOfficer);
    }
}
