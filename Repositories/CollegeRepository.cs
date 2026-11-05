
using CampusPlacementManagement.Data;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories
{
    public class CollegeRepository : ICollegeRepository
    {
        private readonly ApplicationDbContext _context;

        public CollegeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new college
        public async Task AddAsync(College college)
        {
            await _context.Colleges.AddAsync(college);
            await _context.SaveChangesAsync();
        }

        // Get all colleges
        public async Task<List<College>> GetAllAsync()
        {
            return await _context.Colleges
                .ToListAsync();
        }

        // Get college by ID
        public async Task<College?> GetByIdAsync(int collegeId)
        {
            return await _context.Colleges
                .FirstOrDefaultAsync(c => c.CollegeId == collegeId);
        }

        // Get colleges awaiting approval
        public async Task<List<College>> GetPendingAsync()
        {
            return await _context.Colleges
                .Where(c => c.Status == AccountStatus.Pending)
                .ToListAsync();
        }

        // Check whether college code already exists
        public async Task<bool> CollegeCodeExistsAsync(string collegeCode)
        {
            return await _context.Colleges
                .AnyAsync(c => c.CollegeCode == collegeCode);
        }

        // Update college details
        public async Task UpdateAsync(College college)
        {
            _context.Colleges.Update(college);
            await _context.SaveChangesAsync();
        }
    }
}
