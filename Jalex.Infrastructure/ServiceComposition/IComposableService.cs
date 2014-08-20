namespace Jalex.Infrastructure.ServiceComposition
{
    public interface IComposableService<in T>
    {
        bool CanProcess(T item);
        void Process(T item);
    }
}