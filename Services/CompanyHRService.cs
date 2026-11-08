
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Services
{
    public class CompanyHRService : ICompanyHRService
    {
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyHRService(
            ICompanyHRRepository companyHRRepository,
            ICompanyRepository companyRepository,
            UserManager<ApplicationUser> userManager)
        {
            _companyHRRepository = companyHRRepository;
            _companyRepository = companyRepository;
            _userManager = userManager;
        }

        // Convert CompanyHR entity into response DTO
        private static CompanyHRResponseDTO MapToResponse(
            CompanyHR companyHR)
        {
            return new CompanyHRResponseDTO
            {
                CompanyHRId = companyHR.CompanyHRId,
                FullName = companyHR.User?.FullName ?? string.Empty,
                Email = companyHR.User?.Email ?? string.Empty,
                PhoneNumber = companyHR.User?.PhoneNumber ?? string.Empty,
                CompanyId = companyHR.CompanyId,
                CompanyName = companyHR.Company?.CompanyName ?? string.Empty,
                Designation = companyHR.Designation,
                Status = companyHR.User.AccountStatus,
                ApprovedAt = companyHR.ApprovedAt
            };
        }

        // Register a new CompanyHR
        public async Task<CompanyHRResponseDTO> RegisterAsync(
            RegisterCompanyHRDTO companyHRDTO)
        {
            // 1. Check whether company exists
            var company = await _companyRepository
                .GetByIdAsync(companyHRDTO.CompanyId);

            if (company == null)
                throw new InvalidOperationException("Company not found.");

            // 2. Check company approval status
            if (company.Status != AccountStatus.Approved)
            {
                throw new InvalidOperationException(
                    "HR registration is allowed only for approved companies.");
            }

            // 3. Check whether email already exists
            var existingUser = await _userManager
                .FindByEmailAsync(companyHRDTO.Email.Trim());

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "Email address is already registered.");
            }

            // 4. Create Identity user
            var user = new ApplicationUser
            {
                UserName = companyHRDTO.Email.Trim(),
                Email = companyHRDTO.Email.Trim(),
                FullName = companyHRDTO.FullName.Trim(),
                PhoneNumber = companyHRDTO.PhoneNumber,
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 5. Create Identity account with password
            var identityResult = await _userManager.CreateAsync(
                user,
                companyHRDTO.Password);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    identityResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }

            // 6. Create CompanyHR profile
            var companyHR = new CompanyHR
            {
                UserId = user.Id,
                User = user,
                CompanyId = company.CompanyId,
                Company = company,
                Designation = companyHRDTO.Designation?.Trim()
            };

            try
            {
                await _companyHRRepository.AddAsync(companyHR);
            }
            catch
            {
                // Remove Identity user if profile creation fails
                await _userManager.DeleteAsync(user);
                throw;
            }

            // 7. Retrieve saved profile
            var savedHR = await _companyHRRepository
                .GetByUserIdAsync(user.Id);

            if (savedHR == null)
            {
                throw new InvalidOperationException(
                    "CompanyHR profile could not be retrieved.");
            }

            // 8. Return safe response DTO
            return MapToResponse(savedHR);
        }

        // Get all CompanyHR profiles
        public async Task<List<CompanyHRResponseDTO>> GetAllAsync()
        {
            var companyHRs = await _companyHRRepository.GetAllAsync();

            return companyHRs.Select(MapToResponse).ToList();
        }

        // Get CompanyHR by ID
        public async Task<CompanyHRResponseDTO?> GetByIdAsync(
            int companyHRId)
        {
            var companyHR = await _companyHRRepository
                .GetByIdAsync(companyHRId);

            return companyHR == null
                ? null
                : MapToResponse(companyHR);
        }

        // Get CompanyHR by Identity User ID
        public async Task<CompanyHRResponseDTO?> GetByUserIdAsync(
            string userId)
        {
            var companyHR = await _companyHRRepository
                .GetByUserIdAsync(userId);

            return companyHR == null
                ? null
                : MapToResponse(companyHR);
        }

        // Get pending CompanyHR profiles
        public async Task<List<CompanyHRResponseDTO>> GetPendingAsync()
        {
            var companyHRs = await _companyHRRepository.GetPendingAsync();

            return companyHRs.Select(MapToResponse).ToList();
        }

        // Approve CompanyHR
        public async Task<bool> ApproveAsync(
            int companyHRId,
            string approvedByUserId)
        {
            // 1. Find CompanyHR profile
            var companyHR = await _companyHRRepository
                .GetByIdAsync(companyHRId);

            if (companyHR == null ||
                companyHR.User.AccountStatus != AccountStatus.Pending)
            {
                return false;
            }

            // 2. Check whether company is still approved
            if (companyHR.Company.Status != AccountStatus.Approved)
                return false;

            var user = companyHR.User;

            // 3. Assign CompanyHR role
            if (!await _userManager.IsInRoleAsync(
                user, Roles.CompanyHR))
            {
                var roleResult = await _userManager
                    .AddToRoleAsync(user, Roles.CompanyHR);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(errors);
                }
            }

            // 4. Activate Identity account
            user.AccountStatus = AccountStatus.Approved;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    updateResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }

            // 5. Save approval information
            companyHR.ApprovedBy = approvedByUserId;
            companyHR.ApprovedAt = DateTime.UtcNow;

            await _companyHRRepository.UpdateAsync(companyHR);

            return true;
        }

        // Reject CompanyHR
        public async Task<bool> RejectAsync(int companyHRId)
        {
            // 1. Find CompanyHR profile
            var companyHR = await _companyHRRepository
                .GetByIdAsync(companyHRId);

            if (companyHR == null ||
                companyHR.User.AccountStatus != AccountStatus.Pending)
            {
                return false;
            }

            // 2. Update Identity account status
            var user = companyHR.User;

            user.AccountStatus = AccountStatus.Rejected;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    updateResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }

            return true;
        }
    }
}
