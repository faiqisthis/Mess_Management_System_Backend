using Mess_Management_System_Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mess_Management_System_Backend.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            // Check if database has any users
            if (await context.Users.AnyAsync())
            {
                Console.WriteLine("Database already contains users. Skipping seeding.");
                return;
            }

            Console.WriteLine("Seeding database with initial users...");

            var users = new List<User>
            {
                // Admin User
                new User
                {
                    FirstName = "Admin",
                    LastName = "Master",
                    Email = "admin@mess.com",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ContactNumber = "+1234567890"
                },

                // Teacher User
                new User
                {
                    FirstName = "John",
                    LastName = "Teacher",
                    Email = "teacher@mess.com",
                    Role = UserRole.Teacher,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ContactNumber = "+1234567891"
                },

                // Student Users
                new User
                {
                    FirstName = "Alice",
                    LastName = "Student",
                    Email = "alice@mess.com",
                    Role = UserRole.Student,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    RollNumber = "CS2024001",
                    RoomNumber = "A-101",
                    ContactNumber = "+1234567892"
                },

                new User
                {
                    FirstName = "Bob",
                    LastName = "Student",
                    Email = "bob@mess.com",
                    Role = UserRole.Student,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    RollNumber = "CS2024002",
                    RoomNumber = "A-102",
                    ContactNumber = "+1234567893"
                },

                new User
                {
                    FirstName = "Charlie",
                    LastName = "Student",
                    Email = "charlie@mess.com",
                    Role = UserRole.Student,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    RollNumber = "CS2024003",
                    RoomNumber = "B-201",
                    ContactNumber = "+1234567894"
                }
            };

            // Hash passwords for all users
            // Password: Admin@123 for admin
            // Password: Teacher@123 for teacher
            // Password: Student@123 for students
            foreach (var user in users)
            {
                string password = user.Role switch
                {
                    UserRole.Admin => "Admin@123",
                    UserRole.Teacher => "Teacher@123",
                    UserRole.Student => "Student@123",
                    _ => "Default@123"
                };

                user.PasswordHash = passwordHasher.HashPassword(user, password);
            }

            // Add users to database
            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            Console.WriteLine($"Successfully seeded {users.Count} users:");
            Console.WriteLine("  - Admin: admin@mess.com (Admin@123)");
            Console.WriteLine("  - Teacher: teacher@mess.com (Teacher@123)");
            Console.WriteLine("  - Student: alice@mess.com (Student@123)");
            Console.WriteLine("  - Student: bob@mess.com (Student@123)");
            Console.WriteLine("  - Student: charlie@mess.com (Student@123)");
        }
    }
}
