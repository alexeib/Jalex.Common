namespace Jalex.Repository
{
    public interface ISimpleRepository<T> : IReader<T>, IDeleter<T>, IUpdater<T>, IInserter<T>
    {       

    }
}
