using Mess_Management_System_Backend.Models;

namespace Mess_Management_System_Backend.Services
{
    public interface IBillingService
    {
        Task<UserBill> GenerateCurrentBillAsync(int userId, DateTime startDate, DateTime endDate);
        Task<UserBill?> GetUserBillByIdAsync(int billId);
        Task<IEnumerable<UserBill>> GetUserBillsAsync(int userId);
        Task<IEnumerable<UserBill>> GetAllBillsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<UserBill?> MarkBillAsPaidAsync(int billId);
        Task<bool> DeleteBillAsync(int billId);
    }
}
