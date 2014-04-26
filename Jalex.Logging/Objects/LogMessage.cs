using System;

namespace Jalex.Logging.Objects
{
    /// <summary>
    /// Log message entry.
    /// NOTE: This class should stay immutable.
    /// </summary>
    public class LogMessage
    {
        #region Constructors and Destructors

        public LogMessage(string message, Exception exception, LogLevel logLogLevel)
        {
            Message = message;
            Exception = exception;
            LogLogLevel = logLogLevel;
        }

        #endregion

        #region Public Properties

        public Exception Exception { get; private set; }

        public LogLevel LogLogLevel { get; private set; }

        public string Message { get; private set; }

        #endregion
    }
}
