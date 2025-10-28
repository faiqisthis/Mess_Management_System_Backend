using Mess_Management_System_Backend.Dtos;
using Mess_Management_System_Backend.Services;
using Mess_Management_System_Backend.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Mess_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid input data"));

            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = user.Id },
                ApiResponse<UserDto>.Ok(user, "User created successfully"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<IEnumerable<UserDto>>.Ok(users, "Fetched all users successfully"));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail($"User with ID {id} not found"));

            return Ok(ApiResponse<UserDto>.Ok(user, "Fetched user successfully"));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid input data"));

            var updatedUser = await _userService.UpdateUserAsync(id, dto);
            if (updatedUser == null)
                return NotFound(ApiResponse<string>.Fail($"User with ID {id} not found"));

            return Ok(ApiResponse<UserDto>.Ok(updatedUser, "User updated successfully"));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
                return NotFound(ApiResponse<string>.Fail($"User with ID {id} not found"));

            return Ok(ApiResponse<string>.Ok(null, "User deleted successfully"));
        }
    }
}
