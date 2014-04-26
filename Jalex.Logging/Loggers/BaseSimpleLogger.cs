using System;
using Jalex.Logging.Objects;

namespace Jalex.Logging.Loggers
{
    public abstract class BaseSimpleLogger : ILogger
    {
        #region Implementation of ILogger

        public void Trace(string message, params object[] args)
        {
            logMessage(LogLevel.Trace, message, args);
        }

        public void TraceException(Exception exception, string message, params object[] args)
        {
            logExceptionMessage(LogLevel.Trace, exception, message, args);
        }

        public void Debug(string message, params object[] args)
        {
            logMessage(LogLevel.Debug, message, args);
        }

        public void DebugException(Exception exception, string message, params object[] args)
        {
            logExceptionMessage(LogLevel.Debug, exception, message, args);
        }

        public void Info(string message, params object[] args)
        {
            logMessage(LogLevel.Info, message, args);
        }

        public void InfoException(Exception exception, string message, params object[] args)
        {
            logExceptionMessage(LogLevel.Info, exception, message, args);
        }

        public void Warn(string message, params object[] args)
        {
            logMessage(LogLevel.Warn, message, args);
        }

        public void WarnException(Exception exception, string message, params object[] args)
        {
            logExceptionMessage(LogLevel.Warn, exception, message, args);
        }

        public void Error(string message, params object[] args)
        {
            logMessage(LogLevel.Error, message, args);
        }

        public void ErrorException(Exception exception, string message, params object[] args)
        {
            logExceptionMessage(LogLevel.Error, exception, message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            logMessage(LogLevel.Fatal, message, args);
        }

        public void FatalException(Exception exception, string message, params object[] args)
        {
            logExceptionMessage(LogLevel.Fatal, exception, message, args);
        }

        #endregion

        protected abstract void writeLogMessage(string logMessage);

        protected string getLogMessage(LogLevel level, string message, object[] args)
        {
            DateTime currentTime = DateTime.Now;

            var logMessageBody = string.Format(message, args);
            var logMessage = string.Format("{0} - {1}: {2}", currentTime.ToString("O"), level, logMessageBody);
            return logMessage;
        }

        protected void logMessage(LogLevel level, string message, params object[] args)
        {
            var logMessage = getLogMessage(level, message, args);
            writeLogMessage(logMessage);
        }        

        protected void logExceptionMessage(LogLevel level, Exception e, string message, params object[] args)
        {
            var logMessage = getLogMessage(level, message, args);
            var logMessageWithException = string.Format("{0}{1}EXCEPTION: {2}", logMessage, Environment.NewLine, e);
            writeLogMessage(logMessageWithException);
        }        
    }
}
