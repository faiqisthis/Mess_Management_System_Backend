using Mess_Management_System_Backend.Data;

namespace Mess_Management_System_Backend.Models
{
    public enum UserRole
    {
        Student,
        Teacher,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Student;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Mess management fields (only applicable for Students)
        public string? RollNumber { get; set; }
        public string? RoomNumber { get; set; }
        public string? ContactNumber { get; set; }
    }
}
