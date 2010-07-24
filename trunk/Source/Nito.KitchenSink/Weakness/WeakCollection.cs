// <copyright file="WeakCollection.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A collection of weak references to objects of type <typeparamref name="T"/>. The <see cref="ICollection{T}"/> implementations are understood to work on all the objects of the collection (both dead and alive), and do not implicitly cause purges.
    /// </summary>
    /// <typeparam name="T">The type of objects to hold weak references to.</typeparam>
    public interface IWeakCollection<T> : ICollection<T>, IDisposable
    {
        /// <summary>
        /// Gets a sequence of live objects from the collection, causing a purge. The purge may occur at the beginning of the enumeration, at the end of the enumeration, or progressively as the sequence is enumerated.
        /// </summary>
        IEnumerable<T> LiveList { get; }

        /// <summary>
        /// Removes all dead objects from the collection.
        /// </summary>
        void Purge();
    }

    /// <summary>
    /// A collection of weak references to objects. Weak references are purged by iteration/count operations, not by add/remove operations.
    /// </summary>
    /// <typeparam name="T">The type of object to hold weak references to.</typeparam>
    /// <remarks>
    /// <para>Since the collection holds weak references to the actual objects, the collection is comprised of both living and dead references. Living references refer to objects that have not been garbage collected, and may be used as normal references. Dead references refer to objects that have been garbage collected.</para>
    /// <para>Dead references do consume resources; each dead reference is a garbage collection handle.</para>
    /// <para>Dead references may be cleaned up by a <see cref="Purge"/> operation. Some properties and methods cause a purge as a side effect; the member documentation specifies whether a purge takes place.</para>
    /// </remarks>
    public sealed class WeakCollection<T> : IWeakCollection<T> where T : class
    {
        /// <summary>
        /// The actual collection of strongly-typed weak references.
        /// </summary>
        private List<WeakReference<T>> list;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakCollection{T}"/> class that is empty.
        /// </summary>
        public WeakCollection()
        {
            this.list = new List<WeakReference<T>>();
        }

        /// <summary>
        /// Gets a sequence of live objects from the collection, causing a purge.
        /// </summary>
        public IEnumerable<T> LiveList
        {
            get
            {
                List<T> ret = new List<T>(this.list.Count);
                ret.AddRange(this.UnsafeLiveList);
                return ret;
            }
        }

        /// <summary>
        /// Gets the number of live and dead entries in the collection, without causing a purge. O(1).
        /// </summary>
        public int Count
        {
            get { return this.list.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read only. Always returns false.
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a sequence of live objects from the collection, causing a purge. The entire sequence MUST always be enumerated!
        /// </summary>
        private IEnumerable<T> UnsafeLiveList
        {
            get
            {
                // This implementation uses logic similar to List<T>.RemoveAll, which always has O(n) time.
                //  Some other implementations seen in the wild have O(n*m) time, where m is the number of dead entries.
                //  As m approaches n (e.g., mass object extinctions), their running time approaches O(n^2).
                int writeIndex = 0;
                for (int readIndex = 0; readIndex != this.list.Count; ++readIndex)
                {
                    WeakReference<T> weakReference = this.list[readIndex];
                    T weakDelegate = weakReference.Target;
                    if (weakDelegate != null)
                    {
                        yield return weakDelegate;

                        if (readIndex != writeIndex)
                        {
                            this.list[writeIndex] = this.list[readIndex];
                        }

                        ++writeIndex;
                    }
                    else
                    {
                        weakReference.Dispose();
                    }
                }

                this.list.RemoveRange(writeIndex, this.list.Count - writeIndex);
            }
        }

        /// <summary>
        /// Adds a weak reference to an object to the collection. Does not cause a purge.
        /// </summary>
        /// <param name="item">The object to add a weak reference to.</param>
        public void Add(T item)
        {
            this.list.Add(new WeakReference<T>(item));
        }

        /// <summary>
        /// Removes a weak reference to an object from the collection. Does not cause a purge.
        /// </summary>
        /// <param name="item">The object to remove a weak reference to.</param>
        /// <returns>True if the object was found and removed; false if the object was not found.</returns>
        public bool Remove(T item)
        {
            for (int i = 0; i != this.list.Count; ++i)
            {
                WeakReference<T> weakReference = this.list[i];
                T weakDelegate = weakReference.Target;
                if (weakDelegate == item)
                {
                    this.list.RemoveAt(i);
                    weakReference.Dispose();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes all dead objects from the collection.
        /// </summary>
        public void Purge()
        {
            // We cannot simply use List<T>.RemoveAll, because the predicate "x => !x.IsAlive" is not stable.
            this.UnsafeLiveList.Run();
        }

        /// <summary>
        /// Frees all resources held by the collection.
        /// </summary>
        public void Dispose()
        {
            this.Clear();
        }

        /// <summary>
        /// Empties the collection.
        /// </summary>
        public void Clear()
        {
            foreach (WeakReference<T> weakReference in this.list)
            {
                weakReference.Dispose();
            }

            this.list.Clear();
        }

        /// <summary>
        /// Determines whether the collection contains a specific value. Does not cause a purge.
        /// </summary>
        /// <param name="item">The object to locate.</param>
        /// <returns>True if the collection contains a specific value; false if it does not.</returns>
        bool ICollection<T>.Contains(T item)
        {
            return this.Where(x => x != null).Contains(item);
        }

        /// <summary>
        /// Copies all live and dead objects to an array. Does not cause a purge.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The index to begin writing into the array.</param>
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            this.CopyToImpl(array, arrayIndex);
        }

        /// <summary>
        /// Gets a sequence of live and dead objects from the collection. Does not cause a purge.
        /// </summary>
        /// <returns>The sequence of live and dead objects.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.list.Select(x => x.Target).GetEnumerator();
        }

        /// <summary>
        /// Gets a sequence of live and dead objects from the collection. Does not cause a purge.
        /// </summary>
        /// <returns>The sequence of live and dead objects.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return "WeakCollection<" + typeof(T).Name + "> (" + this.Count + ")";
        }
    }
}