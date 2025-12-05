using System.ComponentModel.DataAnnotations;

namespace Mess_Management_System_Backend.Models
{
    /// <summary>
    /// Represents daily attendance for a user
    /// </summary>
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// Attendance status: Present or Absent
        /// </summary>
        [Required]
        public AttendanceStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }

    public enum AttendanceStatus
    {
        Absent = 0,
        Present = 1
    }
}
