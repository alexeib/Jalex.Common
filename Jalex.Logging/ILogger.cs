using System;

namespace Jalex.Logging
{
    public interface ILogger
    {
        void Trace(string message, params object[] args);
        void TraceException(Exception exception, string message, params object[] args);
        void Debug(string message, params object[] args);
        void DebugException(Exception exception, string message, params object[] args);
        void Info(string message, params object[] args);
        void InfoException(Exception exception, string message, params object[] args);
        void Warn(string message, params object[] args);
        void WarnException(Exception exception, string message, params object[] args);
        void Error(string message, params object[] args);
        void ErrorException(Exception exception, string message, params object[] args);
        void Fatal(string message, params object[] args);
        void FatalException(Exception exception, string message, params object[] args);
    }
}