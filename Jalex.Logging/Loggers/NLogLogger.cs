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

        public void TraceException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logger.TraceException(formattedMessage, exception);
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

        public void DebugException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logger.DebugException(formattedMessage, exception);
        }

        public void WarnException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logger.WarnException(formattedMessage, exception);
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

        public void ErrorException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logger.ErrorException(formattedMessage, exception);
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

        public void FatalException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logger.FatalException(formattedMessage, exception);
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

        public void InfoException(Exception exception, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logger.InfoException(formattedMessage, exception);
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
    }
}
