
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Services
{
    public class PlacementOfficerService : IPlacementOfficerService
    {
        private readonly IPlacementOfficerRepository _placementOfficerRepository;
        private readonly ICollegeRepository _collegeRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public PlacementOfficerService(
            IPlacementOfficerRepository placementOfficerRepository,
            ICollegeRepository collegeRepository,
            UserManager<ApplicationUser> userManager)
        {
            _placementOfficerRepository = placementOfficerRepository;
            _collegeRepository = collegeRepository;
            _userManager = userManager;
        }

        // Convert PlacementOfficer entity into response DTO
        private static PlacementOfficerResponseDTO MapToResponse(
            PlacementOfficer placementOfficer)
        {
            return new PlacementOfficerResponseDTO
            {
                PlacementOfficerId = placementOfficer.PlacementOfficerId,
                FullName = placementOfficer.User?.FullName ?? string.Empty,
                Email = placementOfficer.User?.Email ?? string.Empty,
                PhoneNumber = placementOfficer.User?.PhoneNumber ?? string.Empty,
                CollegeId = placementOfficer.CollegeId,
                CollegeName = placementOfficer.College?.CollegeName ?? string.Empty,
                CollegeCode = placementOfficer.College?.CollegeCode ?? string.Empty,
                Designation = placementOfficer.Designation,
                Status = placementOfficer.Status,
                ApprovedAt = placementOfficer.ApprovedAt
            };
        }

        // Register a new placement officer
        public async Task<PlacementOfficerResponseDTO> RegisterAsync(
            RegisterPlacementOfficerDTO placementOfficerDTO)
        {
            // 1. Find the selected college
            var college = await _collegeRepository
                .GetByIdAsync(placementOfficerDTO.CollegeId);

            if (college == null)
                throw new InvalidOperationException("College not found.");

            // 2. Check college approval status
            if (college.Status != AccountStatus.Approved)
                throw new InvalidOperationException(
                    "Placement officer registration is allowed only for approved colleges.");

            // 3. Check whether email already exists
            var existingUser = await _userManager
                .FindByEmailAsync(placementOfficerDTO.Email);

            if (existingUser != null)
                throw new InvalidOperationException(
                    "Email address is already registered.");

            // 4. Create Identity user
            var user = new ApplicationUser
            {
                UserName = placementOfficerDTO.Email.Trim(),
                Email = placementOfficerDTO.Email.Trim(),
                FullName = placementOfficerDTO.FullName.Trim(),
                PhoneNumber = placementOfficerDTO.PhoneNumber,
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 5. Create Identity account
            var identityResult = await _userManager
                .CreateAsync(user, placementOfficerDTO.Password);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    identityResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }

            // 6. Create placement officer profile
            var placementOfficer = new PlacementOfficer
            {
                UserId = user.Id,
                User = user,
                CollegeId = college.CollegeId,
                College = college,
                Designation = placementOfficerDTO.Designation.Trim(),
                Status = AccountStatus.Pending
            };

            await _placementOfficerRepository.AddAsync(placementOfficer);

            // 7. Retrieve saved profile with related information
            var savedOfficer = await _placementOfficerRepository
                .GetByUserIdAsync(user.Id);

            if (savedOfficer == null)
                throw new InvalidOperationException(
                    "Placement officer profile could not be retrieved.");

            // 8. Return only required information
            return MapToResponse(savedOfficer);
        }

        // Get all placement officers
        public async Task<List<PlacementOfficerResponseDTO>> GetAllAsync()
        {
            var officers = await _placementOfficerRepository.GetAllAsync();

            return officers.Select(MapToResponse).ToList();
        }

        // Get placement officer by ID
        public async Task<PlacementOfficerResponseDTO?> GetByIdAsync(
            int placementOfficerId)
        {
            var officer = await _placementOfficerRepository
                .GetByIdAsync(placementOfficerId);

            return officer == null ? null : MapToResponse(officer);
        }

        // Get placement officer by Identity User ID
        public async Task<PlacementOfficerResponseDTO?> GetByUserIdAsync(
            string userId)
        {
            var officer = await _placementOfficerRepository
                .GetByUserIdAsync(userId);

            return officer == null ? null : MapToResponse(officer);
        }

        // Get pending placement officers
        public async Task<List<PlacementOfficerResponseDTO>> GetPendingAsync()
        {
            var officers = await _placementOfficerRepository.GetPendingAsync();

            return officers.Select(MapToResponse).ToList();
        }

        // Approve placement officer
        public async Task<bool> ApproveAsync(
            int placementOfficerId,
            string approvedByUserId)
        {
            var officer = await _placementOfficerRepository
                .GetByIdAsync(placementOfficerId);

            if (officer == null ||
                officer.Status != AccountStatus.Pending)
                return false;

            var user = await _userManager.FindByIdAsync(officer.UserId);

            if (user == null)
                return false;

            // Assign PlacementOfficer role
            if (!await _userManager.IsInRoleAsync(
                user, Roles.PlacementOfficer))
            {
                var roleResult = await _userManager
                    .AddToRoleAsync(user, Roles.PlacementOfficer);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(errors);
                }
            }

            // Activate Identity account
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

            // Update officer approval information
            officer.Status = AccountStatus.Approved;
            officer.ApprovedBy = approvedByUserId;
            officer.ApprovedAt = DateTime.UtcNow;

            await _placementOfficerRepository.UpdateAsync(officer);

            return true;
        }

        // Reject placement officer
        public async Task<bool> RejectAsync(int placementOfficerId)
        {
            var officer = await _placementOfficerRepository
                .GetByIdAsync(placementOfficerId);

            if (officer == null ||
                officer.Status != AccountStatus.Pending)
                return false;

            var user = await _userManager.FindByIdAsync(officer.UserId);

            if (user == null)
                return false;

            officer.Status = AccountStatus.Rejected;

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

            await _placementOfficerRepository.UpdateAsync(officer);

            return true;
        }

        // Suspend placement officer
        public async Task<bool> SuspendAsync(int placementOfficerId)
        {
            var officer = await _placementOfficerRepository
                .GetByIdAsync(placementOfficerId);

            if (officer == null ||
                officer.Status != AccountStatus.Approved)
                return false;

            var user = await _userManager.FindByIdAsync(officer.UserId);

            if (user == null)
                return false;

            officer.Status = AccountStatus.Suspended;

            user.AccountStatus = AccountStatus.Suspended;
            user.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    updateResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }

            await _placementOfficerRepository.UpdateAsync(officer);

            return true;
        }
    }
}
