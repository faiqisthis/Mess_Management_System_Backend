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

        public async Task<UserBill> GenerateMonthlyBillAsync(int userId, int year, int month)
        {
            // Validate that the requested month is not the current month
            var now = DateTime.UtcNow;
            if (year == now.Year && month >= now.Month)
            {
                throw new InvalidOperationException("Bills can only be generated for previous months, not the current month.");
            }

            // Validate month and year
            if (month < 1 || month > 12)
                throw new InvalidOperationException("Month must be between 1 and 12.");

            if (year < 2000 || year > now.Year)
                throw new InvalidOperationException($"Year must be between 2000 and {now.Year}.");

            // Check if user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found.");

            // Check if bill already exists for this month
            var existingBill = await _context.UserBills
                .Where(b => b.UserId == userId && 
                           b.StartDate.Year == year && 
                           b.StartDate.Month == month)
                .FirstOrDefaultAsync();

            if (existingBill != null)
                throw new InvalidOperationException($"Bill already exists for {new DateTime(year, month, 1):MMMM yyyy}.");

            // Calculate start and end dates for the month
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Get all menus in the month
            var menus = await _context.DailyMenus
                .Where(m => m.Date >= startDate && m.Date <= endDate)
                .ToListAsync();

            // Get user attendance in the month
            var attendances = await _context.Attendances
                .Where(a => a.UserId == userId && a.Date >= startDate && a.Date <= endDate)
                .ToListAsync();

            // Calculate total days in month
            var totalDays = (endDate - startDate).Days + 1;

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
                StartDate = startDate,
                EndDate = endDate,
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
