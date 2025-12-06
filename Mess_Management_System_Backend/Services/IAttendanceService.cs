using Mess_Management_System_Backend.Models;

namespace Mess_Management_System_Backend.Services
{
    public interface IAttendanceService
    {
        Task<Attendance> MarkAttendanceAsync(int userId, DateTime date, AttendanceStatus status);
        Task<IEnumerable<Attendance>> MarkBulkAttendanceAsync(DateTime date, List<UserAttendanceRecord> records);
        Task<Attendance?> UpdateAttendanceAsync(int attendanceId, AttendanceStatus status);
        Task<IEnumerable<Attendance>> GetAttendanceByDateAsync(DateTime date);
        Task<IEnumerable<Attendance>> GetUserAttendanceAsync(int userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<Attendance?> GetAttendanceByUserAndDateAsync(int userId, DateTime date);
        Task<bool> DeleteAttendanceAsync(int attendanceId);

        /// <summary>
        /// Get user attendance with menu details for a specific month (for students to estimate their bill)
        /// </summary>
        Task<object> GetUserAttendanceWithMenuAsync(int userId, int year, int month);
    }

    public class UserAttendanceRecord
    {
        public int UserId { get; set; }
        public AttendanceStatus Status { get; set; }
    }
}
