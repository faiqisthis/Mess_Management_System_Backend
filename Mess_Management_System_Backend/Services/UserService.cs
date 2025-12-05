using Mess_Management_System_Backend.Data;
using Mess_Management_System_Backend.Dtos;
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

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Email == normalizedEmail))
                throw new InvalidOperationException("Email already exists.");

            // Check if RollNumber is unique (if provided)
            if (!string.IsNullOrWhiteSpace(dto.RollNumber))
            {
                if (await _context.Users.AnyAsync(u => u.RollNumber == dto.RollNumber.Trim()))
                    throw new InvalidOperationException("Roll number already exists.");
            }

            var user = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = normalizedEmail,
                Role = dto.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RollNumber = dto.RollNumber?.Trim(),
                RoomNumber = dto.RoomNumber?.Trim(),
                ContactNumber = dto.ContactNumber?.Trim()
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            if (dto.FirstName != null)
                user.FirstName = dto.FirstName.Trim();

            if (dto.LastName != null)
                user.LastName = dto.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email.Trim().ToLowerInvariant();
            
            if (dto.Role.HasValue)
                user.Role = dto.Role.Value;
            
            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;
            
            // Update mess management fields
            if (dto.RollNumber != null)
                user.RollNumber = dto.RollNumber.Trim();
            
            if (dto.RoomNumber != null)
                user.RoomNumber = dto.RoomNumber.Trim();
            
            if (dto.ContactNumber != null)
                user.ContactNumber = dto.ContactNumber.Trim();

            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public AuthResponseDto? Authenticate(AuthRequestDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var user = _context.Users
                .FirstOrDefault(u => u.Email == normalizedEmail);

            if (user == null || !user.IsActive)
                return null;

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (verification == PasswordVerificationResult.Failed)
                return null;

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role.ToString(),
                Token = token
            };
        }

        private static UserDto MapToDto(User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            RollNumber = user.RollNumber,
            RoomNumber = user.RoomNumber,
            ContactNumber = user.ContactNumber
        };
    }
}
