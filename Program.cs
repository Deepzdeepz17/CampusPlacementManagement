
using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services;
using CampusPlacementManagement.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

// 2. Add OpenAPI
builder.Services.AddOpenApi();

// 3. Configure SQL Server and DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 5. Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

// Convert Base64 JWT key into bytes
var jwtKeyBytes = Convert.FromBase64String(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        // Same key conversion used by JwtService
        IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes)
    };
});

// 6. Add Authorization
builder.Services.AddAuthorization();

// 7. Authentication and Refresh Token Services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<RefreshTokenService>();

// 8. College Management - Dependency Injection
builder.Services.AddScoped<ICollegeRepository, CollegeRepository>();
builder.Services.AddScoped<ICollegeService, CollegeService>();

// 9. Placement Officer Management - Dependency Injection
builder.Services.AddScoped<
    IPlacementOfficerRepository,
    PlacementOfficerRepository>();

builder.Services.AddScoped<
    IPlacementOfficerService,
    PlacementOfficerService>();

// 10. Student Management - Dependency Injection
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

// 11. Company Management - Dependency Injection
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

// 12. CompanyHR Management - Dependency Injection
builder.Services.AddScoped<ICompanyHRRepository, CompanyHRRepository>();
builder.Services.AddScoped<ICompanyHRService, CompanyHRService>();
// Job Management
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();

var app = builder.Build();

// 13. Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Authentication must come before Authorization
app.UseAuthentication();

app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

app.Run();
