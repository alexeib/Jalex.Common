namespace Jalex.Infrastructure.Logging
{
    public static class LoggerExtensions
    {
        public static void Trace(this ILogger logger, string message, params object[] args)
        {
            logger.Trace(string.Format(message, args));
        }

        public static void Debug(this ILogger logger, string message, params object[] args)
        {
            logger.Debug(string.Format(message, args));
        }

        public static void Info(this ILogger logger, string message, params object[] args)
        {
            logger.Info(string.Format(message, args));
        }

        public static void Warn(this ILogger logger, string message, params object[] args)
        {
            logger.Warn(string.Format(message, args));
        }

        public static void Error(this ILogger logger, string message, params object[] args)
        {
            logger.Error(string.Format(message, args));
        }

        public static void Fatal(this ILogger logger, string message, params object[] args)
        {
            logger.Fatal(string.Format(message, args));
        }
    }
}
