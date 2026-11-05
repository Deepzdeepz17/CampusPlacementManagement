using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService,
            RefreshTokenService refreshTokenService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _configuration = configuration;
        }

        // GET: api/Auth/me
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var fullName = User.FindFirstValue(ClaimTypes.Name);

            var roles = User.FindAll(ClaimTypes.Role)
                            .Select(role => role.Value)
                            .ToList();

            return Ok(new
            {
                message = "Authenticated successfully.",
                userId,
                fullName,
                email,
                roles
            });
        }

        // GET: api/Auth/superadmin-check
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("superadmin-check")]
        public IActionResult SuperAdminCheck()
        {
            return Ok(new
            {
                message = "Welcome SuperAdmin! You have access to this endpoint."
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            // 1. Find user using email
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // 2. Check account status
            if (user.AccountStatus != AccountStatus.Approved)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "Your account is not approved or is inactive."
                });
            }

            // 3. Verify password
            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                loginDTO.Password,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // 4. Generate access token
            var accessToken = await _jwtService.GenerateTokenAsync(user);

            // 5. Generate and store refresh token
            var refreshToken = await _refreshTokenService
                .CreateRefreshTokenAsync(user);

            // 6. Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // 7. Read access token expiry
            var expiryMinutes = _configuration
                .GetValue<int>("Jwt:AccessTokenMinutes");

            // 8. Return both tokens
            return Ok(new
            {
                message = "Login successful.",

                accessToken = accessToken,
                refreshToken = refreshToken,

                accessTokenExpiresInMinutes = expiryMinutes,

                user = new
                {
                    id = user.Id,
                    fullName = user.FullName,
                    email = user.Email,
                    roles = roles
                }
            });
        }
    }
}