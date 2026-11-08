
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IPlacementOfficerRepository _placementOfficerRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentService(
            IStudentRepository studentRepository,
            IPlacementOfficerRepository placementOfficerRepository,
            UserManager<ApplicationUser> userManager)
        {
            _studentRepository = studentRepository;
            _placementOfficerRepository = placementOfficerRepository;
            _userManager = userManager;
        }

        // Convert Student entity into response DTO
        private static StudentResponseDTO MapToResponse(Student student)
        {
            return new StudentResponseDTO
            {
                StudentId = student.StudentId,
                FullName = student.User?.FullName ?? string.Empty,
                Email = student.User?.Email ?? string.Empty,
                PhoneNumber = student.User?.PhoneNumber ?? string.Empty,
                CollegeId = student.CollegeId,
                CollegeName = student.College?.CollegeName ?? string.Empty,
                PlacementOfficerId = student.PlacementOfficerId,
                RegisterNumber = student.RegisterNumber,
                Department = student.Department,
                Degree = student.Degree,
                GraduationYear = student.GraduationYear,
                CGPA = student.CGPA,
                ActiveBacklogs = student.ActiveBacklogs,
                ResumeUrl = student.ResumeUrl,
                Status = student.Status,
                ApprovedAt = student.ApprovedAt
            };
        }

        // Verify the placement officer and college
        private async Task<PlacementOfficer> GetApprovedOfficerAsync(
            string placementOfficerUserId)
        {
            var officer = await _placementOfficerRepository
                .GetByUserIdAsync(placementOfficerUserId);

            if (officer == null ||
                officer.User.AccountStatus != AccountStatus.Approved ||
                officer.Status != AccountStatus.Approved)
            {
                throw new InvalidOperationException(
                    "An approved placement officer account is required.");
            }

            if (officer.College.Status != AccountStatus.Approved)
            {
                throw new InvalidOperationException(
                    "Your college is not currently approved.");
            }

            return officer;
        }

        // Enroll a new student
        public async Task<StudentResponseDTO> EnrollAsync(
            EnrollStudentDTO studentDTO,
            string placementOfficerUserId)
        {
            // 1. Verify the logged-in placement officer
            var officer = await GetApprovedOfficerAsync(
                placementOfficerUserId);

            // 2. Normalize register number
            var registerNumber = studentDTO.RegisterNumber
                .Trim()
                .ToUpperInvariant();

            // 3. Check duplicate register number within college
            bool registerNumberExists = await _studentRepository
                .RegisterNumberExistsAsync(
                    officer.CollegeId,
                    registerNumber);

            if (registerNumberExists)
            {
                throw new InvalidOperationException(
                    "This register number is already registered in your college.");
            }

            // 4. Check whether email already exists
            var existingUser = await _userManager
                .FindByEmailAsync(studentDTO.Email.Trim());

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "This email address is already registered.");
            }

            // 5. Create Identity user
            var user = new ApplicationUser
            {
                UserName = studentDTO.Email.Trim(),
                Email = studentDTO.Email.Trim(),
                FullName = studentDTO.FullName.Trim(),
                PhoneNumber = studentDTO.PhoneNumber,
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create account without a password.
            // Password setup will happen through account activation.
            var identityResult = await _userManager.CreateAsync(user);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    identityResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }

            // 6. Create student profile
            var student = new Student
            {
                UserId = user.Id,
                User = user,
                CollegeId = officer.CollegeId,
                College = officer.College,
                PlacementOfficerId = officer.PlacementOfficerId,
                PlacementOfficer = officer,
                RegisterNumber = registerNumber,
                Department = studentDTO.Department.Trim(),
                Degree = studentDTO.Degree.Trim(),
                GraduationYear = studentDTO.GraduationYear,
                CGPA = studentDTO.CGPA,
                ActiveBacklogs = studentDTO.ActiveBacklogs,
                ResumeUrl = studentDTO.ResumeUrl,
                Status = AccountStatus.Pending
            };

            try
            {
                await _studentRepository.AddAsync(student);
            }
            catch
            {
                // Delete the Identity account if profile creation fails
                await _userManager.DeleteAsync(user);
                throw;
            }

            // 7. Retrieve saved student profile
            var savedStudent = await _studentRepository
                .GetByUserIdAsync(user.Id);

            if (savedStudent == null)
            {
                throw new InvalidOperationException(
                    "Student profile could not be retrieved.");
            }

            return MapToResponse(savedStudent);
        }

        // Get all students
        public async Task<List<StudentResponseDTO>> GetAllAsync()
        {
            var students = await _studentRepository.GetAllAsync();

            return students.Select(MapToResponse).ToList();
        }

        // Get student by ID
        public async Task<StudentResponseDTO?> GetByIdAsync(int studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);

            return student == null ? null : MapToResponse(student);
        }

        // Get student by Identity User ID
        public async Task<StudentResponseDTO?> GetByUserIdAsync(string userId)
        {
            var student = await _studentRepository.GetByUserIdAsync(userId);

            return student == null ? null : MapToResponse(student);
        }

        // Get students by college
        public async Task<List<StudentResponseDTO>> GetByCollegeIdAsync(
            int collegeId)
        {
            var students = await _studentRepository
                .GetByCollegeIdAsync(collegeId);

            return students.Select(MapToResponse).ToList();
        }

        // Get pending students by college
        public async Task<List<StudentResponseDTO>> GetPendingByCollegeIdAsync(
            int collegeId)
        {
            var students = await _studentRepository
                .GetPendingByCollegeIdAsync(collegeId);

            return students.Select(MapToResponse).ToList();
        }

        // Approve student enrollment
        public async Task<bool> ApproveAsync(
            int studentId,
            string placementOfficerUserId)
        {
            // 1. Verify placement officer
            var officer = await GetApprovedOfficerAsync(
                placementOfficerUserId);

            // 2. Find student
            var student = await _studentRepository.GetByIdAsync(studentId);

            // 3. Check college ownership and pending status
            if (student == null ||
                student.CollegeId != officer.CollegeId ||
                student.Status != AccountStatus.Pending)
            {
                return false;
            }

            // 4. Find Identity user
            var user = await _userManager.FindByIdAsync(student.UserId);

            if (user == null)
                return false;

            // 5. Assign Student role
            if (!await _userManager.IsInRoleAsync(user, Roles.Student))
            {
                var roleResult = await _userManager
                    .AddToRoleAsync(user, Roles.Student);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(errors);
                }
            }

            // 6. Activate account
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

            // 7. Update student profile
            student.Status = AccountStatus.Approved;
            student.ApprovedAt = DateTime.UtcNow;

            await _studentRepository.UpdateAsync(student);

            return true;
        }

        // Reject student enrollment
        public async Task<bool> RejectAsync(
            int studentId,
            string placementOfficerUserId)
        {
            // 1. Verify placement officer
            var officer = await GetApprovedOfficerAsync(
                placementOfficerUserId);

            // 2. Find student
            var student = await _studentRepository.GetByIdAsync(studentId);

            if (student == null ||
                student.CollegeId != officer.CollegeId ||
                student.Status != AccountStatus.Pending)
            {
                return false;
            }

            // 3. Find Identity user
            var user = await _userManager.FindByIdAsync(student.UserId);

            if (user == null)
                return false;

            // 4. Update account status
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

            // 5. Update student profile
            student.Status = AccountStatus.Rejected;

            await _studentRepository.UpdateAsync(student);

            return true;
        }

        // Suspend student
        public async Task<bool> SuspendAsync(
            int studentId,
            string? placementOfficerUserId)
        {
            Student? student;

            // If an officer ID is provided, verify ownership
            if (placementOfficerUserId != null)
            {
                var officer = await GetApprovedOfficerAsync(
                    placementOfficerUserId);

                student = await _studentRepository.GetByIdAsync(studentId);

                if (student == null ||
                    student.CollegeId != officer.CollegeId)
                {
                    return false;
                }
            }
            else
            {
                // SuperAdmin callers can pass null after controller authorization
                student = await _studentRepository.GetByIdAsync(studentId);
            }

            if (student == null ||
                student.Status != AccountStatus.Approved)
            {
                return false;
            }

            // Find Identity user
            var user = await _userManager.FindByIdAsync(student.UserId);

            if (user == null)
                return false;

            // Suspend account
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

            // Update student profile
            student.Status = AccountStatus.Suspended;

            await _studentRepository.UpdateAsync(student);

            return true;
        }
    }
}
