namespace Jalex.Infrastructure.Configuration
{
    public interface IConfigurationProvider
    {
        TConfiguration GetConfiguration<TConfiguration>() where TConfiguration : class, IConfiguration;
    }
}
