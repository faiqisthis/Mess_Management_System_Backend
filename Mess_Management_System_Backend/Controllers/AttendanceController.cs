using Mess_Management_System_Backend.Models;
using Mess_Management_System_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mess_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IUserService _userService;

        public AttendanceController(IAttendanceService attendanceService, IUserService userService)
        {
            _attendanceService = attendanceService;
            _userService = userService;
        }

        /// <summary>
        /// Get all students for marking attendance (Admin/Teacher only)
        /// Teachers can see all students to mark their attendance
        /// </summary>
        [HttpGet("students")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAllStudents()
        {
            var allUsers = await _userService.GetAllUsersAsync();
            
            // Filter to get only students
            var students = allUsers
                .Where(u => u.Role == UserRole.Student && u.IsActive == true)
                .Select(u => new
                {
                    id = u.Id,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    fullName = $"{u.FirstName} {u.LastName}",
                    email = u.Email,
                    rollNumber = u.RollNumber,
                    roomNumber = u.RoomNumber
                })
                .OrderBy(u => u.rollNumber)
                .ToList();

            return Ok(students);
        }

        /// <summary>
        /// Mark attendance for a single user (Admin/Teacher only)
        /// Teachers cannot mark attendance for themselves, only for students
        /// Admin can mark attendance for everyone
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> MarkAttendance([FromBody] MarkAttendanceRequest request)
        {
            try
            {
                // Get current user's ID and role from JWT token
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRoleClaim = User.FindFirst(ClaimTypes.Role);
                
                if (currentUserIdClaim == null || currentUserRoleClaim == null)
                    return Unauthorized(new { message = "Invalid token claims" });

                var currentUserId = int.Parse(currentUserIdClaim.Value);
                var currentUserRole = currentUserRoleClaim.Value;

                // Teachers cannot mark attendance for themselves
                if (currentUserRole == "Teacher" && request.UserId == currentUserId)
                    return Forbid();

                // Teachers can only mark attendance for students
                if (currentUserRole == "Teacher")
                {
                    var targetUser = await _userService.GetUserByIdAsync(request.UserId);
                    if (targetUser == null)
                        return NotFound(new { message = $"User with ID {request.UserId} not found" });
                    
                    if (targetUser.Role != UserRole.Student)
                        return Forbid();
                }

                var attendance = await _attendanceService.MarkAttendanceAsync(
                    request.UserId,
                    request.Date,
                    request.Status
                );
                return CreatedAtAction(nameof(GetAttendanceById), new { id = attendance.Id }, attendance);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mark attendance for multiple users at once (Admin/Teacher only)
        /// Teachers can only mark attendance for students, not for themselves
        /// Admin can mark attendance for everyone
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> MarkBulkAttendance([FromBody] MarkBulkAttendanceRequest request)
        {
            try
            {
                // Get current user's ID and role from JWT token
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRoleClaim = User.FindFirst(ClaimTypes.Role);
                
                if (currentUserIdClaim == null || currentUserRoleClaim == null)
                    return Unauthorized(new { message = "Invalid token claims" });

                var currentUserId = int.Parse(currentUserIdClaim.Value);
                var currentUserRole = currentUserRoleClaim.Value;

                // Teachers: validate they're only marking students (not themselves or other teachers)
                if (currentUserRole == "Teacher")
                {
                    var userIds = request.Attendances.Select(a => a.UserId).Distinct().ToList();
                    
                    // Check if teacher is trying to mark their own attendance
                    if (userIds.Contains(currentUserId))
                        return Forbid();

                    // Validate all users are students
                    var allUsers = await _userService.GetAllUsersAsync();
                    var targetUsers = allUsers.Where(u => userIds.Contains(u.Id)).ToList();
                    
                    if (targetUsers.Any(u => u.Role != UserRole.Student))
                        return Forbid();
                }

                var attendances = await _attendanceService.MarkBulkAttendanceAsync(
                    request.Date,
                    request.Attendances
                );
                return Ok(new
                {
                    message = $"Successfully marked attendance for {attendances.Count()} users",
                    attendances = attendances
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update existing attendance (Admin/Teacher only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] UpdateAttendanceRequest request)
        {
            var attendance = await _attendanceService.UpdateAttendanceAsync(id, request.Status);
            if (attendance == null)
                return NotFound(new { message = $"Attendance with ID {id} not found" });

            return Ok(attendance);
        }

        /// <summary>
        /// Get attendance by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAttendanceById(int id)
        {
            var attendance = await _attendanceService.GetAttendanceByUserAndDateAsync(id, DateTime.UtcNow);
            if (attendance == null)
                return NotFound(new { message = $"Attendance with ID {id} not found" });

            return Ok(attendance);
        }

        /// <summary>
        /// Get all attendance for a specific date (Admin/Teacher only)
        /// </summary>
        [HttpGet("date/{date}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetAttendanceByDate(DateTime date)
        {
            var attendances = await _attendanceService.GetAttendanceByDateAsync(date);
            return Ok(attendances);
        }

        /// <summary>
        /// Get attendance for a specific user with flexible filtering options
        /// 
        /// Query Parameters:
        /// - startDate & endDate: Filter by date range
        /// - year & month: Filter by specific month (alternative to date range)
        /// - includeSummary: Include summary statistics (total days, present/absent counts, estimated costs)
        /// - includeMenuDetails: Include daily menu details and cost breakdown
        /// 
        /// Examples:
        /// - Simple list: /api/attendance/user/5?startDate=2024-01-01&endDate=2024-01-31
        /// - With summary: /api/attendance/user/5?year=2024&month=1&includeSummary=true
        /// - Full details: /api/attendance/user/5?year=2024&month=1&includeSummary=true&includeMenuDetails=true
        /// 
        /// Users can view their own, Admin/Teacher can view anyone's
        /// </summary>
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserAttendance(
            int userId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? year = null,
            [FromQuery] int? month = null,
            [FromQuery] bool includeSummary = false,
            [FromQuery] bool includeMenuDetails = false)
        {
            try
            {
                var result = await _attendanceService.GetUserAttendanceAsync(
                    userId, 
                    startDate, 
                    endDate, 
                    year, 
                    month, 
                    includeSummary, 
                    includeMenuDetails
                );
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete attendance (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var result = await _attendanceService.DeleteAttendanceAsync(id);
            if (!result)
                return NotFound(new { message = $"Attendance with ID {id} not found" });

            return NoContent();
        }
    }

    // Request models
    public class MarkAttendanceRequest
    {
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
    }

    public class MarkBulkAttendanceRequest
    {
        public DateTime Date { get; set; }
        public List<UserAttendanceRecord> Attendances { get; set; } = new List<UserAttendanceRecord>();
    }

    public class UpdateAttendanceRequest
    {
        public AttendanceStatus Status { get; set; }
    }
}
