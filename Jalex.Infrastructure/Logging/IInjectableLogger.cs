namespace Jalex.Infrastructure.Logging
{
    public interface IInjectableLogger
    {
        ILogger Logger { get; set; }
    }
}
