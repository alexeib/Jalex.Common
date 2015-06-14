using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jalex.Infrastructure.Extensions;

namespace Jalex.Infrastructure.Containers
{
    /// <summary>
    ///     A collection of weak references to objects. Weak references are purged by iteration/count operations, not by
    ///     add/remove operations.
    ///     From http://nitokitchensink.codeplex.com/SourceControl/changeset/view/51391#1006413 via
    ///     http://stackoverflow.com/questions/2837478/is-there-a-way-to-do-weaklist-think-weakreference-in-clr
    /// </summary>
    /// <typeparam name="T">The type of object to hold weak references to.</typeparam>
    /// <remarks>
    ///     <para>
    ///         Since the collection holds weak references to the actual objects, the collection is comprised of both living
    ///         and dead references. Living references refer to objects that have not been garbage collected, and may be used
    ///         as normal references. Dead references refer to objects that have been garbage collected.
    ///     </para>
    ///     <para>Dead references do consume resources; each dead reference is a garbage collection handle.</para>
    ///     <para>
    ///         Dead references may be cleaned up by a <see cref="Purge" /> operation. Some properties and methods cause a
    ///         purge as a side effect; the member documentation specifies whether a purge takes place.
    ///     </para>
    /// </remarks>
    public sealed class WeakCollection<T> : IWeakCollection<T> where T : class
    {
        /// <summary>
        ///     The actual collection of strongly-typed weak references.
        /// </summary>
        private readonly List<WeakReference<T>> _list;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WeakCollection{T}" /> class that is empty.
        /// </summary>
        public WeakCollection()
        {
            _list = new List<WeakReference<T>>();
        }

        /// <summary>
        ///     Gets a sequence of live objects from the collection, causing a purge. The entire sequence MUST always be
        ///     enumerated!
        /// </summary>
        private IEnumerable<T> UnsafeLiveList
        {
            get
            {
                // This implementation uses logic similar to List<T>.RemoveAll, which always has O(n) time.
                //  Some other implementations seen in the wild have O(n*m) time, where m is the number of dead entries.
                //  As m approaches n (e.g., mass object extinctions), their running time approaches O(n^2).
                var writeIndex = 0;
                for (var readIndex = 0; readIndex != _list.Count; ++readIndex)
                {
                    var weakReference = _list[readIndex];
                    T weakDelegate;
                    if (weakReference.TryGetTarget(out weakDelegate))
                    {
                        yield return weakDelegate;

                        if (readIndex != writeIndex) _list[writeIndex] = _list[readIndex];

                        ++writeIndex;
                    }
                }

                _list.RemoveRange(writeIndex, _list.Count - writeIndex);
            }
        }

        #region IWeakCollection<T> Members

        /// <summary>
        ///     Gets a sequence of live objects from the collection, causing a purge.
        /// </summary>
        public IEnumerable<T> LiveList
        {
            get
            {
                var ret = new List<T>(_list.Count);
                ret.AddRange(UnsafeLiveList);
                return ret;
            }
        }

        /// <summary>
        ///     Gets a complete sequence of objects from the collection. Does not cause a purge. Null entries represent dead
        ///     objects.
        /// </summary>
        public IEnumerable<T> CompleteList
        {
            get { return _list.Select(x => x.GetTargetOrDefault()); }
        }

        /// <summary>
        ///     Gets a sequence of live objects from the collection without causing a purge.
        /// </summary>
        public IEnumerable<T> LiveListWithoutPurge
        {
            get { return CompleteList.Where(x => x != null); }
        }

        /// <summary>
        ///     Gets the number of live and dead entries in the collection. Does not cause a purge. O(1).
        /// </summary>
        public int CompleteCount => _list.Count;

        /// <summary>
        ///     Gets the number of dead entries in the collection. Does not cause a purge. O(n).
        /// </summary>
        public int DeadCount
        {
            get { return CompleteList.Count(x => x == null); }
        }

        /// <summary>
        ///     Gets the number of live entries in the collection, causing a purge. O(n).
        /// </summary>
        public int LiveCount => UnsafeLiveList.Count();

        /// <summary>
        ///     Gets the number of live entries in the collection without causing a purge. O(n).
        /// </summary>
        public int LiveCountWithoutPurge
        {
            get { return CompleteList.Count(x => x != null); }
        }

        /// <summary>
        ///     Adds a weak reference to an object to the collection. Does not cause a purge.
        /// </summary>
        /// <param name="item">The object to add a weak reference to.</param>
        public void Add(T item)
        {
            _list.Add(new WeakReference<T>(item));
        }

        /// <summary>
        ///     Removes a weak reference to an object from the collection. Does not cause a purge.
        /// </summary>
        /// <param name="item">The object to remove a weak reference to.</param>
        /// <returns>True if the object was found and removed; false if the object was not found.</returns>
        public bool Remove(T item)
        {
            for (var i = 0; i != _list.Count; ++i)
            {
                var weakReference = _list[i];
                T weakDelegate;
                if (weakReference.TryGetTarget(out weakDelegate) && weakDelegate == item)
                {
                    _list.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Removes all dead objects from the collection.
        /// </summary>
        public void Purge()
        {
            // We cannot simply use List<T>.RemoveAll, because the predicate "x => !x.IsAlive" is not stable.
            var enumerator = UnsafeLiveList.GetEnumerator();
            while (enumerator.MoveNext())
            {
            }
        }

        /// <summary>
        ///     Frees all resources held by the collection.
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        /// <summary>
        ///     Empties the collection.
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        ///     Gets a sequence of live objects from the collection, causing a purge.
        /// </summary>
        /// <returns>The sequence of live objects.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return LiveList.GetEnumerator();
        }

        /// <summary>
        ///     Gets a sequence of live objects from the collection, causing a purge.
        /// </summary>
        /// <returns>The sequence of live objects.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }

        #endregion

        #region ICollection<T> Properties

        /// <summary>
        ///     Gets the number of live entries in the collection, causing a purge. O(n).
        /// </summary>
        int ICollection<T>.Count => LiveCount;

        /// <summary>
        ///     Gets a value indicating whether the collection is read only. Always returns false.
        /// </summary>
        bool ICollection<T>.IsReadOnly => false;

        #endregion

        #region ICollection<T> Methods

        /// <summary>
        ///     Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate.</param>
        /// <returns>True if the collection contains a specific value; false if it does not.</returns>
        bool ICollection<T>.Contains(T item)
        {
            return LiveListWithoutPurge.Contains(item);
        }

        /// <summary>
        ///     Copies all live objects to an array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The index to begin writing into the array.</param>
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            var ret = new List<T>(_list.Count);
            ret.AddRange(UnsafeLiveList);
            ret.CopyTo(array, arrayIndex);
        }

        #endregion
    }
}