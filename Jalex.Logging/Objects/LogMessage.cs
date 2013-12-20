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

        public LogMessage(string message, Exception exception, Level logLevel)
        {
            Message = message;
            Exception = exception;
            LogLevel = logLevel;
        }

        #endregion

        #region Enums

        public enum Level
        {
            Trace = 0,

            Debug = 1,

            Info = 2,

            Warn = 3,

            Error = 4,

            Fatal = 5,

            Off = 6,
        }

        #endregion

        #region Public Properties

        public Exception Exception { get; private set; }

        public Level LogLevel { get; private set; }

        public string Message { get; private set; }

        #endregion
    }
}
