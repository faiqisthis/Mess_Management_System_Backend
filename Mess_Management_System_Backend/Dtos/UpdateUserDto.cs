using System.ComponentModel.DataAnnotations;

namespace Mess_Management_System_Backend.Dtos
{
    public class UpdateUserDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public string? Role { get; set; }

        public bool? IsActive { get; set; }
    }
}
