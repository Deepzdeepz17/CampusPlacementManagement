
using CampusPlacementManagement.DTOs;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface ICompanyHRService
    {
        // Register a new CompanyHR
        Task<CompanyHRResponseDTO> RegisterAsync(
            RegisterCompanyHRDTO companyHRDTO);

        // Get all CompanyHR profiles
        Task<List<CompanyHRResponseDTO>> GetAllAsync();

        // Get CompanyHR by ID
        Task<CompanyHRResponseDTO?> GetByIdAsync(int companyHRId);

        // Get CompanyHR using Identity User ID
        Task<CompanyHRResponseDTO?> GetByUserIdAsync(string userId);

        // Get CompanyHR profiles awaiting approval
        Task<List<CompanyHRResponseDTO>> GetPendingAsync();

        // Approve CompanyHR
        Task<bool> ApproveAsync(
            int companyHRId,
            string approvedByUserId);

        // Reject CompanyHR
        Task<bool> RejectAsync(int companyHRId);
    }
}
