using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface ICollegeRepository
    {
        Task AddAsync(College college);

        Task<List<College>> GetAllAsync();

        Task<College?> GetByIdAsync(int collegeId);

        Task<List<College>> GetPendingAsync();

        Task<bool> CollegeCodeExistsAsync(string collegeCode);

        Task UpdateAsync(College college);
    }
}