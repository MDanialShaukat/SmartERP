using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SmartERP.Data
{
    public class SmartERPDbContextFactory : IDesignTimeDbContextFactory<SmartERPDbContext>
    {
        public SmartERPDbContext CreateDbContext(string[] args)
        {
            // Try to find appsettings.json in multiple locations
            var basePath = FindAppSettingsDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Get connection string from configuration
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found in appsettings.json. " +
                    "Please ensure appsettings.json is configured correctly with a DefaultConnection entry.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<SmartERPDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new SmartERPDbContext(optionsBuilder.Options);
        }

        private string FindAppSettingsDirectory()
        {
            // Start from current directory
            var currentPath = Directory.GetCurrentDirectory();

            // Search for appsettings.json in the current directory and parent directories
            var searchPath = currentPath;
            var maxLevels = 10;
            var levelsChecked = 0;

            while (levelsChecked < maxLevels)
            {
                var appSettingsPath = Path.Combine(searchPath, "appsettings.json");
                if (File.Exists(appSettingsPath))
                {
                    return searchPath;
                }

                // Check parent directory
                var parentPath = Path.GetDirectoryName(searchPath);
                if (parentPath == null || parentPath == searchPath)
                {
                    break; // Reached root directory
                }

                searchPath = parentPath;
                levelsChecked++;
            }

            // If not found, try to construct path based on known structure
            // Assume we're running from SmartERP.Data or SmartERP.UI directory
            var workspacePath = FindSmartERPWorkspaceRoot(currentPath);
            var uiPath = Path.Combine(workspacePath, "SmartERP.UI");
            
            if (Directory.Exists(uiPath) && File.Exists(Path.Combine(uiPath, "appsettings.json")))
            {
                return uiPath;
            }

            // Default fallback - will fail with clear error message
            throw new InvalidOperationException(
                $"Unable to find appsettings.json in any parent directory starting from: {currentPath}\n" +
                $"Searched up to 10 levels.\n" +
                $"Please ensure appsettings.json exists in the SmartERP.UI directory.");
        }

        private string FindSmartERPWorkspaceRoot(string startPath)
        {
            var currentPath = startPath;

            for (int i = 0; i < 10; i++)
            {
                // Check if this directory contains SmartERP.sln or SmartERP.UI folder
                if (File.Exists(Path.Combine(currentPath, "SmartERP.sln")) ||
                    Directory.Exists(Path.Combine(currentPath, "SmartERP.UI")))
                {
                    return currentPath;
                }

                var parentPath = Path.GetDirectoryName(currentPath);
                if (parentPath == null || parentPath == currentPath)
                {
                    break;
                }

                currentPath = parentPath;
            }

            // If we can't find workspace root, return the current path
            return startPath;
        }
    }
}
