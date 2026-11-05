using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
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

            // 2. Check whether account is approved
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

            // 4. Generate JWT token
            var token = await _jwtService.GenerateTokenAsync(user);

            // 5. Return token and user details
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                message = "Login successful.",
                token = token,
                expirationMinutes = 120,
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