
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services
{
    public class CollegeService : ICollegeService
    {
        private readonly ICollegeRepository _collegeRepository;

        public CollegeService(ICollegeRepository collegeRepository)
        {
            _collegeRepository = collegeRepository;
        }

        // Register a new college
        public async Task<College> RegisterAsync(College college)
        {
            if (college == null)
                throw new ArgumentNullException(nameof(college));

            college.CollegeCode = college.CollegeCode.Trim().ToUpperInvariant();

            // Check whether college code already exists
            bool exists = await _collegeRepository
                .CollegeCodeExistsAsync(college.CollegeCode);

            if (exists)
                throw new InvalidOperationException(
                    "College code already exists.");

            // New colleges must wait for SuperAdmin approval
            college.Status = AccountStatus.Pending;
            college.CreatedAt = DateTime.UtcNow;

            await _collegeRepository.AddAsync(college);

            return college;
        }

        // Get all colleges
        public async Task<List<College>> GetAllAsync()
        {
            return await _collegeRepository.GetAllAsync();
        }

        // Get college by ID
        public async Task<College?> GetByIdAsync(int collegeId)
        {
            return await _collegeRepository.GetByIdAsync(collegeId);
        }

        // Get pending colleges
        public async Task<List<College>> GetPendingAsync()
        {
            return await _collegeRepository.GetPendingAsync();
        }

        // Approve a college
        public async Task<bool> ApproveAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null || college.Status != AccountStatus.Pending)
                return false;

            college.Status = AccountStatus.Approved;

            await _collegeRepository.UpdateAsync(college);

            return true;
        }

        // Reject a college
        public async Task<bool> RejectAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null || college.Status != AccountStatus.Pending)
                return false;

            college.Status = AccountStatus.Rejected;

            await _collegeRepository.UpdateAsync(college);

            return true;
        }

        // Suspend a college
        public async Task<bool> SuspendAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null || college.Status != AccountStatus.Approved)
                return false;

            college.Status = AccountStatus.Suspended;

            await _collegeRepository.UpdateAsync(college);

            return true;
        }

        // Update college details
        public async Task<bool> UpdateAsync(int collegeId, College college)
        {
            if (college == null)
                return false;

            var existingCollege = await _collegeRepository
                .GetByIdAsync(collegeId);

            if (existingCollege == null)
                return false;

            string newCode = college.CollegeCode.Trim().ToUpperInvariant();

            // Check duplicate code only if the code is changing
            if (!string.Equals(
                existingCollege.CollegeCode,
                newCode,
                StringComparison.OrdinalIgnoreCase))
            {
                bool exists = await _collegeRepository
                    .CollegeCodeExistsAsync(newCode);

                if (exists)
                    return false;
            }

            existingCollege.CollegeName = college.CollegeName;
            existingCollege.CollegeCode = newCode;
            existingCollege.UniversityName = college.UniversityName;
            existingCollege.OfficialEmail = college.OfficialEmail;
            existingCollege.PhoneNumber = college.PhoneNumber;
            existingCollege.Address = college.Address;
            existingCollege.Website = college.Website;

            await _collegeRepository.UpdateAsync(existingCollege);

            return true;
        }
    }
}
