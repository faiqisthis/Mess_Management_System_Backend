using Mess_Management_System_Backend.Data;
using Mess_Management_System_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Mess_Management_System_Backend.Services
{
    public class DailyMenuService : IDailyMenuService
    {
        private readonly ApplicationDbContext _context;

        public DailyMenuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DailyMenu> CreateMenuAsync(DailyMenu menu)
        {
            // Normalize date to remove time component
            var normalizedDate = menu.Date.Date;

            // Check if menu already exists for this date
            var existingMenu = await _context.DailyMenus
                .FirstOrDefaultAsync(m => m.Date == normalizedDate);

            if (existingMenu != null)
                throw new InvalidOperationException($"Menu already exists for date {normalizedDate:yyyy-MM-dd}. Use update instead.");

            menu.Date = normalizedDate;
            menu.CreatedAt = DateTime.UtcNow;

            _context.DailyMenus.Add(menu);
            await _context.SaveChangesAsync();

            return menu;
        }

        public async Task<DailyMenu?> UpdateMenuAsync(int menuId, DailyMenu updatedMenu)
        {
            var menu = await _context.DailyMenus.FindAsync(menuId);
            if (menu == null) return null;

            // Update fields
            menu.Meals = updatedMenu.Meals;
            menu.DailyFixedCharge = updatedMenu.DailyFixedCharge;
            menu.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return menu;
        }

        public async Task<DailyMenu?> GetMenuByDateAsync(DateTime date)
        {
            var normalizedDate = date.Date;
            return await _context.DailyMenus
                .FirstOrDefaultAsync(m => m.Date == normalizedDate);
        }

        public async Task<DailyMenu?> GetMenuByIdAsync(int menuId)
        {
            return await _context.DailyMenus.FindAsync(menuId);
        }

        public async Task<IEnumerable<DailyMenu>> GetMenusInRangeAsync(DateTime startDate, DateTime endDate)
        {
            var normalizedStart = startDate.Date;
            var normalizedEnd = endDate.Date;

            return await _context.DailyMenus
                .Where(m => m.Date >= normalizedStart && m.Date <= normalizedEnd)
                .OrderBy(m => m.Date)
                .ToListAsync();
        }

        public async Task<bool> DeleteMenuAsync(int menuId)
        {
            var menu = await _context.DailyMenus.FindAsync(menuId);
            if (menu == null) return false;

            _context.DailyMenus.Remove(menu);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
