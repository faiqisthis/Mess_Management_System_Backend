using Mess_Management_System_Backend.Data;
using Mess_Management_System_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Mess_Management_System_Backend.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Attendance> MarkAttendanceAsync(int userId, DateTime date, AttendanceStatus status)
        {
            // Normalize date to remove time component
            var normalizedDate = date.Date;

            // Check if user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {userId} not found.");

            // Check if attendance already exists for this user on this date
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == normalizedDate);

            if (existingAttendance != null)
                throw new InvalidOperationException($"Attendance already marked for user {userId} on {normalizedDate:yyyy-MM-dd}. Use update instead.");

            var attendance = new Attendance
            {
                UserId = userId,
                Date = normalizedDate,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return attendance;
        }

        public async Task<IEnumerable<Attendance>> MarkBulkAttendanceAsync(DateTime date, List<UserAttendanceRecord> records)
        {
            var normalizedDate = date.Date;
            var attendances = new List<Attendance>();
            var errors = new List<string>();

            // Get all user IDs to validate they exist
            var userIds = records.Select(r => r.UserId).Distinct().ToList();
            var existingUsers = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            // Check for already existing attendance records
            var existingAttendances = await _context.Attendances
                .Where(a => a.Date == normalizedDate && userIds.Contains(a.UserId))
                .Select(a => a.UserId)
                .ToListAsync();

            foreach (var record in records)
            {
                // Validate user exists
                if (!existingUsers.Contains(record.UserId))
                {
                    errors.Add($"User with ID {record.UserId} not found.");
                    continue;
                }

                // Check if attendance already exists
                if (existingAttendances.Contains(record.UserId))
                {
                    errors.Add($"Attendance already exists for user {record.UserId} on {normalizedDate:yyyy-MM-dd}.");
                    continue;
                }

                var attendance = new Attendance
                {
                    UserId = record.UserId,
                    Date = normalizedDate,
                    Status = record.Status,
                    CreatedAt = DateTime.UtcNow
                };

                attendances.Add(attendance);
            }

            if (errors.Any())
            {
                throw new InvalidOperationException($"Bulk attendance marking failed with errors: {string.Join("; ", errors)}");
            }

            if (attendances.Any())
            {
                _context.Attendances.AddRange(attendances);
                await _context.SaveChangesAsync();
            }

            return attendances;
        }

        public async Task<Attendance?> UpdateAttendanceAsync(int attendanceId, AttendanceStatus status)
        {
            var attendance = await _context.Attendances.FindAsync(attendanceId);
            if (attendance == null) return null;

            attendance.Status = status;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return attendance;
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByDateAsync(DateTime date)
        {
            var normalizedDate = date.Date;
            return await _context.Attendances
                .Include(a => a.User)
                .Where(a => a.Date == normalizedDate)
                .OrderBy(a => a.User.FirstName)
                .ToListAsync();
        }

        public async Task<object> GetUserAttendanceAsync(
            int userId, 
            DateTime? startDate = null, 
            DateTime? endDate = null,
            int? year = null,
            int? month = null,
            bool includeSummary = false,
            bool includeMenuDetails = false)
        {
            // Determine date range from either explicit dates or year/month
            DateTime rangeStart;
            DateTime rangeEnd;

            if (year.HasValue && month.HasValue)
            {
                // Validate month
                if (month < 1 || month > 12)
                    throw new InvalidOperationException("Month must be between 1 and 12.");

                rangeStart = new DateTime(year.Value, month.Value, 1);
                rangeEnd = rangeStart.AddMonths(1).AddDays(-1);
            }
            else if (startDate.HasValue || endDate.HasValue)
            {
                rangeStart = startDate?.Date ?? DateTime.MinValue;
                rangeEnd = endDate?.Date ?? DateTime.MaxValue;
            }
            else
            {
                // No filters provided - return simple list
                var allAttendances = await _context.Attendances
                    .Where(a => a.UserId == userId)
                    .OrderBy(a => a.Date)
                    .ToListAsync();
                
                return allAttendances;
            }

            // Get attendance records for the date range
            var attendances = await _context.Attendances
                .Where(a => a.UserId == userId && a.Date >= rangeStart && a.Date <= rangeEnd)
                .OrderBy(a => a.Date)
                .ToListAsync();

            // If no summary or menu details requested, return simple list
            if (!includeSummary && !includeMenuDetails)
            {
                return attendances;
            }

            // Get menus for the date range if menu details are requested
            List<DailyMenu> menus = new List<DailyMenu>();
            if (includeMenuDetails)
            {
                menus = await _context.DailyMenus
                    .Where(m => m.Date >= rangeStart && m.Date <= rangeEnd)
                    .OrderBy(m => m.Date)
                    .ToListAsync();
            }

            // Calculate summary statistics
            var totalDays = (rangeEnd - rangeStart).Days + 1;
            var presentDays = attendances.Count(a => a.Status == AttendanceStatus.Present);
            var absentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent);
            var daysWithoutAttendance = totalDays - attendances.Count;

            decimal estimatedFixedCharges = 0;
            decimal estimatedFoodCharges = 0;

            if (includeMenuDetails)
            {
                estimatedFixedCharges = menus.Sum(m => m.DailyFixedCharge);
                if (menus.Count < totalDays)
                {
                    estimatedFixedCharges += (totalDays - menus.Count) * 20; // Default fixed charge
                }
            }

            // Build daily summaries if menu details are requested
            var dailySummaries = new List<object>();
            if (includeMenuDetails)
            {
                for (var date = rangeStart; date <= rangeEnd; date = date.AddDays(1))
                {
                    var attendance = attendances.FirstOrDefault(a => a.Date == date);
                    var menu = menus.FirstOrDefault(m => m.Date == date);
                    
                    var dailyFoodCost = menu?.Meals.Sum(m => m.Price) ?? 0;
                    var dailyFixedCharge = menu?.DailyFixedCharge ?? 20;
                    
                    // Add to food charges only if present
                    if (attendance?.Status == AttendanceStatus.Present)
                    {
                        estimatedFoodCharges += dailyFoodCost;
                    }

                    dailySummaries.Add(new
                    {
                        date = date.ToString("yyyy-MM-dd"),
                        status = attendance?.Status.ToString() ?? "NotMarked",
                        meals = menu?.Meals ?? new List<MealItem>(),
                        dailyFixedCharge = dailyFixedCharge,
                        dailyFoodCost = dailyFoodCost,
                        dailyTotalCost = attendance?.Status == AttendanceStatus.Present 
                            ? dailyFixedCharge + dailyFoodCost 
                            : dailyFixedCharge,
                        menuAvailable = menu != null
                    });
                }
            }

            // Build response based on what was requested
            var response = new Dictionary<string, object>
            {
                { "userId", userId },
                { "startDate", rangeStart.ToString("yyyy-MM-dd") },
                { "endDate", rangeEnd.ToString("yyyy-MM-dd") }
            };

            if (year.HasValue && month.HasValue)
            {
                response.Add("year", year.Value);
                response.Add("month", month.Value);
                response.Add("monthName", rangeStart.ToString("MMMM yyyy"));
            }

            if (includeMenuDetails)
            {
                response.Add("dailySummaries", dailySummaries);
            }
            else
            {
                response.Add("attendances", attendances);
            }

            if (includeSummary)
            {
                response.Add("summary", new
                {
                    totalDays = totalDays,
                    presentDays = presentDays,
                    absentDays = absentDays,
                    daysWithoutAttendance = daysWithoutAttendance,
                    estimatedFixedCharges = estimatedFixedCharges,
                    estimatedFoodCharges = estimatedFoodCharges,
                    estimatedTotalCost = estimatedFixedCharges + estimatedFoodCharges
                });
            }

            if (includeMenuDetails)
            {
                response.Add("note", "This is an estimated cost based on your attendance. Actual bill may vary and will be generated by admin at the start of next month.");
            }

            return response;
        }

        public async Task<Attendance?> GetAttendanceByUserAndDateAsync(int userId, DateTime date)
        {
            var normalizedDate = date.Date;
            return await _context.Attendances
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == normalizedDate);
        }

        public async Task<bool> DeleteAttendanceAsync(int attendanceId)
        {
            var attendance = await _context.Attendances.FindAsync(attendanceId);
            if (attendance == null) return false;

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
