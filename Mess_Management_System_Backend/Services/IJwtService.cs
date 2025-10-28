using Mess_Management_System_Backend.Models;

namespace Mess_Management_System_Backend.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}