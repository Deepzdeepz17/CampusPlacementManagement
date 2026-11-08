
using CampusPlacementManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Database tables
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<College> Colleges { get; set; }
        public DbSet<PlacementOfficer> PlacementOfficers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyHR> CompanyHRs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // RefreshToken relationship
            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenHash)
                .IsUnique();

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId);

            // College unique code
            builder.Entity<College>()
                .HasIndex(c => c.CollegeCode)
                .IsUnique();

            // PlacementOfficer - ApplicationUser
            builder.Entity<PlacementOfficer>()
                .HasOne(po => po.User)
                .WithOne()
                .HasForeignKey<PlacementOfficer>(po => po.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // College - PlacementOfficer
            builder.Entity<PlacementOfficer>()
                .HasOne(po => po.College)
                .WithMany()
                .HasForeignKey(po => po.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ApplicationUser - PlacementOfficer Approver
            builder.Entity<PlacementOfficer>()
                .HasOne(po => po.Approver)
                .WithMany()
                .HasForeignKey(po => po.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Student - ApplicationUser
            builder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<Student>(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // College - Student
            builder.Entity<Student>()
                .HasOne(s => s.College)
                .WithMany()
                .HasForeignKey(s => s.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            // PlacementOfficer - Student
            builder.Entity<Student>()
                .HasOne(s => s.PlacementOfficer)
                .WithMany()
                .HasForeignKey(s => s.PlacementOfficerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique RegisterNumber within each college
            builder.Entity<Student>()
                .HasIndex(s => new
                {
                    s.CollegeId,
                    s.RegisterNumber
                })
                .IsUnique();

            // CompanyHR - ApplicationUser (one-to-one)
            builder.Entity<CompanyHR>()
                .HasOne(hr => hr.User)
                .WithOne()
                .HasForeignKey<CompanyHR>(hr => hr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Company - CompanyHR (one-to-many)
            builder.Entity<CompanyHR>()
                .HasOne(hr => hr.Company)
                .WithMany()
                .HasForeignKey(hr => hr.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ApplicationUser - CompanyHR Approver
            builder.Entity<CompanyHR>()
                .HasOne(hr => hr.Approver)
                .WithMany()
                .HasForeignKey(hr => hr.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
