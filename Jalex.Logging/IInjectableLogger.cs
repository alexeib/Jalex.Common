namespace Jalex.Logging
{
    public interface IInjectableLogger
    {
        ILogger Logger { get; set; }
    }
}
