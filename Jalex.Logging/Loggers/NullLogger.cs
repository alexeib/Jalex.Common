using System;

namespace Jalex.Logging.Loggers
{
    public class NullLogger : ILogger
    {
        public void TraceException(Exception exception, string message, params object[] args)
        {
        }

        public void Debug(string message, params object[] args)
        {
        }

        public void DebugException(Exception exception, string message, params object[] args)
        {
        }

        public void WarnException(Exception exception, string message, params object[] args)
        {
        }

        public void Error(string message, params object[] args)
        {
        }

        public void ErrorException(Exception exception, string message, params object[] args)
        {
        }

        public void Fatal(string message, params object[] args)
        {
        }

        public void FatalException(Exception exception, string message, params object[] args)
        {
        }

        public void Info(string message, params object[] args)
        {
        }

        public void InfoException(Exception exception, string message, params object[] args)
        {
        }

        public void Trace(string message, params object[] args)
        {
        }

        public void Warn(string message, params object[] args)
        {
        }
    }
}
