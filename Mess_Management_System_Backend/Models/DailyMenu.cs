using System.ComponentModel.DataAnnotations;

namespace Mess_Management_System_Backend.Models
{
    /// <summary>
    /// Represents the daily menu for a specific date
    /// Only one menu per date
    /// </summary>
    public class DailyMenu
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Meals stored as JSON arrays for easy destructuring
        public List<MealItem> Meals { get; set; } = new List<MealItem>();

        // Fixed daily charges (tea + water) - applies to everyone regardless of attendance
        [Required]
        public decimal DailyFixedCharge { get; set; } = 20; // Default value

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents a single meal item in the menu
    /// </summary>
    public class MealItem
    {
        [Required]
        public string Name { get; set; } = string.Empty; // e.g., "Chicken Biryani"

        [Required]
        public MealType Type { get; set; } // Breakfast, Lunch, Dinner

        [Required]
        public decimal Price { get; set; }
    }

    public enum MealType
    {
        Breakfast = 0,
        Lunch = 1,
        Dinner = 2
    }
}
