using AutoMapper;
using Mess_Management_System_Backend.Data;
using Mess_Management_System_Backend.Dtos;
using Mess_Management_System_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Identity;

namespace Mess_Management_System_Backend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IJwtService _jwtService;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, IMapper mapper, IJwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already exists.");

            var user = _mapper.Map<User>(dto);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created user with email: {Email}", user.Email);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return null;

            _mapper.Map(dto, user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated user {Id}", id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted user {Id}", id);
            return true;
        }

        public async Task<AuthResponseDto?> AuthenticateAsync(AuthRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
    
            if (user == null || !user.IsActive)
                return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
    
            if (result == PasswordVerificationResult.Failed)
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
    }
}
