using Mess_Management_System_Backend.Data;
using Mess_Management_System_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Mess_Management_System_Backend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            IJwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var normalizedEmail = user.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == normalizedEmail))
                throw new InvalidOperationException("Email already exists.");

            // Check if RollNumber is unique (if provided)
            if (!string.IsNullOrWhiteSpace(user.RollNumber))
            {
                if (await _context.Users.AnyAsync(u => u.RollNumber == user.RollNumber.Trim()))
                    throw new InvalidOperationException("Roll number already exists.");
            }

            if (string.IsNullOrWhiteSpace(user.Password))
                throw new InvalidOperationException("Password is required.");

            var newUser = new User
            {
                FirstName = user.FirstName.Trim(),
                LastName = user.LastName.Trim(),
                Email = normalizedEmail,
                Role = user.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RollNumber = user.RollNumber?.Trim(),
                RoomNumber = user.RoomNumber?.Trim(),
                ContactNumber = user.ContactNumber?.Trim()
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, user.Password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> UpdateUserAsync(int id, User updateData)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            // Only update fields that are provided (not null or empty)
            if (!string.IsNullOrWhiteSpace(updateData.FirstName))
                user.FirstName = updateData.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(updateData.LastName))
                user.LastName = updateData.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(updateData.Email))
                user.Email = updateData.Email.Trim().ToLowerInvariant();
            
            // Update role if different
            if (updateData.Role != default(UserRole))
                user.Role = updateData.Role;
            
            // Update IsActive - we need to check if it's explicitly set
            user.IsActive = updateData.IsActive;
            
            // Update mess management fields
            if (updateData.RollNumber != null)
                user.RollNumber = updateData.RollNumber.Trim();
            
            if (updateData.RoomNumber != null)
                user.RoomNumber = updateData.RoomNumber.Trim();
            
            if (updateData.ContactNumber != null)
                user.ContactNumber = updateData.ContactNumber.Trim();

            // Handle password update if provided
            if (!string.IsNullOrWhiteSpace(updateData.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, updateData.Password);
            }

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public AuthResponse? Authenticate(LoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = _context.Users
                .FirstOrDefault(u => u.Email == normalizedEmail);

            if (user == null || !user.IsActive)
                return null;

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verification == PasswordVerificationResult.Failed)
                return null;

            var token = _jwtService.GenerateToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = token
            };
        }
    }
}
