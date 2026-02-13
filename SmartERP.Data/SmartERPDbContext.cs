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
        public DbSet<InventoryAssignment> InventoryAssignments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            
            // Suppress the PendingModelChangesWarning since we're using EnsureCreatedAsync
            // which creates schema directly from the model, not migrations
            optionsBuilder.ConfigureWarnings(w => 
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

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
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired(false);

                entity.HasOne(e => e.LastModifiedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.LastModifiedBy)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired(false);
            });

            // Configure Inventory entity
            modelBuilder.Entity<Inventory>(entity =>
            {
                // Configure audit relationships
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedBy)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired(false);

                entity.HasOne(e => e.LastModifiedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.LastModifiedBy)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired(false);
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
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired(false);

                entity.HasOne(e => e.LastModifiedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.LastModifiedBy)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired(false);
            });

            // Configure InventoryAssignment entity
            modelBuilder.Entity<InventoryAssignment>(entity =>
            {
                entity.HasOne(ia => ia.Inventory)
                      .WithMany()
                      .HasForeignKey(ia => ia.InventoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ia => ia.AssignedToUser)
                      .WithMany()
                      .HasForeignKey(ia => ia.AssignedToUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ia => ia.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(ia => ia.CreatedBy)
                      .OnDelete(DeleteBehavior.NoAction)
                      .IsRequired(false);
            });

            // Note: Do NOT seed data in OnModelCreating - it causes initialization errors
            // Seeding will happen in DatabaseInitializer.SeedDataAsync() instead
        }
    }
}
