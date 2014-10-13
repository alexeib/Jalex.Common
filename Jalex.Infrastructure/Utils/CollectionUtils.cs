using System.Collections.Generic;
using System.Linq;

namespace Jalex.Infrastructure.Utils
{
    public static class CollectionUtils
    {
        public static IEnumerable<T> LockAndCreateNewCollectionWithItemAppended<T>(T itemToAdd, IEnumerable<T> collection, object lockObject)
        {
            Guard.AgainstNull(collection, "collection");
            Guard.AgainstNull(lockObject, "lockObject");

            lock (lockObject)
            {
                var oldItems = collection as T[] ?? collection.ToArray();
                var newItems = new T[oldItems.Length + 1];
                oldItems.CopyTo(newItems, 0);
                newItems[oldItems.Length] = itemToAdd;
                return newItems;
            }
        }
    }
}
