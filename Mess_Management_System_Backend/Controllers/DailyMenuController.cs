using Mess_Management_System_Backend.Models;
using Mess_Management_System_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mess_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DailyMenuController : ControllerBase
    {
        private readonly IDailyMenuService _menuService;

        public DailyMenuController(IDailyMenuService menuService)
        {
            _menuService = menuService;
        }

        /// <summary>
        /// Create a new daily menu (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMenu([FromBody] DailyMenu menu)
        {
            try
            {
                var createdMenu = await _menuService.CreateMenuAsync(menu);
                return CreatedAtAction(nameof(GetMenuById), new { id = createdMenu.Id }, createdMenu);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update existing menu (Admin only)
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMenu(int id, [FromBody] DailyMenu menu)
        {
            var updatedMenu = await _menuService.UpdateMenuAsync(id, menu);
            if (updatedMenu == null)
                return NotFound(new { message = $"Menu with ID {id} not found" });

            return Ok(updatedMenu);
        }

        /// <summary>
        /// Get menu by ID (Anyone can view)
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMenuById(int id)
        {
            var menu = await _menuService.GetMenuByIdAsync(id);
            if (menu == null)
                return NotFound(new { message = $"Menu with ID {id} not found" });

            return Ok(menu);
        }

        /// <summary>
        /// Get menu for a specific date (Anyone can view)
        /// </summary>
        [HttpGet("date/{date}")]
        public async Task<IActionResult> GetMenuByDate(DateTime date)
        {
            var menu = await _menuService.GetMenuByDateAsync(date);
            if (menu == null)
                return NotFound(new { message = $"No menu found for date {date:yyyy-MM-dd}" });

            return Ok(menu);
        }

        /// <summary>
        /// Get menus for a date range (Anyone can view)
        /// </summary>
        [HttpGet("range")]
        public async Task<IActionResult> GetMenusInRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var menus = await _menuService.GetMenusInRangeAsync(startDate, endDate);
            return Ok(menus);
        }

        /// <summary>
        /// Delete menu (Admin only)
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            var result = await _menuService.DeleteMenuAsync(id);
            if (!result)
                return NotFound(new { message = $"Menu with ID {id} not found" });

            return NoContent();
        }
    }
}
