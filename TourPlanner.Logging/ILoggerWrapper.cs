namespace TourPlanner.Logging
{
    public interface ILoggerWrapper<TCategory>
    {
        // Simple message
        void LogDebug(string message);
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogFatal(string message);

        // Exception + message
        void LogDebug(Exception ex, string message);
        void LogInformation(Exception ex, string message);
        void LogWarning(Exception ex, string message);
        void LogError(Exception ex, string message);
        void LogFatal(Exception ex, string message);
    }
}