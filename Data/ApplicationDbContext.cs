
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // RefreshToken relationship
            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // RefreshToken indexes
            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.TokenHash)
                .IsUnique();

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId);

            // College unique code
            builder.Entity<College>()
                .HasIndex(c => c.CollegeCode)
                .IsUnique();

            // PlacementOfficer - ApplicationUser (one-to-one)
            builder.Entity<PlacementOfficer>()
                .HasOne(po => po.User)
                .WithOne()
                .HasForeignKey<PlacementOfficer>(po => po.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // College - PlacementOfficer (one-to-many)
            builder.Entity<PlacementOfficer>()
                .HasOne(po => po.College)
                .WithMany()
                .HasForeignKey(po => po.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ApplicationUser - Approver (one-to-many)
            builder.Entity<PlacementOfficer>()
                .HasOne(po => po.Approver)
                .WithMany()
                .HasForeignKey(po => po.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
