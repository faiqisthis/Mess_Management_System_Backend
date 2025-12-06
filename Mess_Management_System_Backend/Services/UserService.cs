using Mess_Management_System_Backend.Data;
using Mess_Management_System_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            // Validate required fields
            if (string.IsNullOrWhiteSpace(user.FirstName))
                throw new InvalidOperationException("First name is required.");
            
            if (string.IsNullOrWhiteSpace(user.LastName))
                throw new InvalidOperationException("Last name is required.");
            
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new InvalidOperationException("Email is required.");

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
                Role = user.Role ?? UserRole.Student,
                IsActive = user.IsActive ?? true,
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
            
            // Update role if provided
            if (updateData.Role.HasValue)
                user.Role = updateData.Role.Value;
            
            // Update IsActive if provided
            if (updateData.IsActive.HasValue)
                user.IsActive = updateData.IsActive.Value;
            
            // Update mess management fields (allow null to clear values)
            if (updateData.RollNumber != null)
                user.RollNumber = string.IsNullOrWhiteSpace(updateData.RollNumber) ? null : updateData.RollNumber.Trim();
            
            if (updateData.RoomNumber != null)
                user.RoomNumber = string.IsNullOrWhiteSpace(updateData.RoomNumber) ? null : updateData.RoomNumber.Trim();
            
            if (updateData.ContactNumber != null)
                user.ContactNumber = string.IsNullOrWhiteSpace(updateData.ContactNumber) ? null : updateData.ContactNumber.Trim();

            // Password changes are NOT handled here - use ChangePasswordAsync instead

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Verify current password
            var verification = _passwordHasher.VerifyHashedPassword(
                user, 
                user.PasswordHash, 
                request.CurrentPassword
            );

            if (verification == PasswordVerificationResult.Failed)
                return false; // Current password is incorrect

            // Hash and save new password
            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            await _context.SaveChangesAsync();

            return true;
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

            if (user == null || user.IsActive == false)
                return null;

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verification == PasswordVerificationResult.Failed)
                return null;

            var token = _jwtService.GenerateToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Role = user.Role?.ToString() ?? "Student",
                Token = token
            };
        }
    }
}
