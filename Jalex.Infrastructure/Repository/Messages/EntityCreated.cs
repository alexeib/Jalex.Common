namespace Jalex.Infrastructure.Repository.Messages
{
    public class EntityCreated<T>
    {
        public T Entity { get; }

        public EntityCreated(T entity)
        {
            Entity = entity;
        }
    }
}
