using Mess_Management_System_Backend.Data;
using Mess_Management_System_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Mess_Management_System_Backend.Services
{
    public class BillingService : IBillingService
    {
        private readonly ApplicationDbContext _context;

        public BillingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserBill> GenerateCurrentBillAsync(int userId, DateTime startDate, DateTime endDate)
        {
            // Normalize dates
            var normalizedStart = startDate.Date;
            var normalizedEnd = endDate.Date;

            // Check if user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found.");

            // Get all menus in the date range
            var menus = await _context.DailyMenus
                .Where(m => m.Date >= normalizedStart && m.Date <= normalizedEnd)
                .ToListAsync();

            // Get user attendance in the date range
            var attendances = await _context.Attendances
                .Where(a => a.UserId == userId && a.Date >= normalizedStart && a.Date <= normalizedEnd)
                .ToListAsync();

            // Calculate total days in period
            var totalDays = (normalizedEnd - normalizedStart).Days + 1;

            // Calculate fixed charges (tea + water for ALL days)
            decimal totalFixedCharges = 0;
            foreach (var menu in menus)
            {
                totalFixedCharges += menu.DailyFixedCharge;
            }

            // If no menus exist for some days, use default fixed charge
            var daysWithoutMenu = totalDays - menus.Count;
            if (daysWithoutMenu > 0)
            {
                totalFixedCharges += daysWithoutMenu * 20; // Default fixed charge
            }

            // Calculate food charges (only for PRESENT days)
            decimal totalFoodCharges = 0;
            var presentDays = 0;

            foreach (var attendance in attendances.Where(a => a.Status == AttendanceStatus.Present))
            {
                presentDays++;
                var menu = menus.FirstOrDefault(m => m.Date == attendance.Date);
                
                if (menu != null)
                {
                    // Sum all meal prices for that day
                    totalFoodCharges += menu.Meals.Sum(meal => meal.Price);
                }
            }

            var absentDays = totalDays - presentDays;

            var bill = new UserBill
            {
                UserId = userId,
                StartDate = normalizedStart,
                EndDate = normalizedEnd,
                TotalFixedCharges = totalFixedCharges,
                TotalFoodCharges = totalFoodCharges,
                TotalAmount = totalFixedCharges + totalFoodCharges,
                TotalDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                GeneratedDate = DateTime.UtcNow,
                IsPaid = false
            };

            _context.UserBills.Add(bill);
            await _context.SaveChangesAsync();

            return bill;
        }

        public async Task<UserBill?> GetUserBillByIdAsync(int billId)
        {
            return await _context.UserBills
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == billId);
        }

        public async Task<IEnumerable<UserBill>> GetUserBillsAsync(int userId)
        {
            return await _context.UserBills
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.GeneratedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserBill>> GetAllBillsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.UserBills.Include(b => b.User).AsQueryable();

            if (startDate.HasValue)
                query = query.Where(b => b.StartDate >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(b => b.EndDate <= endDate.Value.Date);

            return await query
                .OrderByDescending(b => b.GeneratedDate)
                .ToListAsync();
        }

        public async Task<UserBill?> MarkBillAsPaidAsync(int billId)
        {
            var bill = await _context.UserBills.FindAsync(billId);
            if (bill == null) return null;

            bill.IsPaid = true;
            bill.PaidDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return bill;
        }

        public async Task<bool> DeleteBillAsync(int billId)
        {
            var bill = await _context.UserBills.FindAsync(billId);
            if (bill == null) return false;

            _context.UserBills.Remove(bill);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
