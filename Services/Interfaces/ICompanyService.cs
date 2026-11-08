
using CampusPlacementManagement.DTOs;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface ICompanyService
    {
        // Register a new company
        Task<CompanyResponseDTO> RegisterAsync(
            RegisterCompanyDTO companyDTO);

        // Get all companies
        Task<List<CompanyResponseDTO>> GetAllAsync();

        // Get company by ID
        Task<CompanyResponseDTO?> GetByIdAsync(int companyId);

        // Get companies awaiting approval
        Task<List<CompanyResponseDTO>> GetPendingAsync();

        // Approve company
        Task<bool> ApproveAsync(int companyId);

        // Reject company
        Task<bool> RejectAsync(int companyId);

        // Suspend company
        Task<bool> SuspendAsync(int companyId);

        // Update company details
        Task<bool> UpdateAsync(
            int companyId,
            RegisterCompanyDTO companyDTO);
    }
}
