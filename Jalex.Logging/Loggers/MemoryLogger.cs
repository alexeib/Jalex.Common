using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Jalex.Logging.Objects;

namespace Jalex.Logging.Loggers
{
    public class MemoryLogger : ILogger
    {
        private readonly ConcurrentBag<LogMessage> _logs = new ConcurrentBag<LogMessage>();

        public IEnumerable<LogMessage> Logs { get { return _logs; } }

        public void Debug(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogMessage.Level.Debug) : new LogMessage(string.Format(message, args), null, LogMessage.Level.Debug));
        }

        public void DebugException(string message, Exception exception)
        {
            _logs.Add(new LogMessage(message, exception, LogMessage.Level.Debug));
        }

        public void Error(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogMessage.Level.Error) : new LogMessage(string.Format(message, args), null, LogMessage.Level.Error));
        }

        public void ErrorException(string message, Exception exception)
        {
            _logs.Add(new LogMessage(message, exception, LogMessage.Level.Error));
        }

        public void Fatal(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogMessage.Level.Fatal) : new LogMessage(string.Format(message, args), null, LogMessage.Level.Fatal));
        }

        public void FatalException(string message, Exception exception)
        {
            _logs.Add(new LogMessage(message, exception, LogMessage.Level.Fatal));
        }

        public void Info(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogMessage.Level.Info) : new LogMessage(string.Format(message, args), null, LogMessage.Level.Info));
        }

        public void InfoException(string message, Exception exception)
        {
            _logs.Add(new LogMessage(message, exception, LogMessage.Level.Info));
        }

        public void Trace(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogMessage.Level.Trace) : new LogMessage(string.Format(message, args), null, LogMessage.Level.Trace));
        }

        public void TraceException(string message, Exception exception)
        {
            _logs.Add(new LogMessage(message, exception, LogMessage.Level.Trace));
        }

        public void Warn(string message, params object[] args)
        {
            _logs.Add(args == null ? new LogMessage(message, null, LogMessage.Level.Warn) : new LogMessage(string.Format(message, args), null, LogMessage.Level.Warn));
        }

        public void WarnException(string message, Exception exception)
        {
            _logs.Add(new LogMessage(message, exception, LogMessage.Level.Warn));
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
