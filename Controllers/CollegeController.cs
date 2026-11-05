
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/colleges")]
    [ApiController]
    public class CollegeController : ControllerBase
    {
        private readonly ICollegeService _collegeService;

        public CollegeController(ICollegeService collegeService)
        {
            _collegeService = collegeService;
        }

        // POST: api/colleges/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] College college)
        {
            try
            {
                var registeredCollege =
                    await _collegeService.RegisterAsync(college);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "College registration submitted successfully. Awaiting approval.",
                    college = registeredCollege
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        // GET: api/colleges
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var colleges = await _collegeService.GetAllAsync();

            return Ok(colleges);
        }

        // GET: api/colleges/{id}
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var college = await _collegeService.GetByIdAsync(id);

            if (college == null)
            {
                return NotFound(new
                {
                    message = "College not found."
                });
            }

            return Ok(college);
        }

        // GET: api/colleges/pending
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var colleges = await _collegeService.GetPendingAsync();

            return Ok(colleges);
        }

        // PUT: api/colleges/{id}/approve
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var isApproved =
                await _collegeService.ApproveAsync(id);

            if (!isApproved)
            {
                var college = await _collegeService.GetByIdAsync(id);

                if (college == null)
                {
                    return NotFound(new
                    {
                        message = "College not found."
                    });
                }

                return Conflict(new
                {
                    message = "Only pending colleges can be approved."
                });
            }

            return Ok(new
            {
                message = "College approved successfully."
            });
        }

        // PUT: api/colleges/{id}/reject
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var isRejected =
                await _collegeService.RejectAsync(id);

            if (!isRejected)
            {
                var college = await _collegeService.GetByIdAsync(id);

                if (college == null)
                {
                    return NotFound(new
                    {
                        message = "College not found."
                    });
                }

                return Conflict(new
                {
                    message = "Only pending colleges can be rejected."
                });
            }

            return Ok(new
            {
                message = "College rejected successfully."
            });
        }

        // PUT: api/colleges/{id}/suspend
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}/suspend")]
        public async Task<IActionResult> Suspend(int id)
        {
            var isSuspended =
                await _collegeService.SuspendAsync(id);

            if (!isSuspended)
            {
                var college = await _collegeService.GetByIdAsync(id);

                if (college == null)
                {
                    return NotFound(new
                    {
                        message = "College not found."
                    });
                }

                return Conflict(new
                {
                    message = "Only approved colleges can be suspended."
                });
            }

            return Ok(new
            {
                message = "College suspended successfully."
            });
        }

        // PUT: api/colleges/{id}
        [Authorize(Roles = Roles.SuperAdmin)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] College college)
        {
            var existingCollege =
                await _collegeService.GetByIdAsync(id);

            if (existingCollege == null)
            {
                return NotFound(new
                {
                    message = "College not found."
                });
            }

            var isUpdated =
                await _collegeService.UpdateAsync(id, college);

            if (!isUpdated)
            {
                return Conflict(new
                {
                    message = "College code already exists."
                });
            }

            return Ok(new
            {
                message = "College details updated successfully."
            });
        }
    }
}
