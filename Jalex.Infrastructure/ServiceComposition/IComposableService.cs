namespace Jalex.Infrastructure.ServiceComposition
{
    public interface IComposableService<in T, out TRet>
    {
        bool CanProcess(T item);
        TRet Process(T item);
    }
}