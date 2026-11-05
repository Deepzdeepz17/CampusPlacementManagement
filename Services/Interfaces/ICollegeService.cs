
using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface ICollegeService
    {
        Task<College> RegisterAsync(College college);

        Task<List<College>> GetAllAsync();

        Task<College?> GetByIdAsync(int collegeId);

        Task<List<College>> GetPendingAsync();

        Task<bool> ApproveAsync(int collegeId);

        Task<bool> RejectAsync(int collegeId);

        Task<bool> SuspendAsync(int collegeId);

        Task<bool> UpdateAsync(int collegeId, College college);
    }
}
