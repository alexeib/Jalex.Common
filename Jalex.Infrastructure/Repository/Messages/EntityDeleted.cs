namespace Jalex.Infrastructure.Repository.Messages
{
    public class EntityDeleted<T>
    {
        public T Entity { get; }

        public EntityDeleted(T entity)
        {
            Entity = entity;
        }
    }
}
