
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly ICompanyHRService _companyHRService;

        public CompanyController(
            ICompanyService companyService,
            ICompanyHRService companyHRService)
        {
            _companyService = companyService;
            _companyHRService = companyHRService;
        }

        // POST: api/companies/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCompanyDTO companyDTO)
        {
            var company = await _companyService.RegisterAsync(companyDTO);

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Company registration submitted successfully. Awaiting approval.",
                company
            });
        }

        // GET: api/companies
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetAllAsync();

            return Ok(companies);
        }

        // GET: api/companies/pending
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var companies = await _companyService.GetPendingAsync();

            return Ok(companies);
        }

        // GET: api/companies/{id}
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.CompanyHR)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companyService.GetByIdAsync(id);

            if (company == null)
            {
                return NotFound(new
                {
                    message = "Company not found."
                });
            }

            // SuperAdmin can view any company
            if (User.IsInRole(Roles.SuperAdmin))
                return Ok(company);

            // CompanyHR can view only their associated company
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var companyHR = await _companyHRService
                .GetByUserIdAsync(userId);

            if (companyHR == null || companyHR.CompanyId != id)
                return Forbid();

            return Ok(company);
        }

        // PUT: api/companies/{id}/approve
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var isApproved = await _companyService.ApproveAsync(id);

            if (!isApproved)
            {
                var company = await _companyService.GetByIdAsync(id);

                if (company == null)
                {
                    return NotFound(new
                    {
                        message = "Company not found."
                    });
                }

                return Conflict(new
                {
                    message = "Only pending companies can be approved."
                });
            }

            return Ok(new
            {
                message = "Company approved successfully."
            });
        }

        // PUT: api/companies/{id}/reject
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var isRejected = await _companyService.RejectAsync(id);

            if (!isRejected)
            {
                var company = await _companyService.GetByIdAsync(id);

                if (company == null)
                {
                    return NotFound(new
                    {
                        message = "Company not found."
                    });
                }

                return Conflict(new
                {
                    message = "Only pending companies can be rejected."
                });
            }

            return Ok(new
            {
                message = "Company rejected successfully."
            });
        }

        // PUT: api/companies/{id}/suspend
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/suspend")]
        public async Task<IActionResult> Suspend(int id)
        {
            var isSuspended = await _companyService.SuspendAsync(id);

            if (!isSuspended)
            {
                var company = await _companyService.GetByIdAsync(id);

                if (company == null)
                {
                    return NotFound(new
                    {
                        message = "Company not found."
                    });
                }

                return Conflict(new
                {
                    message = "Only approved companies can be suspended."
                });
            }

            return Ok(new
            {
                message = "Company suspended successfully."
            });
        }

        // PUT: api/companies/{id}
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.CompanyHR)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] RegisterCompanyDTO companyDTO)
        {
            var existingCompany = await _companyService.GetByIdAsync(id);

            if (existingCompany == null)
            {
                return NotFound(new
                {
                    message = "Company not found."
                });
            }

            // CompanyHR can update only its own company
            if (!User.IsInRole(Roles.SuperAdmin))
            {
                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (userId == null)
                    return Unauthorized();

                var companyHR = await _companyHRService
                    .GetByUserIdAsync(userId);

                if (companyHR == null || companyHR.CompanyId != id)
                    return Forbid();
            }

            var isUpdated = await _companyService
                .UpdateAsync(id, companyDTO);

            if (!isUpdated)
            {
                return NotFound(new
                {
                    message = "Company could not be updated."
                });
            }

            return Ok(new
            {
                message = "Company details updated successfully."
            });
        }
    }
}
