using Mess_Management_System_Backend.Models;

namespace Mess_Management_System_Backend.Services
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> UpdateUserAsync(int id, User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        AuthResponse? Authenticate(LoginRequest request);
    }
}
