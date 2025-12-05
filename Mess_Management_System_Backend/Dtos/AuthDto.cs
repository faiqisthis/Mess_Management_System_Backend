namespace Mess_Management_System_Backend.Dtos
{
    public class AuthRequestDto
    {
        required
        public string Email { get; set; } = string.Empty;
        required
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
