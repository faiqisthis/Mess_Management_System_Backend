using System.ComponentModel.DataAnnotations;

namespace Mess_Management_System_Backend.Models
{
    /// <summary>
    /// Represents a bill for a user for a specific period (usually monthly)
    /// </summary>
    public class UserBill
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Total fixed charges (tea + water) for all days in period
        /// </summary>
        public decimal TotalFixedCharges { get; set; }

        /// <summary>
        /// Total food charges (only for days marked present)
        /// </summary>
        public decimal TotalFoodCharges { get; set; }

        /// <summary>
        /// Total amount = Fixed charges + Food charges
        /// </summary>
        public decimal TotalAmount { get; set; }

        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }

        public bool IsPaid { get; set; } = false;
        public DateTime? PaidDate { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; } = null!;
    }
}
