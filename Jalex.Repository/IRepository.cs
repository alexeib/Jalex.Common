namespace Jalex.Repository
{
    public interface IRepository<T> : IReader<T>, IDeleter<T>, IUpdater<T>, IInserter<T>
    {       

    }
}
