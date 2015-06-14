using System;
using System.Collections.Generic;

namespace Jalex.Infrastructure.Containers
{
    /// <summary>
    /// A collection of weak references to objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of objects to hold weak references to.</typeparam>
    public interface IWeakCollection<T> : ICollection<T>, IDisposable where T : class
    {
        /// <summary>
        /// Gets a sequence of live objects from the collection, causing a purge.
        /// </summary>
        IEnumerable<T> LiveList { get; }

        /// <summary>
        /// Gets a complete sequence of objects from the collection. Does not cause a purge. Null entries represent dead objects.
        /// </summary>
        IEnumerable<T> CompleteList { get; }

        /// <summary>
        /// Gets a sequence of live objects from the collection without causing a purge.
        /// </summary>
        IEnumerable<T> LiveListWithoutPurge { get; }

        /// <summary>
        /// Gets the number of live and dead entries in the collection. Does not cause a purge. O(1).
        /// </summary>
        int CompleteCount { get; }

        /// <summary>
        /// Gets the number of dead entries in the collection. Does not cause a purge. O(n).
        /// </summary>
        int DeadCount { get; }

        /// <summary>
        /// Gets the number of live entries in the collection, causing a purge. O(n).
        /// </summary>
        int LiveCount { get; }

        /// <summary>
        /// Gets the number of live entries in the collection without causing a purge. O(n).
        /// </summary>
        int LiveCountWithoutPurge { get; }

        /// <summary>
        /// Removes all dead objects from the collection.
        /// </summary>
        void Purge();
    }
}