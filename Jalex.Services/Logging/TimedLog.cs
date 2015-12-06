using System;
using System.Diagnostics;
using Magnum;
using NLog;

namespace Jalex.Services.Logging
{
    public class TimedLog : IDisposable
    {
        private readonly string _message;
        private readonly ILogger _logger;
        private readonly Stopwatch _timer;

        public static TimedLog Log(ILogger logger, string message)
        {
            return new TimedLog(logger, message);
        }

        public TimedLog(ILogger logger, string message)
        {
            Guard.AgainstNull(logger, "logger");

            _logger = logger;
            _message = message;

            _logger.Info("Starting: {0}", _message);

            _timer = new Stopwatch();
            _timer.Start();
        }

        [DebuggerNonUserCode]
        public void Dispose()
        {
            _timer.Stop();
            _logger.Info("{0}: {1}ms", _message, _timer.Elapsed.TotalMilliseconds);
        }
    }
}
