using Cassandra.Mapping;
using Magnum.Reflection;

namespace Jalex.Repository.Cassandra
{
    public static class CassandraSetup
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
            MappingConfiguration.Global.ConvertTypesUsing(new ComplexTypeConverter());
            FastInvoker<MappingConfiguration>.Current.FastInvoke(MappingConfiguration.Global, "Clear");
        }
    }
}
