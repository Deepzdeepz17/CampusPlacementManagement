
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

        public DbSet<JobOpening> JobOpenings { get; set; }

        public DbSet<JobTargetCollege> JobTargetColleges { get; set; }

        public DbSet<JobEligibleDepartment> JobEligibleDepartments { get; set; }

        public DbSet<JobEligibleGraduationYear> JobEligibleGraduationYears { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ==========================================
            // 1. REFRESH TOKEN CONFIGURATION
            // ==========================================

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


            // ==========================================
            // 2. COLLEGE CONFIGURATION
            // ==========================================

            builder.Entity<College>()
                .HasIndex(c => c.CollegeCode)
                .IsUnique();


            // ==========================================
            // 3. PLACEMENT OFFICER CONFIGURATION
            // ==========================================

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


            // ==========================================
            // 4. STUDENT CONFIGURATION
            // ==========================================

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

            // CGPA precision
            builder.Entity<Student>()
                .Property(s => s.CGPA)
                .HasPrecision(4, 2);


            // ==========================================
            // 5. COMPANY HR CONFIGURATION
            // ==========================================

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


            // ==========================================
            // 6. JOB OPENING CONFIGURATION
            // ==========================================

            builder.Entity<JobOpening>(entity =>
            {
                entity.HasKey(j => j.JobId);

                entity.Property(j => j.JobTitle)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(j => j.JobDescription)
                    .IsRequired();

                entity.Property(j => j.JobType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(j => j.Location)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(j => j.SalaryPackage)
                    .HasPrecision(12, 2);

                entity.Property(j => j.MinimumCGPA)
                    .HasPrecision(4, 2);

                // Company - JobOpening (one-to-many)
                entity.HasOne(j => j.Company)
                    .WithMany()
                    .HasForeignKey(j => j.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                // CompanyHR - JobOpening (one-to-many)
                entity.HasOne(j => j.CreatedByHR)
                    .WithMany()
                    .HasForeignKey(j => j.CreatedByHRId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // ==========================================
            // 7. JOB TARGET COLLEGE CONFIGURATION
            // ==========================================

            builder.Entity<JobTargetCollege>(entity =>
            {
                entity.HasKey(j => j.JobTargetCollegeId);

                // JobOpening - JobTargetCollege
                entity.HasOne(j => j.Job)
                    .WithMany(j => j.TargetColleges)
                    .HasForeignKey(j => j.JobId)
                    .OnDelete(DeleteBehavior.Restrict);

                // College - JobTargetCollege
                entity.HasOne(j => j.College)
                    .WithMany()
                    .HasForeignKey(j => j.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Prevent duplicate college targeting
                entity.HasIndex(j => new
                {
                    j.JobId,
                    j.CollegeId
                })
                .IsUnique();
            });


            // ==========================================
            // 8. JOB ELIGIBLE DEPARTMENT CONFIGURATION
            // ==========================================

            builder.Entity<JobEligibleDepartment>(entity =>
            {
                entity.HasKey(e => e.JobEligibleDepartmentId);

                entity.Property(e => e.Department)
                    .IsRequired()
                    .HasMaxLength(100);

                // JobOpening - Eligible Departments
                entity.HasOne(e => e.Job)
                    .WithMany(j => j.EligibleDepartments)
                    .HasForeignKey(e => e.JobId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Prevent duplicate departments for a job
                entity.HasIndex(e => new
                {
                    e.JobId,
                    e.Department
                })
                .IsUnique();
            });


            // ==========================================
            // 9. JOB ELIGIBLE GRADUATION YEAR CONFIGURATION
            // ==========================================

            builder.Entity<JobEligibleGraduationYear>(entity =>
            {
                entity.HasKey(e => e.JobEligibleGraduationYearId);

                // JobOpening - Eligible Graduation Years
                entity.HasOne(e => e.Job)
                    .WithMany(j => j.EligibleGraduationYears)
                    .HasForeignKey(e => e.JobId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Prevent duplicate graduation years for a job
                entity.HasIndex(e => new
                {
                    e.JobId,
                    e.GraduationYear
                })
                .IsUnique();
            });
        }
    }
}
