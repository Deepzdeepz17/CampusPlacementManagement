
using CampusPlacementManagement.Data;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories
{
    public class CompanyHRRepository : ICompanyHRRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyHRRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new CompanyHR profile
        public async Task AddAsync(CompanyHR companyHR)
        {
            await _context.CompanyHRs.AddAsync(companyHR);
            await _context.SaveChangesAsync();
        }

        // Get all CompanyHR profiles
        public async Task<List<CompanyHR>> GetAllAsync()
        {
            return await _context.CompanyHRs
                .Include(hr => hr.User)
                .Include(hr => hr.Company)
                .ToListAsync();
        }

        // Get CompanyHR by ID
        public async Task<CompanyHR?> GetByIdAsync(int companyHRId)
        {
            return await _context.CompanyHRs
                .Include(hr => hr.User)
                .Include(hr => hr.Company)
                .FirstOrDefaultAsync(
                    hr => hr.CompanyHRId == companyHRId);
        }

        // Get CompanyHR by Identity User ID
        public async Task<CompanyHR?> GetByUserIdAsync(string userId)
        {
            return await _context.CompanyHRs
                .Include(hr => hr.User)
                .Include(hr => hr.Company)
                .FirstOrDefaultAsync(hr => hr.UserId == userId);
        }

        // Get CompanyHR profiles awaiting approval
        public async Task<List<CompanyHR>> GetPendingAsync()
        {
            return await _context.CompanyHRs
                .Include(hr => hr.User)
                .Include(hr => hr.Company)
                .Where(hr =>
                    hr.User.AccountStatus == AccountStatus.Pending)
                .ToListAsync();
        }

        // Update CompanyHR profile
        public async Task UpdateAsync(CompanyHR companyHR)
        {
            _context.CompanyHRs.Update(companyHR);
            await _context.SaveChangesAsync();
        }
    }
}
