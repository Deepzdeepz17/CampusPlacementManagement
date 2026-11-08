
using CampusPlacementManagement.Constants;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusPlacementManagement.Controllers
{
    [Route("api/students")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IPlacementOfficerService _placementOfficerService;

        public StudentController(
            IStudentService studentService,
            IPlacementOfficerService placementOfficerService)
        {
            _studentService = studentService;
            _placementOfficerService = placementOfficerService;
        }

        // POST: api/students/enroll
        [Authorize(Roles = Roles.PlacementOfficer)]
        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll(
            [FromBody] EnrollStudentDTO studentDTO)
        {
            try
            {
                var placementOfficerUserId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (placementOfficerUserId == null)
                    return Unauthorized();

                var student = await _studentService.EnrollAsync(
                    studentDTO,
                    placementOfficerUserId);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "Student enrolled successfully. Awaiting approval.",
                    student
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/students
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.PlacementOfficer)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // SuperAdmin can view all students
            if (User.IsInRole(Roles.SuperAdmin))
            {
                var allStudents = await _studentService.GetAllAsync();
                return Ok(allStudents);
            }

            // Placement Officer can view only their college students
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var officer = await _placementOfficerService
                .GetByUserIdAsync(userId);

            if (officer == null ||
                officer.Status != AccountStatus.Approved)
                return Forbid();

            var students = await _studentService
                .GetByCollegeIdAsync(officer.CollegeId);

            return Ok(students);
        }

        // GET: api/students/pending
        [Authorize(Roles = Roles.PlacementOfficer)]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var officer = await _placementOfficerService
                .GetByUserIdAsync(userId);

            if (officer == null ||
                officer.Status != AccountStatus.Approved)
                return Forbid();

            var students = await _studentService
                .GetPendingByCollegeIdAsync(officer.CollegeId);

            return Ok(students);
        }

        // GET: api/students/me
        [Authorize(Roles = Roles.Student)]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var student = await _studentService.GetByUserIdAsync(userId);

            if (student == null)
            {
                return NotFound(new
                {
                    message = "Student profile not found."
                });
            }

            return Ok(student);
        }

        // GET: api/students/{id}
        [Authorize(Roles = Roles.SuperAdmin + "," +
                          Roles.PlacementOfficer + "," +
                          Roles.Student)]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentService.GetByIdAsync(id);

            if (student == null)
            {
                return NotFound(new
                {
                    message = "Student not found."
                });
            }

            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            // SuperAdmin can view any student
            if (User.IsInRole(Roles.SuperAdmin))
                return Ok(student);

            // Student can view only their own profile
            if (User.IsInRole(Roles.Student))
            {
                if (student.StudentId !=
                    (await _studentService.GetByUserIdAsync(userId!))
                    ?.StudentId)
                {
                    return Forbid();
                }

                return Ok(student);
            }

            // Placement Officer can view only students in their college
            var officer = await _placementOfficerService
                .GetByUserIdAsync(userId!);

            if (officer == null ||
                officer.Status != AccountStatus.Approved ||
                officer.CollegeId != student.CollegeId)
            {
                return Forbid();
            }

            return Ok(student);
        }

        // PUT: api/students/{id}/approve
        [Authorize(Roles = Roles.PlacementOfficer)]
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var isApproved = await _studentService.ApproveAsync(id, userId);

            if (!isApproved)
            {
                var student = await _studentService.GetByIdAsync(id);

                if (student == null)
                    return NotFound(new { message = "Student not found." });

                return Conflict(new
                {
                    message = "Student cannot be approved. Check college ownership and pending status."
                });
            }

            return Ok(new
            {
                message = "Student approved successfully."
            });
        }

        // PUT: api/students/{id}/reject
        [Authorize(Roles = Roles.PlacementOfficer)]
        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var isRejected = await _studentService.RejectAsync(id, userId);

            if (!isRejected)
            {
                var student = await _studentService.GetByIdAsync(id);

                if (student == null)
                    return NotFound(new { message = "Student not found." });

                return Conflict(new
                {
                    message = "Student cannot be rejected. Check college ownership and pending status."
                });
            }

            return Ok(new
            {
                message = "Student rejected successfully."
            });
        }

        // PUT: api/students/{id}/suspend
        [Authorize(Roles = Roles.SuperAdmin + "," + Roles.PlacementOfficer)]
        [HttpPut("{id:int}/suspend")]
        public async Task<IActionResult> Suspend(int id)
        {
            string? placementOfficerUserId = null;

            // Placement Officer must pass their own identity ID
            if (User.IsInRole(Roles.PlacementOfficer))
            {
                placementOfficerUserId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (placementOfficerUserId == null)
                    return Unauthorized();
            }

            var isSuspended = await _studentService.SuspendAsync(
                id,
                placementOfficerUserId);

            if (!isSuspended)
            {
                var student = await _studentService.GetByIdAsync(id);

                if (student == null)
                    return NotFound(new { message = "Student not found." });

                return Conflict(new
                {
                    message = "Student cannot be suspended. Check college ownership and approved status."
                });
            }

            return Ok(new
            {
                message = "Student suspended successfully."
            });
        }
    }
}
