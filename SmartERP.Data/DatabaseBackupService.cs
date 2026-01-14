using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SmartERP.Data
{
    public interface IBackupService
    {
        Task<string> CreateBackupAsync();
        Task RestoreBackupAsync(string backupFilePath);
        Task<string[]> GetAvailableBackupsAsync();
        Task CleanupOldBackupsAsync();
    }

    public class DatabaseBackupService : IBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly string _databasePath;
        private readonly string _backupDirectory;
        private readonly int _maxBackupFiles;
        private readonly Action<string>? _logAction;

        public DatabaseBackupService(IConfiguration configuration, Action<string>? logAction = null)
        {
            _configuration = configuration;
            _logAction = logAction;
            
            _databasePath = _configuration.GetConnectionString("DefaultConnection")
                ?.Replace("Data Source=", "") ?? "SmartERP.db";
            
            _backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
            _maxBackupFiles = int.Parse(_configuration["AppSettings:MaxBackupFiles"] ?? "10");

            EnsureBackupDirectoryExists();
        }

        private void Log(string message)
        {
            _logAction?.Invoke(message);
        }

        private void EnsureBackupDirectoryExists()
        {
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
                Log($"Backup directory created: {_backupDirectory}");
            }
        }

        public async Task<string> CreateBackupAsync()
        {
            try
            {
                if (!File.Exists(_databasePath))
                {
                    throw new FileNotFoundException("Database file not found", _databasePath);
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"SmartERP_Backup_{timestamp}.db";
                var backupFilePath = Path.Combine(_backupDirectory, backupFileName);

                Log($"Creating database backup: {backupFileName}");

                // Copy database file
                await Task.Run(() => File.Copy(_databasePath, backupFilePath, true));

                // Cleanup old backups
                await CleanupOldBackupsAsync();

                Log($"Backup created successfully: {backupFilePath}");
                
                return backupFilePath;
            }
            catch (Exception ex)
            {
                Log($"Failed to create backup: {ex.Message}");
                throw;
            }
        }

        public async Task RestoreBackupAsync(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                {
                    throw new FileNotFoundException("Backup file not found", backupFilePath);
                }

                Log($"Restoring database from backup: {backupFilePath}");

                // Create a backup of current database before restore
                var currentBackup = Path.Combine(_backupDirectory, $"BeforeRestore_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                if (File.Exists(_databasePath))
                {
                    await Task.Run(() => File.Copy(_databasePath, currentBackup, true));
                }

                // Restore backup
                await Task.Run(() => File.Copy(backupFilePath, _databasePath, true));

                Log("Database restored successfully");
            }
            catch (Exception ex)
            {
                Log($"Failed to restore backup: {ex.Message}");
                throw;
            }
        }

        public async Task<string[]> GetAvailableBackupsAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    if (!Directory.Exists(_backupDirectory))
                        return Array.Empty<string>();

                    return Directory.GetFiles(_backupDirectory, "SmartERP_Backup_*.db")
                        .OrderByDescending(f => new FileInfo(f).CreationTime)
                        .ToArray();
                });
            }
            catch (Exception ex)
            {
                Log($"Failed to get available backups: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        public async Task CleanupOldBackupsAsync()
        {
            try
            {
                var backups = await GetAvailableBackupsAsync();
                
                if (backups.Length > _maxBackupFiles)
                {
                    var backupsToDelete = backups.Skip(_maxBackupFiles);
                    
                    foreach (var backup in backupsToDelete)
                    {
                        await Task.Run(() => File.Delete(backup));
                        Log($"Deleted old backup: {Path.GetFileName(backup)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to cleanup old backups: {ex.Message}");
            }
        }
    }
}
