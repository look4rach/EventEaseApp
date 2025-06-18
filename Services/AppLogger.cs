using Microsoft.Extensions.Logging;
// This namespace contains the AppLogger class which implements IAppLogger interface.
namespace EventEaseApp
{
    public interface IAppLogger
    {
        void LogNavigation(string path);
        void LogError(string message, Exception? exception = null);
    }

    public class AppLogger : IAppLogger
    {
        private readonly ILogger<AppLogger> _logger;

        public AppLogger(ILogger<AppLogger> logger)
        {
            _logger = logger;
        }

        public void LogNavigation(string path)
        {
            _logger.LogInformation("Navigation to: {Path}", path);
        }

        public void LogError(string message, Exception? exception = null)
        {
            if (exception != null)
            {
                _logger.LogError(exception, "{Message}", message);
            }
            else
            {
                _logger.LogError("{Message}", message);
            }
        }
    }
}
