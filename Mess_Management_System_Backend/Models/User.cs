using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
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
        
        [MaxLength(100)]
        public string? FirstName { get; set; }
        
        [MaxLength(100)]
        public string? LastName { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        // This is stored in database but NEVER sent to client
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        
        // This is used for input only (create/login), never stored or returned
        [JsonIgnore]
        [NotMapped] // Not stored in database
        public string? Password { get; set; }
        
        public UserRole? Role { get; set; }
        
        public bool? IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Mess management fields (only applicable for Students)
        public string? RollNumber { get; set; }
        public string? RoomNumber { get; set; }
        public string? ContactNumber { get; set; }
    }
    
    // Simple classes for login/auth responses
    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
    
    // Password change request
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required, MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
