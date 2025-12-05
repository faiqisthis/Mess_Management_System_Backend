using Mess_Management_System_Backend.Models;

namespace Mess_Management_System_Backend.Services
{
    public interface IDailyMenuService
    {
        Task<DailyMenu> CreateMenuAsync(DailyMenu menu);
        Task<DailyMenu?> UpdateMenuAsync(int menuId, DailyMenu menu);
        Task<DailyMenu?> GetMenuByDateAsync(DateTime date);
        Task<DailyMenu?> GetMenuByIdAsync(int menuId);
        Task<IEnumerable<DailyMenu>> GetMenusInRangeAsync(DateTime startDate, DateTime endDate);
        Task<bool> DeleteMenuAsync(int menuId);
    }
}
