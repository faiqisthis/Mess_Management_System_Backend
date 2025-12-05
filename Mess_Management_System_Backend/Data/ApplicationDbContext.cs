using Microsoft.EntityFrameworkCore;
using Mess_Management_System_Backend.Models;
using System;
using System.Collections.Generic;

namespace Mess_Management_System_Backend.Data
{
    // 1. The Database Context
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Users (Student, Teacher, Admin)
        public DbSet<User> Users { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<DailyAttendance> Attendances { get; set; }
        public DbSet<Bill> Bills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set unique index on RollNumber (for students who have one)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.RollNumber)
                .IsUnique()
                .HasFilter("[RollNumber] IS NOT NULL");
        }
    }

    // 2. The Entity Classes

    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Chicken Biryani"
        public string DayOfWeek { get; set; } = string.Empty; // e.g., "Monday"
        public string MealType { get; set; } = string.Empty; // "Lunch" or "Dinner"
        public decimal Price { get; set; }
    }

    public class DailyAttendance
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool HadBreakfast { get; set; }
        public bool HadLunch { get; set; }
        public bool HadDinner { get; set; }

        // Foreign Key
        public int UserID { get; set; }
        public User User { get; set; } = null!;
    }

    public class Bill
    {
        public int Id { get; set; }
        public string Month { get; set; } = string.Empty; // e.g., "November 2023"
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        // Foreign Key
        public int UserID { get; set; }
        public User User { get; set; } = null!;
    }
}