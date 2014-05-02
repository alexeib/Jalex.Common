using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Jalex.Infrastructure.Logging;
using Jalex.Logging.Objects;

namespace Jalex.Logging.Loggers
{
    public class MemoryLogger : ILogger
    {
        private readonly ConcurrentBag<LogMessage> _logs = new ConcurrentBag<LogMessage>();

        public IEnumerable<LogMessage> Logs { get { return _logs; } }

        public void TraceException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logs.Add(new LogMessage(formattedMessage, exception, LogLevel.Trace));
        }

        public void Debug(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogLevel.Debug) : new LogMessage(string.Format(message, args), null, LogLevel.Debug));
        }

        public void DebugException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logs.Add(new LogMessage(formattedMessage, exception, LogLevel.Debug));
        }

        public void WarnException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logs.Add(new LogMessage(formattedMessage, exception, LogLevel.Warn));
        }

        public void Error(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogLevel.Error) : new LogMessage(string.Format(message, args), null, LogLevel.Error));
        }

        public void ErrorException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logs.Add(new LogMessage(formattedMessage, exception, LogLevel.Error));
        }

        public void Fatal(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogLevel.Fatal) : new LogMessage(string.Format(message, args), null, LogLevel.Fatal));
        }

        public void FatalException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logs.Add(new LogMessage(formattedMessage, exception, LogLevel.Fatal));
        }

        public void Info(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogLevel.Info) : new LogMessage(string.Format(message, args), null, LogLevel.Info));
        }

        public void InfoException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logs.Add(new LogMessage(formattedMessage, exception, LogLevel.Info));
        }

        public void Trace(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogLevel.Trace) : new LogMessage(string.Format(message, args), null, LogLevel.Trace));
        }

        public void Warn(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogLevel.Warn) : new LogMessage(string.Format(message, args), null, LogLevel.Warn));
        }

        public void Clear()
        {
            while (!_logs.IsEmpty)
            {
                LogMessage log;
                _logs.TryTake(out log);
            }
        }
    }
}
