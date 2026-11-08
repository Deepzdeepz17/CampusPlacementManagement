
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        // Convert Company entity into response DTO
        private static CompanyResponseDTO MapToResponse(Company company)
        {
            return new CompanyResponseDTO
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                CompanyEmail = company.CompanyEmail,
                Website = company.Website,
                Industry = company.Industry,
                Address = company.Address,
                Description = company.Description,
                Status = company.Status,
                CreatedAt = company.CreatedAt
            };
        }

        // Register a new company
        public async Task<CompanyResponseDTO> RegisterAsync(
            RegisterCompanyDTO companyDTO)
        {
            var company = new Company
            {
                CompanyName = companyDTO.CompanyName.Trim(),
                CompanyEmail = companyDTO.CompanyEmail.Trim(),
                Website = companyDTO.Website,
                Industry = companyDTO.Industry.Trim(),
                Address = companyDTO.Address.Trim(),
                Description = companyDTO.Description,
                Status = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _companyRepository.AddAsync(company);

            return MapToResponse(company);
        }

        // Get all companies
        public async Task<List<CompanyResponseDTO>> GetAllAsync()
        {
            var companies = await _companyRepository.GetAllAsync();

            return companies.Select(MapToResponse).ToList();
        }

        // Get company by ID
        public async Task<CompanyResponseDTO?> GetByIdAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            return company == null ? null : MapToResponse(company);
        }

        // Get pending companies
        public async Task<List<CompanyResponseDTO>> GetPendingAsync()
        {
            var companies = await _companyRepository.GetPendingAsync();

            return companies.Select(MapToResponse).ToList();
        }

        // Approve company
        public async Task<bool> ApproveAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null ||
                company.Status != AccountStatus.Pending)
                return false;

            company.Status = AccountStatus.Approved;

            await _companyRepository.UpdateAsync(company);

            return true;
        }

        // Reject company
        public async Task<bool> RejectAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null ||
                company.Status != AccountStatus.Pending)
                return false;

            company.Status = AccountStatus.Rejected;

            await _companyRepository.UpdateAsync(company);

            return true;
        }

        // Suspend company
        public async Task<bool> SuspendAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null ||
                company.Status != AccountStatus.Approved)
                return false;

            company.Status = AccountStatus.Suspended;

            await _companyRepository.UpdateAsync(company);

            return true;
        }

        // Update company details
        public async Task<bool> UpdateAsync(
            int companyId,
            RegisterCompanyDTO companyDTO)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null)
                return false;

            company.CompanyName = companyDTO.CompanyName.Trim();
            company.CompanyEmail = companyDTO.CompanyEmail.Trim();
            company.Website = companyDTO.Website;
            company.Industry = companyDTO.Industry.Trim();
            company.Address = companyDTO.Address.Trim();
            company.Description = companyDTO.Description;

            await _companyRepository.UpdateAsync(company);

            return true;
        }
    }
}
