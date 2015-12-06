namespace Jalex.Infrastructure.Repository
{
    public interface ISimpleRepository<T> : IReader<T>, IDeleter<T>, IWriter<T>
        where T: class
    {
    }
}
