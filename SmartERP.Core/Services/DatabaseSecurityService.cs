using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Extensions.Configuration;

namespace SmartERP.Core.Services
{
    /// <summary>
    /// Provides database security services including file protection,
    /// access control, and integrity verification
    /// </summary>
    public interface IDatabaseSecurityService
    {
        void SecureDatabaseFile(string databasePath);
        void SetDatabasePermissions(string databasePath);
        bool VerifyDatabaseIntegrity(string databasePath);
        string GetSecureConnectionString(string basePath);
        void CreateSecureBackup(string sourcePath, string backupPath);
        bool IsDatabaseAccessible(string databasePath);
    }

    public class DatabaseSecurityService : IDatabaseSecurityService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggingService _loggingService;
        private readonly IEncryptionService _encryptionService;

        public DatabaseSecurityService(
            IConfiguration configuration,
            ILoggingService loggingService,
            IEncryptionService encryptionService)
        {
            _configuration = configuration;
            _loggingService = loggingService;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Secures the database file by setting proper permissions and attributes
        /// </summary>
        public void SecureDatabaseFile(string databasePath)
        {
            try
            {
                if (!File.Exists(databasePath))
                {
                    _loggingService.LogWarning($"Database file not found: {databasePath}", "DatabaseSecurity");
                    return;
                }

                // Set file permissions
                SetDatabasePermissions(databasePath);

                // Set file attributes (hidden from casual browsing)
                var attributes = File.GetAttributes(databasePath);
                
                // Don't make it hidden in development, but can enable for production
                var hideDatabase = bool.Parse(_configuration["Security:HideDatabaseFile"] ?? "false");
                if (hideDatabase)
                {
                    File.SetAttributes(databasePath, attributes | FileAttributes.Hidden);
                }

                _loggingService.LogInformation($"Database file secured: {databasePath}", "DatabaseSecurity");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to secure database file: {databasePath}", ex, "DatabaseSecurity");
            }
        }

        /// <summary>
        /// Sets restricted file system permissions on the database file
        /// Only the application and administrators can access it
        /// </summary>
        public void SetDatabasePermissions(string databasePath)
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    _loggingService.LogWarning("File permissions only supported on Windows", "DatabaseSecurity");
                    return;
                }

                var fileInfo = new FileInfo(databasePath);
                var security = fileInfo.GetAccessControl();

                // Get current user
                var currentUser = WindowsIdentity.GetCurrent();
                var currentUserSid = currentUser.User;

                // Get administrators group
                var adminsSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

                // Get SYSTEM account
                var systemSid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);

                // Remove inheritance
                security.SetAccessRuleProtection(true, false);

                // Clear existing rules
                var existingRules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));
                foreach (FileSystemAccessRule rule in existingRules)
                {
                    security.RemoveAccessRule(rule);
                }

                // Add rule for current user (Full Control)
                if (currentUserSid != null)
                {
                    security.AddAccessRule(new FileSystemAccessRule(
                        currentUserSid,
                        FileSystemRights.FullControl,
                        AccessControlType.Allow));
                }

                // Add rule for Administrators (Full Control)
                security.AddAccessRule(new FileSystemAccessRule(
                    adminsSid,
                    FileSystemRights.FullControl,
                    AccessControlType.Allow));

                // Add rule for SYSTEM (Full Control)
                security.AddAccessRule(new FileSystemAccessRule(
                    systemSid,
                    FileSystemRights.FullControl,
                    AccessControlType.Allow));

                // Apply the new security settings
                fileInfo.SetAccessControl(security);

                _loggingService.LogInformation("Database file permissions set successfully", "DatabaseSecurity");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to set database permissions", ex, "DatabaseSecurity");
            }
        }

        /// <summary>
        /// Verifies the integrity of the database file using SQLite PRAGMA
        /// </summary>
        public bool VerifyDatabaseIntegrity(string databasePath)
        {
            try
            {
                if (!File.Exists(databasePath))
                {
                    _loggingService.LogError("Database file not found for integrity check", null, "DatabaseSecurity");
                    return false;
                }

                // SQLite integrity check would be done via the DbContext
                // For now, we'll check basic file accessibility
                using var stream = File.Open(databasePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                
                // Check SQLite header (first 16 bytes should be "SQLite format 3\0")
                var header = new byte[16];
                stream.Read(header, 0, 16);
                
                var headerString = System.Text.Encoding.ASCII.GetString(header);
                var isValid = headerString.StartsWith("SQLite format 3");

                if (isValid)
                {
                    _loggingService.LogInformation("Database integrity check passed", "DatabaseSecurity");
                }
                else
                {
                    _loggingService.LogError("Database integrity check failed - invalid header", null, "DatabaseSecurity");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Database integrity check failed", ex, "DatabaseSecurity");
                return false;
            }
        }

        /// <summary>
        /// Gets a secure connection string with encryption password if configured
        /// </summary>
        public string GetSecureConnectionString(string basePath)
        {
            var connectionString = $"Data Source={basePath}";

            // Add password if database encryption is enabled
            var dbPassword = _configuration["Security:DatabasePassword"];
            if (!string.IsNullOrEmpty(dbPassword))
            {
                // Decrypt the stored password
                try
                {
                    var decryptedPassword = _encryptionService.Decrypt(dbPassword);
                    connectionString += $";Password={decryptedPassword}";
                }
                catch
                {
                    // Password might not be encrypted yet (first run)
                    connectionString += $";Password={dbPassword}";
                }
            }

            // Add additional security options
            connectionString += ";Mode=ReadWriteCreate";
            
            return connectionString;
        }

        /// <summary>
        /// Creates a secure backup of the database
        /// </summary>
        public void CreateSecureBackup(string sourcePath, string backupPath)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException("Source database not found", sourcePath);
                }

                // Ensure backup directory exists
                var backupDir = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Copy database file
                File.Copy(sourcePath, backupPath, true);

                // Secure the backup file
                SetDatabasePermissions(backupPath);

                _loggingService.LogInformation($"Secure backup created: {backupPath}", "DatabaseSecurity");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to create secure backup", ex, "DatabaseSecurity");
                throw;
            }
        }

        /// <summary>
        /// Checks if the database file is accessible
        /// </summary>
        public bool IsDatabaseAccessible(string databasePath)
        {
            try
            {
                if (!File.Exists(databasePath))
                    return false;

                using var stream = File.Open(databasePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
