
using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface ICompanyHRRepository
    {
        // Add a new CompanyHR profile
        Task AddAsync(CompanyHR companyHR);

        // Get all CompanyHR profiles
        Task<List<CompanyHR>> GetAllAsync();

        // Get CompanyHR by ID
        Task<CompanyHR?> GetByIdAsync(int companyHRId);

        // Get CompanyHR using Identity User ID
        Task<CompanyHR?> GetByUserIdAsync(string userId);

        // Get CompanyHR profiles awaiting approval
        Task<List<CompanyHR>> GetPendingAsync();

        // Update CompanyHR details
        Task UpdateAsync(CompanyHR companyHR);
    }
}
