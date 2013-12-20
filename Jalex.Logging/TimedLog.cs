using System;
using System.Diagnostics;

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
            _logger = logger;
            _message = message;
            _timer = new Stopwatch();
            _timer.Start();
        }

        [DebuggerNonUserCode]
        public void Dispose()
        {
            _timer.Stop();
            if (_logger != null)
            {
                _logger.Info(string.Format("{0} [Time taken: {1}]", _message, _timer.Elapsed));
            }
        }
    }
}
