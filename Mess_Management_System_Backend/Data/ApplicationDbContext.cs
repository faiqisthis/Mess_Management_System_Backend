using Microsoft.EntityFrameworkCore;
using Mess_Management_System_Backend.Models;

namespace Mess_Management_System_Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Users
        public DbSet<User> Users { get; set; }
        
        // Attendance Management
        public DbSet<Attendance> Attendances { get; set; }
        
        // Menu Management
        public DbSet<DailyMenu> DailyMenus { get; set; }
        
        // Billing
        public DbSet<UserBill> UserBills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User: Unique index on RollNumber (for students who have one)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.RollNumber)
                .IsUnique()
                .HasFilter("[RollNumber] IS NOT NULL");

            // Attendance: Unique constraint on UserId + Date (one attendance record per user per day)
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.UserId, a.Date })
                .IsUnique();

            // DailyMenu: Unique constraint on Date (one menu per date)
            modelBuilder.Entity<DailyMenu>()
                .HasIndex(m => m.Date)
                .IsUnique();

            // DailyMenu: Store Meals as JSON
            modelBuilder.Entity<DailyMenu>()
                .Property(m => m.Meals)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<MealItem>>(v, (System.Text.Json.JsonSerializerOptions)null) ?? new List<MealItem>()
                );

            // DailyMenu: Configure decimal precision for DailyFixedCharge
            modelBuilder.Entity<DailyMenu>()
                .Property(m => m.DailyFixedCharge)
                .HasPrecision(18, 2);

            // UserBill: Configure decimal precision
            modelBuilder.Entity<UserBill>()
                .Property(b => b.TotalFixedCharges)
                .HasPrecision(18, 2);

            modelBuilder.Entity<UserBill>()
                .Property(b => b.TotalFoodCharges)
                .HasPrecision(18, 2);

            modelBuilder.Entity<UserBill>()
                .Property(b => b.TotalAmount)
                .HasPrecision(18, 2);

            // Configure relationships
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserBill>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}