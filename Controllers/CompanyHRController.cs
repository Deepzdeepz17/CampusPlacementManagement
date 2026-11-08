
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/company-hrs")]
    [ApiController]
    public class CompanyHRController : ControllerBase
    {
        private readonly ICompanyHRService _companyHRService;

        public CompanyHRController(
            ICompanyHRService companyHRService)
        {
            _companyHRService = companyHRService;
        }

        // POST: api/company-hrs/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCompanyHRDTO companyHRDTO)
        {
            try
            {
                var companyHR = await _companyHRService
                    .RegisterAsync(companyHRDTO);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Company HR registration submitted successfully. Awaiting approval.",
                    companyHR
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/company-hrs
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companyHRs = await _companyHRService.GetAllAsync();

            return Ok(companyHRs);
        }

        // GET: api/company-hrs/pending
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var companyHRs = await _companyHRService.GetPendingAsync();

            return Ok(companyHRs);
        }

        // GET: api/company-hrs/{id}
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var companyHR = await _companyHRService.GetByIdAsync(id);

            if (companyHR == null)
            {
                return NotFound(new
                {
                    message = "Company HR not found."
                });
            }

            return Ok(companyHR);
        }

        // GET: api/company-hrs/me
        [Authorize(Roles = Roles.CompanyHR)]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var companyHR = await _companyHRService
                .GetByUserIdAsync(userId);

            if (companyHR == null)
            {
                return NotFound(new
                {
                    message = "Company HR profile not found."
                });
            }

            return Ok(companyHR);
        }

        // PUT: api/company-hrs/{id}/approve
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var approvedByUserId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (approvedByUserId == null)
                return Unauthorized();

            var isApproved = await _companyHRService.ApproveAsync(
                id, approvedByUserId);

            if (!isApproved)
            {
                var companyHR = await _companyHRService.GetByIdAsync(id);

                if (companyHR == null)
                {
                    return NotFound(new
                    {
                        message = "Company HR not found."
                    });
                }

                return Conflict(new
                {
                    message = "Company HR cannot be approved. Check its account and company status."
                });
            }

            return Ok(new
            {
                message = "Company HR approved successfully."
            });
        }

        // PUT: api/company-hrs/{id}/reject
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var isRejected = await _companyHRService.RejectAsync(id);

            if (!isRejected)
            {
                var companyHR = await _companyHRService.GetByIdAsync(id);

                if (companyHR == null)
                {
                    return NotFound(new
                    {
                        message = "Company HR not found."
                    });
                }

                return Conflict(new
                {
                    message = "Only pending Company HR accounts can be rejected."
                });
            }

            return Ok(new
            {
                message = "Company HR rejected successfully."
            });
        }
    }
}
