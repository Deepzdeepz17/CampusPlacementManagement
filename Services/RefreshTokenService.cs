using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CampusPlacementManagement.Services
{
    public class RefreshTokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public RefreshTokenService(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. Generate and store a new refresh token
        public async Task<string> CreateRefreshTokenAsync(
            ApplicationUser user)
        {
            var refreshTokenDays = _configuration
                .GetValue<int>("Jwt:RefreshTokenDays");

            if (refreshTokenDays <= 0)
            {
                throw new InvalidOperationException(
                    "Refresh token expiry configuration is invalid.");
            }

            // Generate a secure random token
            var token = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));

            // Hash the token before storing it
            var tokenHash = HashToken(token);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(refreshTokenDays)
            };

            // Store the hashed token in database
            _context.RefreshTokens.Add(refreshToken);

            await _context.SaveChangesAsync();

            // Return original token to client
            return token;
        }

        // 2. Validate and rotate refresh token
        public async Task<(ApplicationUser User, string NewRefreshToken)?>
            RotateRefreshTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var tokenHash = HashToken(token);

            // Find the refresh token and its associated user
            var existingToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            // Check whether token exists
            if (existingToken == null)
            {
                return null;
            }

            // Check whether token has been revoked
            if (existingToken.RevokedAtUtc.HasValue)
            {
                return null;
            }

            // Check whether token has expired
            if (existingToken.ExpiresAtUtc <= DateTime.UtcNow)
            {
                return null;
            }

            // Generate replacement token
            var newToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));

            var newTokenHash = HashToken(newToken);

            var refreshTokenDays = _configuration
                .GetValue<int>("Jwt:RefreshTokenDays");

            if (refreshTokenDays <= 0)
            {
                throw new InvalidOperationException(
                    "Refresh token expiry configuration is invalid.");
            }

            // Revoke the old token
            existingToken.RevokedAtUtc = DateTime.UtcNow;

            // Record its replacement
            existingToken.ReplacedByTokenHash = newTokenHash;

            // Create the new refresh token record
            var replacementToken = new RefreshToken
            {
                UserId = existingToken.UserId,
                TokenHash = newTokenHash,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(refreshTokenDays)
            };

            _context.RefreshTokens.Add(replacementToken);

            await _context.SaveChangesAsync();

            // Return user and new original token
            return (existingToken.User, newToken);
        }

        // 3. Revoke refresh token during logout
        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var tokenHash = HashToken(token);

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (refreshToken == null ||
                refreshToken.RevokedAtUtc.HasValue)
            {
                return false;
            }

            refreshToken.RevokedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // 4. Hash refresh token using SHA-256
        private static string HashToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            var hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToBase64String(hashBytes);
        }
    }
}