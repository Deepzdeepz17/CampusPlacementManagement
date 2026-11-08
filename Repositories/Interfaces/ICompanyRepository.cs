
using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        // Add a new company
        Task AddAsync(Company company);

        // Get all companies
        Task<List<Company>> GetAllAsync();

        // Get company by ID
        Task<Company?> GetByIdAsync(int companyId);

        // Get companies awaiting approval
        Task<List<Company>> GetPendingAsync();

        // Update company details or status
        Task UpdateAsync(Company company);
    }
}
