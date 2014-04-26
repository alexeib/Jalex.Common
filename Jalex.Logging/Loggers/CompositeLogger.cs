using System;
using System.Collections.Generic;
using System.Linq;
using Jalex.Infrastructure.Utils;

namespace Jalex.Logging.Loggers
{
    public class CompositeLogger : ILogger
    {
        private readonly ILogger[] _loggers;

        public CompositeLogger(IEnumerable<ILogger> loggers)
        {
            ParameterChecker.CheckForVoid(() => loggers);
            _loggers = loggers as ILogger[] ?? loggers.ToArray();
        }

        #region Implementation of ILogger

        public void Trace(string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.Trace(message, args);
            }
        }

        public void TraceException(Exception exception, string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.TraceException(exception, message, args);
            }
        }

        public void Debug(string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.Debug(message, args);
            }
        }

        public void DebugException(Exception exception, string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.DebugException(exception, message, args);
            }
        }

        public void Info(string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.Info(message, args);
            }
        }

        public void InfoException(Exception exception, string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.InfoException(exception, message, args);
            }
        }

        public void Warn(string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.Warn(message, args);
            }
        }

        public void WarnException(Exception exception, string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.WarnException(exception, message, args);
            }
        }

        public void Error(string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.Error(message, args);
            }
        }

        public void ErrorException(Exception exception, string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.ErrorException(exception, message, args);
            }
        }

        public void Fatal(string message, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void FatalException(Exception exception, string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.FatalException(exception, message, args);
            }
        }

        #endregion
    }
}
