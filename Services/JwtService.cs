using CampusPlacementManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CampusPlacementManagement.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            // 1. Read JWT settings
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var secretKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience) ||
                string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException(
                    "JWT configuration is missing.");
            }

            // 2. Read token expiration time
            var expiryMinutes = _configuration
                .GetValue<int>("Jwt:AccessTokenMinutes");

            if (expiryMinutes <= 0)
            {
                throw new InvalidOperationException(
                    "JWT expiration time must be greater than zero.");
            }

            // 3. Get user's assigned roles
            var roles = await _userManager.GetRolesAsync(user);

            // 4. Create user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 5. Create signing key
            var keyBytes = Convert.FromBase64String(secretKey);
            var key = new SymmetricSecurityKey(keyBytes);

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            // 6. Generate JWT token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            // 7. Return token as a string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}