using System;
using System.Diagnostics;
using Jalex.Infrastructure.Utils;

namespace Jalex.Logging
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
            ParameterChecker.CheckForVoid(() => logger);

            _logger = logger;
            _message = message;

            _logger.Info(string.Format("Starting: {0}", _message));

            _timer = new Stopwatch();
            _timer.Start();
        }

        [DebuggerNonUserCode]
        public void Dispose()
        {
            _timer.Stop();
            _logger.Info(string.Format("{0}: {1}ms", _message, _timer.Elapsed.TotalMilliseconds));
        }
    }
}
