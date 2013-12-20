using System;

namespace Jalex.Logging.Loggers
{
    public class NLogLogger : ILogger
    {
        private readonly NLog.Logger _logger;

        public NLogLogger(Type type)
        {
            _logger = NLog.LogManager.GetLogger(type.FullName);
        }

        public void Debug(string message, params object[] args)
        {
            if (args == null)
            {
                _logger.Debug(message);
            }
            else
            {
                _logger.Debug(message, args);
            }
        }

        public void DebugException(string message, Exception exception)
        {
            _logger.DebugException(message, exception);
        }

        public void Error(string message, params object[] args)
        {
            if (args == null)
            {
                _logger.Error(message);
            }
            else
            {
                _logger.Error(message, args);
            }
        }

        public void ErrorException(string message, Exception exception)
        {
            _logger.ErrorException(message, exception);
        }

        public void Fatal(string message, params object[] args)
        {
            if (args == null)
            {
                _logger.Fatal(message);
            }
            else
            {
                _logger.Fatal(message, args);
            }
        }

        public void FatalException(string message, Exception exception)
        {
            _logger.FatalException(message, exception);
        }

        public void Info(string message, params object[] args)
        {
            if (args == null)
            {
                _logger.Info(message);
            }
            else
            {
                _logger.Info(message, args);
            }
        }

        public void InfoException(string message, Exception exception)
        {
            _logger.InfoException(message, exception);
        }

        public void Trace(string message, params object[] args)
        {
            if (args == null)
            {
                _logger.Trace(message);
            }
            else
            {
                _logger.Trace(message, args);
            }
        }

        public void TraceException(string message, Exception exception)
        {
            _logger.TraceException(message, exception);
        }

        public void Warn(string message, params object[] args)
        {
            if (args == null)
            {
                _logger.Warn(message);
            }
            else
            {
                _logger.Warn(message, args);
            }
        }

        public void WarnException(string message, Exception exception)
        {
            _logger.WarnException(message, exception);
        }
    }
}
