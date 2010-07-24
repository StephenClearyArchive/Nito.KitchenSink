// <copyright file="WeakReference.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a weak reference, which references an object while still allowing that object to be reclaimed by garbage collection.
    /// </summary>
    /// <remarks>
    /// <para>We define our own type, unrelated to <see cref="System.WeakReference"/> both to provide type safety and because <see cref="System.WeakReference"/> is an incorrect implementation (it does not implement <see cref="IDisposable"/>).</para>
    /// </remarks>
    /// <typeparam name="T">The type of object to reference.</typeparam>
    public sealed class WeakReference<T> : IDisposable where T : class
    {
        /// <summary>
        /// The contained <see cref="SafeGCHandle"/>.
        /// </summary>
        private readonly SafeGCHandle safeHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakReference{T}"/> class, referencing the specified object.
        /// </summary>
        /// <param name="target">The object to track. May not be null.</param>
        public WeakReference(T target)
        {
            this.safeHandle = new SafeGCHandle(target, GCHandleType.Weak);
        }

        /// <summary>
        /// Gets the referenced object. Will return null if the object has been garbage collected.
        /// </summary>
        public T Target
        {
            get { return this.safeHandle.Handle.Target as T; }
        }

        /// <summary>
        /// Gets a value indicating whether the object is still alive (has not been garbage collected).
        /// </summary>
        public bool IsAlive
        {
            get { return this.safeHandle.Handle.Target != null; }
        }

        /// <summary>
        /// Frees the weak reference.
        /// </summary>
        public void Dispose()
        {
            this.safeHandle.Dispose();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            var tmp = this.Target;
            if (tmp == null)
            {
                return typeof(T).Name + ": <null>";
            }

            return typeof(T).Name + ": " + tmp.ToString();
        }
    }
}