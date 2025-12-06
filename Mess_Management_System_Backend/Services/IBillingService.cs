using Mess_Management_System_Backend.Models;

namespace Mess_Management_System_Backend.Services
{
    public interface IBillingService
    {
        /// <summary>
        /// Generate bill for a specific month (only for previous months, not current month)
        /// </summary>
        Task<UserBill> GenerateMonthlyBillAsync(int userId, int year, int month);
        
        Task<UserBill?> GetUserBillByIdAsync(int billId);
        Task<IEnumerable<UserBill>> GetUserBillsAsync(int userId);
        Task<IEnumerable<UserBill>> GetAllBillsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<UserBill?> MarkBillAsPaidAsync(int billId);
        Task<bool> DeleteBillAsync(int billId);
    }
}
