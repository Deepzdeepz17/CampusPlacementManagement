
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
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

        // Register a new placement officer
        public async Task<PlacementOfficer> RegisterAsync(
            RegisterPlacementOfficerDTO placementOfficerDTO)
        {
            // 1. Check whether the selected college exists
            var college = await _collegeRepository
                .GetByIdAsync(placementOfficerDTO.CollegeId);

            if (college == null)
                throw new InvalidOperationException("College not found.");

            // 2. Allow registration only for approved colleges
            if (college.Status != AccountStatus.Approved)
                throw new InvalidOperationException(
                    "Placement officer registration is allowed only for approved colleges.");

            // 3. Check whether the email already exists
            var existingUser = await _userManager
                .FindByEmailAsync(placementOfficerDTO.Email);

            if (existingUser != null)
                throw new InvalidOperationException(
                    "Email address is already registered.");

            // 4. Create Identity user
            var user = new ApplicationUser
            {
                UserName = placementOfficerDTO.Email,
                Email = placementOfficerDTO.Email,
                FullName = placementOfficerDTO.FullName.Trim(),
                PhoneNumber = placementOfficerDTO.PhoneNumber,
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 5. Create the user with the supplied password
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
                CollegeId = placementOfficerDTO.CollegeId,
                Designation = placementOfficerDTO.Designation.Trim(),
                Status = AccountStatus.Pending
            };

            await _placementOfficerRepository.AddAsync(placementOfficer);

            // 7. Return the created profile
            return placementOfficer;
        }

        // Get all placement officers
        public async Task<List<PlacementOfficer>> GetAllAsync()
        {
            return await _placementOfficerRepository.GetAllAsync();
        }

        // Get placement officer by ID
        public async Task<PlacementOfficer?> GetByIdAsync(
            int placementOfficerId)
        {
            return await _placementOfficerRepository
                .GetByIdAsync(placementOfficerId);
        }

        // Get placement officer by Identity User ID
        public async Task<PlacementOfficer?> GetByUserIdAsync(string userId)
        {
            return await _placementOfficerRepository
                .GetByUserIdAsync(userId);
        }

        // Get pending placement officers
        public async Task<List<PlacementOfficer>> GetPendingAsync()
        {
            return await _placementOfficerRepository.GetPendingAsync();
        }

        // Approve placement officer
        public async Task<bool> ApproveAsync(
            int placementOfficerId,
            string approvedByUserId)
        {
            var placementOfficer = await _placementOfficerRepository
                .GetByIdAsync(placementOfficerId);

            if (placementOfficer == null ||
                placementOfficer.Status != AccountStatus.Pending)
                return false;

            var user = await _userManager
                .FindByIdAsync(placementOfficer.UserId);

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

            // Update placement officer approval details
            placementOfficer.Status = AccountStatus.Approved;
            placementOfficer.ApprovedBy = approvedByUserId;
            placementOfficer.ApprovedAt = DateTime.UtcNow;

            await _placementOfficerRepository.UpdateAsync(placementOfficer);

            return true;
        }

        // Reject placement officer
        public async Task<bool> RejectAsync(int placementOfficerId)
        {
            var placementOfficer = await _placementOfficerRepository
                .GetByIdAsync(placementOfficerId);

            if (placementOfficer == null ||
                placementOfficer.Status != AccountStatus.Pending)
                return false;

            var user = await _userManager
                .FindByIdAsync(placementOfficer.UserId);

            if (user == null)
                return false;

            placementOfficer.Status = AccountStatus.Rejected;

            user.AccountStatus = AccountStatus.Rejected;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            await _placementOfficerRepository.UpdateAsync(placementOfficer);

            return true;
        }

        // Suspend placement officer
        public async Task<bool> SuspendAsync(int placementOfficerId)
        {
            var placementOfficer = await _placementOfficerRepository
                .GetByIdAsync(placementOfficerId);

            if (placementOfficer == null ||
                placementOfficer.Status != AccountStatus.Approved)
                return false;

            var user = await _userManager
                .FindByIdAsync(placementOfficer.UserId);

            if (user == null)
                return false;

            placementOfficer.Status = AccountStatus.Suspended;

            user.AccountStatus = AccountStatus.Suspended;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            await _placementOfficerRepository.UpdateAsync(placementOfficer);

            return true;
        }
    }
}
