using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartERP.Core.Services;
using SmartERP.Data;

namespace SmartERP.Core.Services
{
    /// <summary>
    /// Service for automatically applying database migrations
    /// </summary>
    public interface IDatabaseMigrationService
    {
        Task<bool> ApplyPendingMigrationsAsync();
        Task<List<string>> GetPendingMigrationsAsync();
        Task<List<string>> GetAppliedMigrationsAsync();
    }

    /// <summary>
    /// Implementation of database migration service
    /// </summary>
    public class DatabaseMigrationService : IDatabaseMigrationService
    {
        private readonly SmartERPDbContext _dbContext;
        private readonly ILoggingService _loggingService;

        public DatabaseMigrationService(SmartERPDbContext dbContext, ILoggingService loggingService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        /// <summary>
        /// Applies all pending migrations to the database
        /// </summary>
        /// <returns>True if migrations were applied successfully, false otherwise</returns>
        public async Task<bool> ApplyPendingMigrationsAsync()
        {
            try
            {
                _loggingService.LogInformation("=== DATABASE MIGRATION START ===", "MigrationService");

                // Get pending migrations
                var pendingMigrations = await GetPendingMigrationsAsync();

                if (!pendingMigrations.Any())
                {
                    _loggingService.LogInformation("No pending migrations found. Database is up to date.", "MigrationService");
                    return true;
                }

                _loggingService.LogInformation($"Found {pendingMigrations.Count} pending migration(s):", "MigrationService");
                foreach (var migration in pendingMigrations)
                {
                    _loggingService.LogInformation($"  - {migration}", "MigrationService");
                }

                // Apply all pending migrations
                _loggingService.LogInformation("Applying migrations to database...", "MigrationService");
                await _dbContext.Database.MigrateAsync();

                _loggingService.LogInformation("? All migrations applied successfully", "MigrationService");
                
                // Log applied migrations
                var appliedMigrations = await GetAppliedMigrationsAsync();
                _loggingService.LogInformation($"Total applied migrations: {appliedMigrations.Count}", "MigrationService");

                _loggingService.LogInformation("=== DATABASE MIGRATION SUCCESS ===", "MigrationService");
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogCritical("Database migration failed", ex, "MigrationService");
                _loggingService.LogCritical($"Error Type: {ex.GetType().FullName}", null, "MigrationService");
                _loggingService.LogCritical($"Error Message: {ex.Message}", null, "MigrationService");
                _loggingService.LogCritical($"Stack Trace: {ex.StackTrace}", null, "MigrationService");

                return false;
            }
        }

        /// <summary>
        /// Gets list of pending migrations that haven't been applied yet
        /// </summary>
        public async Task<List<string>> GetPendingMigrationsAsync()
        {
            try
            {
                var pendingMigrations = (await _dbContext.Database.GetPendingMigrationsAsync()).ToList();
                return pendingMigrations;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to get pending migrations", ex, "MigrationService");
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets list of migrations that have already been applied
        /// </summary>
        public async Task<List<string>> GetAppliedMigrationsAsync()
        {
            try
            {
                var appliedMigrations = (await _dbContext.Database.GetAppliedMigrationsAsync()).ToList();
                return appliedMigrations;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to get applied migrations", ex, "MigrationService");
                return new List<string>();
            }
        }
    }
}
