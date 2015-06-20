namespace Jalex.Infrastructure.Repository.Messages
{
    public class EntityUpdated<T>
    {
        public T OldEntity { get; }

        public T NewEntity { get; }

        public EntityUpdated(T oldEntity, T newEntity)
        {
            OldEntity = oldEntity;
            NewEntity = newEntity;
        }
    }
}
