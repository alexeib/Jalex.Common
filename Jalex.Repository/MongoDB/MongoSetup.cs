using Jalex.Infrastructure.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;

namespace Jalex.Repository.MongoDB
{
    public static class MongoSetup
    {
        private static bool _isInitialized;
        private static readonly object _syncRoot = new object();

        public static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                lock (_syncRoot)
                {
                    if (!_isInitialized)
                    {
                        initialize();
                        _isInitialized = true;
                    }
                }
            }
        }

        private static void initialize()
        {
            initializeConventions();
        }

        private static void initializeConventions()
        {
            var applyToAllPack = new ConventionPack
            {
                new IgnoreIfDefaultConvention(true),
                new CamelCaseElementNameConvention()
            };
            ConventionRegistry.Register("Common pack", applyToAllPack, t => true);
        }
    }
}
