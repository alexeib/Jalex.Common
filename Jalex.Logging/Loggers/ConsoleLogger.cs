using System;

namespace Jalex.Logging.Loggers
{
    public class ConsoleLogger : BaseSimpleLogger
    {
        #region Overrides of BaseSimpleLogger

        protected override void writeLogMessage(string logMessage)
        {
            Console.WriteLine(logMessage);
        }

        #endregion
    }
}
