using Mess_Management_System_Backend.Models;

namespace Mess_Management_System_Backend.Services
{
    public interface IAttendanceService
    {
        Task<Attendance> MarkAttendanceAsync(int userId, DateTime date, AttendanceStatus status);
        Task<IEnumerable<Attendance>> MarkBulkAttendanceAsync(DateTime date, List<UserAttendanceRecord> records);
        Task<Attendance?> UpdateAttendanceAsync(int attendanceId, AttendanceStatus status);
        Task<IEnumerable<Attendance>> GetAttendanceByDateAsync(DateTime date);
        Task<Attendance?> GetAttendanceByUserAndDateAsync(int userId, DateTime date);
        Task<bool> DeleteAttendanceAsync(int attendanceId);

        /// <summary>
        /// Get user attendance with flexible filtering and optional detailed summary
        /// Supports both date range (startDate/endDate) and month-based (year/month) queries
        /// </summary>
        Task<object> GetUserAttendanceAsync(
            int userId, 
            DateTime? startDate = null, 
            DateTime? endDate = null,
            int? year = null,
            int? month = null,
            bool includeSummary = false,
            bool includeMenuDetails = false
        );
    }

    public class UserAttendanceRecord
    {
        public int UserId { get; set; }
        public AttendanceStatus Status { get; set; }
    }
}
