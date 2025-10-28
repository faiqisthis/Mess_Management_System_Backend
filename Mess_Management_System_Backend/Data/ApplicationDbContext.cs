using Mess_Management_System_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Mess_Management_System_Backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        // Add more DbSets as needed
    }
}
