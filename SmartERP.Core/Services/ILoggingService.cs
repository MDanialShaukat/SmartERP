using System;

namespace SmartERP.Core.Services
{
    public interface ILoggingService
    {
        void LogInformation(string message, string? source = null);
        void LogWarning(string message, string? source = null);
        void LogError(string message, Exception? exception = null, string? source = null);
        void LogCritical(string message, Exception? exception = null, string? source = null);
        void LogUserAction(string action, string username, string details);
    }

    public enum LogLevel
    {
        Information,
        Warning,
        Error,
        Critical
    }
}
