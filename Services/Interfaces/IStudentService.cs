
using CampusPlacementManagement.DTOs;

namespace CampusPlacementManagement.Services.Interfaces
{
    public interface IStudentService
    {
        // Enroll a new student
        Task<StudentResponseDTO> EnrollAsync(
            EnrollStudentDTO studentDTO,
            string placementOfficerUserId);

        // Retrieve all students
        Task<List<StudentResponseDTO>> GetAllAsync();

        // Retrieve student by ID
        Task<StudentResponseDTO?> GetByIdAsync(int studentId);

        // Retrieve student using Identity User ID
        Task<StudentResponseDTO?> GetByUserIdAsync(string userId);

        // Retrieve students belonging to a college
        Task<List<StudentResponseDTO>> GetByCollegeIdAsync(int collegeId);

        // Retrieve pending students belonging to a college
        Task<List<StudentResponseDTO>> GetPendingByCollegeIdAsync(
            int collegeId);

        // Approve student enrollment
        Task<bool> ApproveAsync(
            int studentId,
            string placementOfficerUserId);

        // Reject student enrollment
        Task<bool> RejectAsync(
            int studentId,
            string placementOfficerUserId);

        // Suspend student
        Task<bool> SuspendAsync(
            int studentId,
            string? placementOfficerUserId);
    }
}
