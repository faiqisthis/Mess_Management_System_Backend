using Mess_Management_System_Backend.Models;

namespace Mess_Management_System_Backend.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Optional mess management fields
        public string? RollNumber { get; set; }
        public string? RoomNumber { get; set; }
        public string? ContactNumber { get; set; }
    }
}
