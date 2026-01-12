using Mess_Management_System_Backend.Models;
using Mess_Management_System_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mess_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        /// <summary>
        /// Generate a monthly bill for a user (Admin only)
        /// Bills can only be generated for previous months, not the current month
        /// </summary>
        [HttpPost("generate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateBill([FromBody] GenerateBillRequest request)
        {
            try
            {
                var bill = await _billingService.GenerateMonthlyBillAsync(
                    request.UserId,
                    request.Year,
                    request.Month
                );
                return CreatedAtAction(nameof(GetBillById), new { id = bill.Id }, bill);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get bill by ID
        /// Users can view their own bills, Admin can view any
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBillById(int id)
        {
            var bill = await _billingService.GetUserBillByIdAsync(id);
            if (bill == null)
                return NotFound(new { message = $"Bill with ID {id} not found" });

            return Ok(bill);
        }

        /// <summary>
        /// Get all bills for a specific user
        /// Users can view their own bills, Admin can view anyone's
        /// </summary>
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserBills(int userId)
        {
            var bills = await _billingService.GetUserBillsAsync(userId);
            return Ok(bills);
        }

        /// <summary>
        /// Get all bills (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBills(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var bills = await _billingService.GetAllBillsAsync(startDate, endDate);
            return Ok(bills);
        }

        /// <summary>
        /// Mark a bill as paid (Admin only)
        /// </summary>
        [HttpPut("{id:int}/pay")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkBillAsPaid(int id)
        {
            var bill = await _billingService.MarkBillAsPaidAsync(id);
            if (bill == null)
                return NotFound(new { message = $"Bill with ID {id} not found" });

            return Ok(bill);
        }

        /// <summary>
        /// Delete a bill (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBill(int id)
        {
            var result = await _billingService.DeleteBillAsync(id);
            if (!result)
                return NotFound(new { message = $"Bill with ID {id} not found" });

            return NoContent();
        }

        /// <summary>
        /// Export bills history report as CSV
        /// Admin: Can export all bills or filter by user
        /// User: Can only export their own bills
        /// Query Parameters:
        /// - userId: Filter by specific user (optional for admin)
        /// - year: Filter by year (optional)
        /// - startDate & endDate: Filter by date range (optional)
        /// </summary>
        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportBillsHistoryCSV(
            [FromQuery] int? userId = null,
            [FromQuery] int? year = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Get current user info
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var currentUserRoleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);

                if (currentUserIdClaim == null || currentUserRoleClaim == null)
                    return Unauthorized(new { message = "Invalid token claims" });

                var currentUserId = int.Parse(currentUserIdClaim.Value);
                var currentUserRole = currentUserRoleClaim.Value;

                // Non-admin users can only export their own data
                if (currentUserRole != "Admin" && userId.HasValue && userId.Value != currentUserId)
                    return Forbid();

                // If non-admin user doesn't specify userId, default to their own
                if (currentUserRole != "Admin")
                    userId = currentUserId;

                // Build query
                var query = _billingService.GetAllBillsAsync(startDate, endDate);
                var bills = await query;

                // Filter by userId if specified
                if (userId.HasValue)
                    bills = bills.Where(b => b.UserId == userId.Value);

                // Filter by year if specified
                if (year.HasValue)
                    bills = bills.Where(b => b.StartDate.Year == year.Value);

                var billsList = bills.OrderByDescending(b => b.StartDate).ToList();

                if (!billsList.Any())
                    return NotFound(new { message = "No bills found for the specified criteria" });

                // Generate CSV
                var csv = new System.Text.StringBuilder();
                
                // CSV Header
                csv.AppendLine("Bill ID,User ID,User Name,Email,Roll Number,Period,Start Date,End Date,Total Days,Present Days,Absent Days,Fixed Charges,Food Charges,Total Amount,Is Paid,Paid Date,Generated Date");

                // CSV Data
                foreach (var bill in billsList)
                {
                    csv.AppendLine($"{bill.Id}," +
                        $"{bill.UserId}," +
                        $"\"{bill.User?.FirstName} {bill.User?.LastName}\"," +
                        $"\"{bill.User?.Email}\"," +
                        $"\"{bill.User?.RollNumber ?? "N/A"}\"," +
                        $"\"{bill.StartDate:MMMM yyyy}\"," +
                        $"{bill.StartDate:yyyy-MM-dd}," +
                        $"{bill.EndDate:yyyy-MM-dd}," +
                        $"{bill.TotalDays}," +
                        $"{bill.PresentDays}," +
                        $"{bill.AbsentDays}," +
                        $"{bill.TotalFixedCharges:F2}," +
                        $"{bill.TotalFoodCharges:F2}," +
                        $"{bill.TotalAmount:F2}," +
                        $"{(bill.IsPaid ? "Yes" : "No")}," +
                        $"{(bill.PaidDate.HasValue ? bill.PaidDate.Value.ToString("yyyy-MM-dd") : "N/A")}," +
                        $"{bill.GeneratedDate:yyyy-MM-dd}");
                }

                // Generate filename
                var filename = userId.HasValue
                    ? $"Bills_History_User_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv"
                    : $"Bills_History_All_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

                // Return CSV file
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", filename);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error generating report: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get bills history report with statistics
        /// Admin: Can view all bills or filter by user
        /// User: Can only view their own bills
        /// Query Parameters:
        /// - userId: Filter by specific user (optional for admin)
        /// - year: Filter by year (optional)
        /// - startDate & endDate: Filter by date range (optional)
        /// </summary>
        [HttpGet("history/report")]
        public async Task<IActionResult> GetBillsHistoryReport(
            [FromQuery] int? userId = null,
            [FromQuery] int? year = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Get current user info
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var currentUserRoleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);

                if (currentUserIdClaim == null || currentUserRoleClaim == null)
                    return Unauthorized(new { message = "Invalid token claims" });

                var currentUserId = int.Parse(currentUserIdClaim.Value);
                var currentUserRole = currentUserRoleClaim.Value;

                // Non-admin users can only view their own data
                if (currentUserRole != "Admin" && userId.HasValue && userId.Value != currentUserId)
                    return Forbid();

                // If non-admin user doesn't specify userId, default to their own
                if (currentUserRole != "Admin")
                    userId = currentUserId;

                // Build query
                var query = _billingService.GetAllBillsAsync(startDate, endDate);
                var bills = await query;

                // Filter by userId if specified
                if (userId.HasValue)
                    bills = bills.Where(b => b.UserId == userId.Value);

                // Filter by year if specified
                if (year.HasValue)
                    bills = bills.Where(b => b.StartDate.Year == year.Value);

                var billsList = bills.OrderByDescending(b => b.StartDate).ToList();

                if (!billsList.Any())
                    return Ok(new
                    {
                        bills = new List<object>(),
                        summary = new
                        {
                            totalBills = 0,
                            totalAmount = 0m,
                            totalPaid = 0m,
                            totalDue = 0m,
                            paidCount = 0,
                            dueCount = 0
                        }
                    });

                // Calculate statistics
                var totalAmount = billsList.Sum(b => b.TotalAmount);
                var totalPaid = billsList.Where(b => b.IsPaid).Sum(b => b.TotalAmount);
                var totalDue = billsList.Where(b => !b.IsPaid).Sum(b => b.TotalAmount);
                var paidCount = billsList.Count(b => b.IsPaid);
                var dueCount = billsList.Count(b => !b.IsPaid);
                var totalFixedCharges = billsList.Sum(b => b.TotalFixedCharges);
                var totalFoodCharges = billsList.Sum(b => b.TotalFoodCharges);
                var totalPresentDays = billsList.Sum(b => b.PresentDays);
                var totalAbsentDays = billsList.Sum(b => b.AbsentDays);

                return Ok(new
                {
                    filters = new
                    {
                        userId = userId,
                        year = year,
                        startDate = startDate?.ToString("yyyy-MM-dd"),
                        endDate = endDate?.ToString("yyyy-MM-dd")
                    },
                    bills = billsList.Select(b => new
                    {
                        id = b.Id,
                        userId = b.UserId,
                        userName = b.User != null ? $"{b.User.FirstName} {b.User.LastName}" : "Unknown",
                        email = b.User?.Email,
                        rollNumber = b.User?.RollNumber,
                        period = b.StartDate.ToString("MMMM yyyy"),
                        startDate = b.StartDate.ToString("yyyy-MM-dd"),
                        endDate = b.EndDate.ToString("yyyy-MM-dd"),
                        totalDays = b.TotalDays,
                        presentDays = b.PresentDays,
                        absentDays = b.AbsentDays,
                        totalFixedCharges = b.TotalFixedCharges,
                        totalFoodCharges = b.TotalFoodCharges,
                        totalAmount = b.TotalAmount,
                        isPaid = b.IsPaid,
                        paidDate = b.PaidDate?.ToString("yyyy-MM-dd"),
                        generatedDate = b.GeneratedDate.ToString("yyyy-MM-dd")
                    }),
                    summary = new
                    {
                        totalBills = billsList.Count,
                        totalAmount = totalAmount,
                        totalPaid = totalPaid,
                        totalDue = totalDue,
                        paidCount = paidCount,
                        dueCount = dueCount,
                        paymentPercentage = billsList.Count > 0 ? Math.Round((paidCount * 100.0m / billsList.Count), 2) : 0,
                        totalFixedCharges = totalFixedCharges,
                        totalFoodCharges = totalFoodCharges,
                        totalPresentDays = totalPresentDays,
                        totalAbsentDays = totalAbsentDays,
                        averageBillAmount = billsList.Count > 0 ? Math.Round(totalAmount / billsList.Count, 2) : 0
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error generating report: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get monthly report for all users (Admin only)
        /// Shows all students' bills for a specific month with statistics
        /// </summary>
        /// <param name="year">Year (e.g., 2024)</param>
        /// <param name="month">Month (1-12)</param>
        [HttpGet("reports/monthly/{year:int}/{month:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMonthlyReport(int year, int month)
        {
            try
            {
                // Validate month and year
                if (month < 1 || month > 12)
                    return BadRequest(new { message = "Month must be between 1 and 12." });

                var now = DateTime.UtcNow;
                if (year < 2000 || year > now.Year)
                    return BadRequest(new { message = $"Year must be between 2000 and {now.Year}." });

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Get all bills for the specified month
                var allBills = await _billingService.GetAllBillsAsync(startDate, endDate);
                var bills = allBills
                    .Where(b => b.StartDate.Year == year && b.StartDate.Month == month)
                    .OrderBy(b => b.User?.RollNumber)
                    .ThenBy(b => b.User?.FirstName)
                    .ToList();

                if (!bills.Any())
                {
                    return Ok(new
                    {
                        month = startDate.ToString("MMMM yyyy"),
                        year = year,
                        monthNumber = month,
                        startDate = startDate.ToString("yyyy-MM-dd"),
                        endDate = endDate.ToString("yyyy-MM-dd"),
                        bills = new List<object>(),
                        summary = new
                        {
                            totalStudents = 0,
                            totalBilled = 0m,
                            totalPaid = 0m,
                            totalDue = 0m,
                            paidCount = 0,
                            dueCount = 0,
                            paymentPercentage = 0m
                        }
                    });
                }

                // Calculate statistics
                var totalBilled = bills.Sum(b => b.TotalAmount);
                var totalPaid = bills.Where(b => b.IsPaid).Sum(b => b.TotalAmount);
                var totalDue = bills.Where(b => !b.IsPaid).Sum(b => b.TotalAmount);
                var paidCount = bills.Count(b => b.IsPaid);
                var dueCount = bills.Count(b => !b.IsPaid);
                var totalFixedCharges = bills.Sum(b => b.TotalFixedCharges);
                var totalFoodCharges = bills.Sum(b => b.TotalFoodCharges);
                var totalPresentDays = bills.Sum(b => b.PresentDays);
                var totalAbsentDays = bills.Sum(b => b.AbsentDays);
                var totalDays = bills.Sum(b => b.TotalDays);

                return Ok(new
                {
                    month = startDate.ToString("MMMM yyyy"),
                    year = year,
                    monthNumber = month,
                    startDate = startDate.ToString("yyyy-MM-dd"),
                    endDate = endDate.ToString("yyyy-MM-dd"),
                    bills = bills.Select(b => new
                    {
                        billId = b.Id,
                        userId = b.UserId,
                        userName = b.User != null ? $"{b.User.FirstName} {b.User.LastName}" : "Unknown",
                        email = b.User?.Email,
                        rollNumber = b.User?.RollNumber,
                        roomNumber = b.User?.RoomNumber,
                        totalDays = b.TotalDays,
                        presentDays = b.PresentDays,
                        absentDays = b.AbsentDays,
                        attendancePercentage = b.TotalDays > 0 ? Math.Round((b.PresentDays * 100.0m / b.TotalDays), 2) : 0,
                        totalFixedCharges = b.TotalFixedCharges,
                        totalFoodCharges = b.TotalFoodCharges,
                        totalAmount = b.TotalAmount,
                        isPaid = b.IsPaid,
                        paidDate = b.PaidDate?.ToString("yyyy-MM-dd"),
                        generatedDate = b.GeneratedDate.ToString("yyyy-MM-dd")
                    }),
                    summary = new
                    {
                        totalStudents = bills.Count,
                        totalBilled = totalBilled,
                        totalPaid = totalPaid,
                        totalDue = totalDue,
                        paidCount = paidCount,
                        dueCount = dueCount,
                        paymentPercentage = bills.Count > 0 ? Math.Round((paidCount * 100.0m / bills.Count), 2) : 0,
                        totalFixedCharges = totalFixedCharges,
                        totalFoodCharges = totalFoodCharges,
                        totalPresentDays = totalPresentDays,
                        totalAbsentDays = totalAbsentDays,
                        totalDays = totalDays,
                        averageAttendancePercentage = totalDays > 0 ? Math.Round((totalPresentDays * 100.0m / totalDays), 2) : 0,
                        averageBillAmount = bills.Count > 0 ? Math.Round(totalBilled / bills.Count, 2) : 0,
                        averagePresentDays = bills.Count > 0 ? Math.Round((decimal)totalPresentDays / bills.Count, 2) : 0
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error generating monthly report: {ex.Message}" });
            }
        }

        /// <summary>
        /// Export monthly report as CSV (Admin only)
        /// Downloads all students' bills for a specific month
        /// </summary>
        /// <param name="year">Year (e.g., 2024)</param>
        /// <param name="month">Month (1-12)</param>
        [HttpGet("reports/monthly/{year:int}/{month:int}/export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportMonthlyReportCSV(int year, int month)
        {
            try
            {
                // Validate month and year
                if (month < 1 || month > 12)
                    return BadRequest(new { message = "Month must be between 1 and 12." });

                var now = DateTime.UtcNow;
                if (year < 2000 || year > now.Year)
                    return BadRequest(new { message = $"Year must be between 2000 and {now.Year}." });

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Get all bills for the specified month
                var allBills = await _billingService.GetAllBillsAsync(startDate, endDate);
                var bills = allBills
                    .Where(b => b.StartDate.Year == year && b.StartDate.Month == month)
                    .OrderBy(b => b.User?.RollNumber)
                    .ThenBy(b => b.User?.FirstName)
                    .ToList();

                if (!bills.Any())
                    return NotFound(new { message = $"No bills found for {startDate:MMMM yyyy}" });

                // Generate CSV
                var csv = new System.Text.StringBuilder();
                
                // CSV Header
                csv.AppendLine($"Monthly Report - {startDate:MMMM yyyy}");
                csv.AppendLine($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                csv.AppendLine();
                csv.AppendLine("Bill ID,User ID,Roll Number,Student Name,Email,Room Number,Total Days,Present Days,Absent Days,Attendance %,Fixed Charges,Food Charges,Total Amount,Payment Status,Paid Date,Generated Date");

                // CSV Data
                foreach (var bill in bills)
                {
                    var attendancePercentage = bill.TotalDays > 0 ? Math.Round((bill.PresentDays * 100.0m / bill.TotalDays), 2) : 0;
                    csv.AppendLine($"{bill.Id}," +
                        $"{bill.UserId}," +
                        $"\"{bill.User?.RollNumber ?? "N/A"}\"," +
                        $"\"{bill.User?.FirstName} {bill.User?.LastName}\"," +
                        $"\"{bill.User?.Email}\"," +
                        $"\"{bill.User?.RoomNumber ?? "N/A"}\"," +
                        $"{bill.TotalDays}," +
                        $"{bill.PresentDays}," +
                        $"{bill.AbsentDays}," +
                        $"{attendancePercentage:F2}%," +
                        $"{bill.TotalFixedCharges:F2}," +
                        $"{bill.TotalFoodCharges:F2}," +
                        $"{bill.TotalAmount:F2}," +
                        $"{(bill.IsPaid ? "Paid" : "Due")}," +
                        $"{(bill.PaidDate.HasValue ? bill.PaidDate.Value.ToString("yyyy-MM-dd") : "N/A")}," +
                        $"{bill.GeneratedDate:yyyy-MM-dd}");
                }

                // Add summary
                csv.AppendLine();
                csv.AppendLine("SUMMARY");
                csv.AppendLine($"Total Students,{bills.Count}");
                csv.AppendLine($"Total Billed,{bills.Sum(b => b.TotalAmount):F2}");
                csv.AppendLine($"Total Paid,{bills.Where(b => b.IsPaid).Sum(b => b.TotalAmount):F2}");
                csv.AppendLine($"Total Due,{bills.Where(b => !b.IsPaid).Sum(b => b.TotalAmount):F2}");
                csv.AppendLine($"Bills Paid,{bills.Count(b => b.IsPaid)}");
                csv.AppendLine($"Bills Due,{bills.Count(b => !b.IsPaid)}");
                csv.AppendLine($"Payment Percentage,{(bills.Count > 0 ? Math.Round((bills.Count(b => b.IsPaid) * 100.0m / bills.Count), 2) : 0):F2}%");
                csv.AppendLine($"Average Bill Amount,{(bills.Count > 0 ? Math.Round(bills.Sum(b => b.TotalAmount) / bills.Count, 2) : 0):F2}");

                // Generate filename
                var filename = $"Monthly_Report_{startDate:yyyy_MM}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

                // Return CSV file
                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", filename);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error exporting monthly report: {ex.Message}" });
            }
        }
    }

    // Request models
    public class GenerateBillRequest
    {
        public int UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
