using CampusPlacementManagement.Constants;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Data.Seeders
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            // Create a scope to access scoped Identity services
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Create roles if they do not already exist
            string[] roles =
            {
                Roles.SuperAdmin,
                Roles.PlacementOfficer,
                Roles.CompanyHR,
                Roles.Student
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to create role {role}: " +
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            // 2. Read SuperAdmin credentials from configuration
            var email = configuration["AdminSeed:Email"];
            var password = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "SuperAdmin email and password are missing from configuration.");
            }

            // 3. Check whether the SuperAdmin account already exists
            var admin = await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                // 4. Create the initial SuperAdmin account
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    AccountStatus = AccountStatus.Approved
                };

                var createResult = await userManager.CreateAsync(
                    admin, password);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Failed to create SuperAdmin: " +
                        string.Join(", ",
                            createResult.Errors.Select(e => e.Description)));
                }
            }

            // 5. Ensure the account is approved
            if (admin.AccountStatus != AccountStatus.Approved ||
                !admin.EmailConfirmed)
            {
                admin.AccountStatus = AccountStatus.Approved;
                admin.EmailConfirmed = true;

                var updateResult = await userManager.UpdateAsync(admin);

                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Failed to update SuperAdmin: " +
                        string.Join(", ",
                            updateResult.Errors.Select(e => e.Description)));
                }
            }

            // 6. Assign the SuperAdmin role if it is not already assigned
            if (!await userManager.IsInRoleAsync(admin, Roles.SuperAdmin))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    admin, Roles.SuperAdmin);

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Failed to assign SuperAdmin role: " +
                        string.Join(", ",
                            roleResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}