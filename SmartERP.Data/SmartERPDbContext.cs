using Microsoft.EntityFrameworkCore;
using SmartERP.Models.Entities;

namespace SmartERP.Data
{
    public class SmartERPDbContext : DbContext
    {
        public SmartERPDbContext(DbContextOptions<SmartERPDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Billing> Billings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Role).HasDefaultValue("User");
            });

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.CustomerCode).IsUnique();

                // Configure audit relationships
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LastModifiedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.LastModifiedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Inventory entity
            modelBuilder.Entity<Inventory>(entity =>
            {
                // Configure audit relationships
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LastModifiedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.LastModifiedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Billing entity
            modelBuilder.Entity<Billing>(entity =>
            {
                entity.HasOne(b => b.Customer)
                      .WithMany()
                      .HasForeignKey(b => b.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.BillNumber).IsUnique();

                // Configure audit relationships
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.LastModifiedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.LastModifiedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed default Admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Default password
                    Role = "Admin",
                    FullName = "System Administrator",
                    Email = "admin@smarterp.local",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );
        }
    }
}
