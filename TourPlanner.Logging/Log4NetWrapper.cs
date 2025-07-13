using log4net;

namespace TourPlanner.Logging
{
    public class Log4NetLoggerWrapper<TCategory> : ILoggerWrapper<TCategory>
    {
        private static readonly ILog _logger;

        static Log4NetLoggerWrapper()
        {
            _logger = LogManager.GetLogger(typeof(TCategory));
        }

        // Simple messages 
        public void LogDebug(string message) => _logger.Debug(message);
        public void LogInformation(string message) => _logger.Info(message);
        public void LogWarning(string message) => _logger.Warn(message);
        public void LogError(string message) => _logger.Error(message);
        public void LogFatal(string message) => _logger.Fatal(message);

        // Exception + message
        public void LogDebug(Exception ex, string message) => _logger.Debug(message, ex);
        public void LogInformation(Exception ex, string message) => _logger.Info(message, ex);
        public void LogWarning(Exception ex, string message) => _logger.Warn(message, ex);
        public void LogError(Exception ex, string message) => _logger.Error(message, ex);
        public void LogFatal(Exception ex, string message) => _logger.Fatal(message, ex);
    }
}
