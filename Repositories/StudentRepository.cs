
using CampusPlacementManagement.Data;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new student
        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }

        // Get all students
        public async Task<List<Student>> GetAllAsync()
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.College)
                .Include(s => s.PlacementOfficer)
                .ToListAsync();
        }

        // Get student by ID
        public async Task<Student?> GetByIdAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.College)
                .Include(s => s.PlacementOfficer)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        // Get student by Identity User ID
        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.College)
                .Include(s => s.PlacementOfficer)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        // Get students belonging to a particular college
        public async Task<List<Student>> GetByCollegeIdAsync(int collegeId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.College)
                .Include(s => s.PlacementOfficer)
                .Where(s => s.CollegeId == collegeId)
                .ToListAsync();
        }

        // Get pending students belonging to a particular college
        public async Task<List<Student>> GetPendingByCollegeIdAsync(int collegeId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.College)
                .Include(s => s.PlacementOfficer)
                .Where(s =>
                    s.CollegeId == collegeId &&
                    s.Status == AccountStatus.Pending)
                .ToListAsync();
        }

        // Check whether register number already exists in the college
        public async Task<bool> RegisterNumberExistsAsync(
            int collegeId,
            string registerNumber)
        {
            return await _context.Students
                .AnyAsync(s =>
                    s.CollegeId == collegeId &&
                    s.RegisterNumber == registerNumber);
        }

        // Update student details or status
        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }
    }
}
