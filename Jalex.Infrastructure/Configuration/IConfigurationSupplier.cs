using System.Collections.Generic;

namespace Jalex.Infrastructure.Configuration
{
    public interface IConfigurationSupplier
    {
        IEnumerable<IConfiguration> GetConfigurations();
    }
}
