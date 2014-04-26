namespace Jalex.Logging.Loggers
{
    public class DebugLogger : BaseSimpleLogger
    {
        #region Overrides of BaseSimpleLogger

        protected override void writeLogMessage(string logMessage)
        {
            System.Diagnostics.Debug.WriteLine(logMessage);
        }

        #endregion
    }
}
