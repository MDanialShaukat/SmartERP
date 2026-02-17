using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SmartERP.Data
{
    public class DatabaseInitializer
    {
        private readonly SmartERPDbContext _context;

        public DatabaseInitializer(SmartERPDbContext context)
        {
            _context = context;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Use only Migrate: creates DB if needed and applies all migrations in order.
                // Do NOT use EnsureCreated - it would create tables that migrations also create, causing "object already exists" errors.
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize database: {ex.GetType().Name} - {ex.Message}", ex);
            }
        }

        public async Task SeedDataAsync()
        {
            try
            {
                // Check if data already exists
                if (await _context.Users.AnyAsync())
                    return;

                // Seed default admin user
                var adminExists = await _context.Users.AnyAsync(u => u.Username == "admin");
                
                if (!adminExists)
                {
                    var admin = new Models.Entities.User
                    {
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        Role = "Admin",
                        FullName = "System Administrator",
                        Email = "admin@smarterp.local",
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };

                    _context.Users.Add(admin);
                }

                // Seed default employee user
                var employeeExists = await _context.Users.AnyAsync(u => u.Username == "employee");
                
                if (!employeeExists)
                {
                    var employee = new Models.Entities.User
                    {
                        Username = "employee",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"),
                        Role = "User",
                        FullName = "Employee User",
                        Email = "employee@smarterp.local",
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };

                    _context.Users.Add(employee);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to seed database with default users: {ex.GetType().Name} - {ex.Message}", ex);
            }
        }
    }
}
