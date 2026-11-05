
using CampusPlacementManagement.Data;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories
{
    public class PlacementOfficerRepository : IPlacementOfficerRepository
    {
        private readonly ApplicationDbContext _context;

        public PlacementOfficerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new placement officer
        public async Task AddAsync(PlacementOfficer placementOfficer)
        {
            await _context.PlacementOfficers.AddAsync(placementOfficer);
            await _context.SaveChangesAsync();
        }

        // Get all placement officers
        public async Task<List<PlacementOfficer>> GetAllAsync()
        {
            return await _context.PlacementOfficers
                .Include(po => po.User)
                .Include(po => po.College)
                .ToListAsync();
        }

        // Get placement officer by ID
        public async Task<PlacementOfficer?> GetByIdAsync(int placementOfficerId)
        {
            return await _context.PlacementOfficers
                .Include(po => po.User)
                .Include(po => po.College)
                .FirstOrDefaultAsync(
                    po => po.PlacementOfficerId == placementOfficerId);
        }

        // Get placement officer by Identity User ID
        public async Task<PlacementOfficer?> GetByUserIdAsync(string userId)
        {
            return await _context.PlacementOfficers
                .Include(po => po.User)
                .Include(po => po.College)
                .FirstOrDefaultAsync(po => po.UserId == userId);
        }

        // Get placement officers awaiting approval
        public async Task<List<PlacementOfficer>> GetPendingAsync()
        {
            return await _context.PlacementOfficers
                .Include(po => po.User)
                .Include(po => po.College)
                .Where(po => po.Status == AccountStatus.Pending)
                .ToListAsync();
        }

        // Update placement officer details or status
        public async Task UpdateAsync(PlacementOfficer placementOfficer)
        {
            _context.PlacementOfficers.Update(placementOfficer);
            await _context.SaveChangesAsync();
        }
    }
}
