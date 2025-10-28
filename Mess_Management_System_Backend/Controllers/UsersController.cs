using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Mess_Management_System_Backend.Data;
using Mess_Management_System_Backend.Models;
using Mess_Management_System_Backend.Dtos;

namespace Mess_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UsersController(ApplicationDbContext db, IPasswordHasher<User> passwordHasher)
        {
            _db = db;
            _passwordHasher = passwordHasher;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var users = await _db.Users
                                .Where(u => u.IsActive) // default filter
                                .OrderBy(u => u.Id)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .Select(u => new UserDto
                                {
                                    Id = u.Id,
                                    FirstName = u.FirstName,
                                    LastName = u.LastName,
                                    Email = u.Email,
                                    Role = u.Role.ToString(),
                                    IsActive = u.IsActive,
                                    CreatedAt = u.CreatedAt
                                })
                                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null || !u.IsActive) return NotFound();

            var dto = new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            };
            return Ok(dto);
        }

        // POST: api/users  (Register)
        [HttpPost]
        public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var emailLower = input.Email.Trim().ToLowerInvariant();
            if (await _db.Users.AnyAsync(x => x.Email.ToLower() == emailLower))
                return Conflict(new { message = "Email already in use." });

            // Map to entity
            var user = new User
            {
                FirstName = input.FirstName.Trim(),
                LastName = input.LastName.Trim(),
                Email = emailLower,
                Role = Enum.TryParse<UserRole>(input.Role, true, out var r) ? r : UserRole.Student,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Hash password
            user.PasswordHash = _passwordHasher.HashPassword(user, input.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var dto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, dto);
        }

        // PUT: api/users/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _db.Users.FindAsync(id);
            if (user == null || !user.IsActive) return NotFound();

            user.FirstName = input.FirstName.Trim();
            user.LastName = input.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(input.Email))
            {
                var newEmail = input.Email.Trim().ToLowerInvariant();
                if (await _db.Users.AnyAsync(u => u.Id != id && u.Email.ToLower() == newEmail))
                    return Conflict(new { message = "Email already in use." });

                user.Email = newEmail;
            }

            if (!string.IsNullOrWhiteSpace(input.Role))
            {
                if (Enum.TryParse<UserRole>(input.Role, true, out var parsedRole))
                    user.Role = parsedRole;
            }

            if (input.IsActive.HasValue)
                user.IsActive = input.IsActive.Value;

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/users/5  (soft delete)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/users/authenticate  (very basic - later replace with JWT)
        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthResponseDto>> Authenticate([FromBody] AuthRequestDto request)
        {
            var emailLower = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == emailLower && u.IsActive);
            if (user == null) return Unauthorized();

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verify == PasswordVerificationResult.Success)
            {
                var resp = new AuthResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Role = user.Role.ToString()
                };
                return Ok(resp);
            }

            return Unauthorized();
        }
    }
}
