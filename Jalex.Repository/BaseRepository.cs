using Jalex.Logging;

namespace Jalex.Repository
{
    public abstract class BaseRepository : IInjectableLogger
    {
        protected static readonly ILogger _staticLogger = LogManager.GetCurrentClassLogger();
        protected ILogger _instanceLogger;

        public ILogger Logger
        {
            get { return _instanceLogger ?? _staticLogger; }
            set { _instanceLogger = value; }
        }
    }
}
