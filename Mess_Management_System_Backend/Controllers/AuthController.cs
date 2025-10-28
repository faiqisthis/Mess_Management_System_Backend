using Mess_Management_System_Backend.Dtos;
using Mess_Management_System_Backend.Services;
using Mess_Management_System_Backend.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Mess_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestDto dto)
        {
            var result = await _userService.AuthenticateAsync(dto);

            if (result == null)
                return Unauthorized(ApiResponse<object>.Fail("Invalid email or password"));

            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful"));
        }
    }
}