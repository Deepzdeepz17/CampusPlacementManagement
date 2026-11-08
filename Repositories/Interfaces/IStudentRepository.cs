
using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        Task AddAsync(Student student);

        Task<List<Student>> GetAllAsync();

        Task<Student?> GetByIdAsync(int studentId);

        Task<Student?> GetByUserIdAsync(string userId);

        Task<List<Student>> GetByCollegeIdAsync(int collegeId);

        Task<List<Student>> GetPendingByCollegeIdAsync(int collegeId);

        Task<bool> RegisterNumberExistsAsync(
            int collegeId,
            string registerNumber);

        Task UpdateAsync(Student student);
    }
}
