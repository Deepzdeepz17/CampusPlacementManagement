
using System.Security.Claims;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobController(IJobService jobService)
    {
        _jobService = jobService;
    }

    // Create a new job
    [HttpPost]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> CreateJob(CreateJobDTO dto)
    {
        try
        {
            var userId = GetUserId();

            var result = await _jobService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetJobById),
                new { id = result.JobId },
                result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // Get all jobs visible to the logged-in role
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,CompanyHR,Student,PlacementOfficer")]
    public async Task<IActionResult> GetAllJobs()
    {
        try
        {
            var userId = GetUserId();
            var role = GetUserRole();

            var jobs = await _jobService.GetAllAsync(userId, role);

            return Ok(jobs);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // Get job by ID
    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin,CompanyHR,Student,PlacementOfficer")]
    public async Task<IActionResult> GetJobById(int id)
    {
        try
        {
            var userId = GetUserId();
            var role = GetUserRole();

            var job = await _jobService.GetByIdAsync(
                id, userId, role);

            return Ok(job);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // Update an existing draft job
    [HttpPut("{id:int}")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> UpdateJob(
        int id,
        UpdateJobDTO dto)
    {
        try
        {
            var userId = GetUserId();

            var result = await _jobService.UpdateAsync(
                id, dto, userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // Publish a job
    [HttpPut("{id:int}/publish")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> PublishJob(int id)
    {
        try
        {
            var userId = GetUserId();

            var result = await _jobService.PublishAsync(id, userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // Close a published job
    [HttpPut("{id:int}/close")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> CloseJob(int id)
    {
        try
        {
            var userId = GetUserId();

            var result = await _jobService.CloseAsync(id, userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // Delete a draft job
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        try
        {
            var userId = GetUserId();

            await _jobService.DeleteAsync(id, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // Get eligible jobs for the logged-in student
    [HttpGet("eligible")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetEligibleJobs()
    {
        try
        {
            var userId = GetUserId();

            var jobs = await _jobService.GetEligibleJobsAsync(userId);

            return Ok(jobs);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // Get the authenticated user's ID from JWT
    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(
                "User ID claim not found.");
    }

    // Get the authenticated user's role from JWT
    private string GetUserRole()
    {
        return User.FindFirstValue(ClaimTypes.Role)
            ?? throw new UnauthorizedAccessException(
                "User role claim not found.");
    }

    // Convert service exceptions into HTTP responses
    private IActionResult HandleException(Exception ex)
    {
        return ex switch
        {
            KeyNotFoundException =>
                NotFound(new { message = ex.Message }),

            UnauthorizedAccessException =>
                StatusCode(403, new { message = ex.Message }),

            InvalidOperationException =>
                Conflict(new { message = ex.Message }),

            _ =>
                StatusCode(500, new
                {
                    message = "An unexpected error occurred."
                })
        };
    }
}
