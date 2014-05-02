namespace Jalex.Infrastructure.Repository
{
    public interface IQueryableRepository<T> : ISimpleRepository<T>, IQueryable<T>
    {
    }
}
