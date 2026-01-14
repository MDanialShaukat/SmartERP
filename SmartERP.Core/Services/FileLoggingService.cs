using System;
using System.IO;
using System.Text;

namespace SmartERP.Core.Services
{
    public class FileLoggingService : ILoggingService
    {
        private readonly string _logDirectory;
        private readonly object _lockObject = new object();

        public FileLoggingService(string logDirectory = "Logs")
        {
            _logDirectory = logDirectory;
            EnsureLogDirectoryExists();
        }

        private void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void LogInformation(string message, string? source = null)
        {
            WriteLog(LogLevel.Information, message, null, source);
        }

        public void LogWarning(string message, string? source = null)
        {
            WriteLog(LogLevel.Warning, message, null, source);
        }

        public void LogError(string message, Exception? exception = null, string? source = null)
        {
            WriteLog(LogLevel.Error, message, exception, source);
        }

        public void LogCritical(string message, Exception? exception = null, string? source = null)
        {
            WriteLog(LogLevel.Critical, message, exception, source);
        }

        public void LogUserAction(string action, string username, string details)
        {
            var message = $"User: {username} | Action: {action} | Details: {details}";
            WriteLog(LogLevel.Information, message, null, "UserAction");
        }

        private void WriteLog(LogLevel level, string message, Exception? exception, string? source)
        {
            try
            {
                lock (_lockObject)
                {
                    var logFileName = $"log_{DateTime.Now:yyyyMMdd}.txt";
                    var logFilePath = Path.Combine(_logDirectory, logFileName);

                    var logEntry = new StringBuilder();
                    logEntry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {(source != null ? $"[{source}] " : "")}");
                    logEntry.AppendLine($"Message: {message}");

                    if (exception != null)
                    {
                        logEntry.AppendLine($"Exception: {exception.GetType().Name}");
                        logEntry.AppendLine($"Exception Message: {exception.Message}");
                        logEntry.AppendLine($"Stack Trace: {exception.StackTrace}");
                        
                        if (exception.InnerException != null)
                        {
                            logEntry.AppendLine($"Inner Exception: {exception.InnerException.Message}");
                        }
                    }

                    logEntry.AppendLine(new string('-', 100));

                    File.AppendAllText(logFilePath, logEntry.ToString());

                    // Also keep an error-only log
                    if (level == LogLevel.Error || level == LogLevel.Critical)
                    {
                        var errorLogFileName = $"error_{DateTime.Now:yyyyMMdd}.txt";
                        var errorLogFilePath = Path.Combine(_logDirectory, errorLogFileName);
                        File.AppendAllText(errorLogFilePath, logEntry.ToString());
                    }

                    // Keep user action log separate
                    if (source == "UserAction")
                    {
                        var userActionLogFileName = $"useraction_{DateTime.Now:yyyyMMdd}.txt";
                        var userActionLogFilePath = Path.Combine(_logDirectory, userActionLogFileName);
                        File.AppendAllText(userActionLogFilePath, logEntry.ToString());
                    }
                }
            }
            catch
            {
                // Logging should never crash the application
                // Optionally write to Windows Event Log as fallback
            }
        }
    }
}
