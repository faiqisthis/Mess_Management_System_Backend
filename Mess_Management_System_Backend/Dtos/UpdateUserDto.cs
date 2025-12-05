using System.ComponentModel.DataAnnotations;
using Mess_Management_System_Backend.Models;

namespace Mess_Management_System_Backend.Dtos
{
    public class UpdateUserDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public UserRole? Role { get; set; }

        public bool? IsActive { get; set; }
        
        // Optional mess management fields
        public string? RollNumber { get; set; }
        public string? RoomNumber { get; set; }
        public string? ContactNumber { get; set; }
    }
}
