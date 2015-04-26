using System;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Configuration
{
    public class ConfigurationMissingException : JalexException
    {
        public ConfigurationMissingException(Type configurationType) 
            : base(String.Format("Configuration type {0} has not been supplied", configurationType.FullName))
        {
        }
    }
}
