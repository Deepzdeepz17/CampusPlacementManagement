
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/placement-officers")]
    [ApiController]
    public class PlacementOfficerController : ControllerBase
    {
        private readonly IPlacementOfficerService _placementOfficerService;

        public PlacementOfficerController(
            IPlacementOfficerService placementOfficerService)
        {
            _placementOfficerService = placementOfficerService;
        }

        // POST: api/placement-officers/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterPlacementOfficerDTO placementOfficerDTO)
        {
            try
            {
                var placementOfficer =
                    await _placementOfficerService.RegisterAsync(
                        placementOfficerDTO);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Placement officer registration submitted successfully. Awaiting approval.",
                    placementOfficer
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

        // GET: api/placement-officers
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var placementOfficers =
                await _placementOfficerService.GetAllAsync();

            return Ok(placementOfficers);
        }

        // GET: api/placement-officers/pending
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var placementOfficers =
                await _placementOfficerService.GetPendingAsync();

            return Ok(placementOfficers);
        }

        // GET: api/placement-officers/me
        [Authorize(Roles = Roles.PlacementOfficer)]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var officer =
                await _placementOfficerService.GetByUserIdAsync(userId);

            if (officer == null)
            {
                return NotFound(new
                {
                    message = "Placement officer profile not found."
                });
            }

            return Ok(officer);
        }

        // GET: api/placement-officers/{id}
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.PlacementOfficer)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var officer =
                await _placementOfficerService.GetByIdAsync(id);

            if (officer == null)
            {
                return NotFound(new
                {
                    message = "Placement officer not found."
                });
            }

            // Placement officers can view only their own profile
            if (!User.IsInRole(Roles.SuperAdmin))
            {
                var userId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                var currentOfficer = userId == null
                    ? null
                    : await _placementOfficerService.GetByUserIdAsync(userId);

                if (currentOfficer == null ||
                    currentOfficer.PlacementOfficerId != id)
                {
                    return Forbid();
                }
            }

            return Ok(officer);
        }

        // PUT: api/placement-officers/{id}/approve
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var approvedByUserId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (approvedByUserId == null)
                return Unauthorized();

            var isApproved =
                await _placementOfficerService.ApproveAsync(
                    id, approvedByUserId);

            if (!isApproved)
            {
                var officer =
                    await _placementOfficerService.GetByIdAsync(id);

                if (officer == null)
                {
                    return NotFound(new
                    {
                        message = "Placement officer not found."
                    });
                }

                return Conflict(new
                {
                    message = "Only pending placement officers can be approved."
                });
            }

            return Ok(new
            {
                message = "Placement officer approved successfully."
            });
        }

        // PUT: api/placement-officers/{id}/reject
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var isRejected =
                await _placementOfficerService.RejectAsync(id);

            if (!isRejected)
            {
                var officer =
                    await _placementOfficerService.GetByIdAsync(id);

                if (officer == null)
                {
                    return NotFound(new
                    {
                        message = "Placement officer not found."
                    });
                }

                return Conflict(new
                {
                    message = "Only pending placement officers can be rejected."
                });
            }

            return Ok(new
            {
                message = "Placement officer rejected successfully."
            });
        }

        // PUT: api/placement-officers/{id}/suspend
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/suspend")]
        public async Task<IActionResult> Suspend(int id)
        {
            var isSuspended =
                await _placementOfficerService.SuspendAsync(id);

            if (!isSuspended)
            {
                var officer =
                    await _placementOfficerService.GetByIdAsync(id);

                if (officer == null)
                {
                    return NotFound(new
                    {
                        message = "Placement officer not found."
                    });
                }

                return Conflict(new
                {
                    message = "Only approved placement officers can be suspended."
                });
            }

            return Ok(new
            {
                message = "Placement officer suspended successfully."
            });
        }
    }
}
