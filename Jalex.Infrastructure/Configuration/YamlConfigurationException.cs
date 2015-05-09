using System;

namespace Jalex.Infrastructure.Configuration
{
    public class YamlConfigurationException : Exception
    {
        public YamlConfigurationException(string message) : base(message)
        {
            
        }
    }
}
