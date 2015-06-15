namespace Jalex.Infrastructure.Repository.Messages
{
    public class EntityUpdated<T>
    {
        public T Entity { get; }

        public EntityUpdated(T entity)
        {
            Entity = entity;
        }
    }
}
