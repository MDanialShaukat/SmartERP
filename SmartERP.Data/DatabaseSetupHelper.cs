using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SmartERP.Data
{
    /// <summary>
    /// Helper class to prepare database context before use
    /// </summary>
    public static class DatabaseSetupHelper
    {
        /// <summary>
        /// Prepares the database by validating the connection string
        /// </summary>
        public static void PrepareDatabase(IConfiguration configuration, string baseDirectory)
        {
            try
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("DefaultConnection not found in appsettings.json");
                }

                // For SQL Server, we just validate the connection string format
                if (!connectionString.Contains("Server=") && !connectionString.Contains("Data Source="))
                {
                    throw new InvalidOperationException("Invalid SQL Server connection string format. Must contain 'Server=' or 'Data Source='");
                }

                // Connection will be tested when DbContext is first accessed
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database preparation failed: {ex.Message}", ex);
            }
        }
    }
}
