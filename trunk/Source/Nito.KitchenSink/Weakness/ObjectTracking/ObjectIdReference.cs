// <copyright file="ObjectIdReference.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness.ObjectTracking
{
    using System;

    /// <summary>
    /// A strongly-typed wrapper around <see cref="ObjectId"/>. Derived types use value equality, and are considered equal if they both refer to the same <see cref="ObjectId"/>, even if the type parameter <typeparamref name="T"/> is different. All members are fully threadsafe.
    /// </summary>
    /// <typeparam name="T">A type of the target object. This does not have to be the exact type of the target object.</typeparam>
    public interface IObjectIdReference<out T> where T : class
    {
        /// <summary>
        /// The object id that is wrapped by this instance. This value never changes.
        /// </summary>
        ObjectId ObjectId { get; }

        /// <summary>
        /// Gets a value indicating whether the target is still alive (has not been garbage collected). This must be the same as <c>ObjectId.IsAlive</c>.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Gets the target object. Will return null if the object has been garbage collected. This must be the same as <c>(T)ObjectId.Target</c>.
        /// </summary>
        T Target { get; }

        /// <summary>
        /// Registers a callback that is called sometime after the target is garbage collected. If the target is already garbage collected, the callback is invoked immediately. It is possible that the callback may never be called, if the target is garbage collected shortly before the application domain is unloaded.
        /// </summary>
        /// <param name="action">The callback to invoke some time after the target is garbage collected. The callback must be callable from any thread (including this one). The callback cannot raise exceptions. The callback should not keep a reference to the target. The callback may run concurrently with the target's finalizer and/or other callbacks.</param>
        void Register(Action action);

        /// <summary>
        /// Registers a callback that is called sometime after the target is garbage collected. If the target is already garbage collected, the callback is invoked immediately. It is possible that the callback may never be called, if the target is garbage collected shortly before the application domain is unloaded.
        /// </summary>
        /// <param name="action">The callback to invoke some time after the target is garbage collected. The callback must be callable from any thread (including this one). The callback cannot raise exceptions. The callback should not keep a reference to the target. The callback may run concurrently with the target's finalizer and/or other callbacks. The callback takes a single parameter: <see cref="ObjectId"/>.</param>
        void Register(Action<ObjectId> action);
    }

    /// <summary>
    /// The abstract base class for object id references.
    /// </summary>
    internal abstract class ObjectIdReferenceBase : IEquatable<ObjectIdReferenceBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdReferenceBase"/> class referring to the specified object id.
        /// </summary>
        /// <param name="objectId">The object id that this object id reference refers to.</param>
        internal ObjectIdReferenceBase(ObjectId objectId)
        {
            this.ObjectId = objectId;
        }

        /// <summary>
        /// The object id that is wrapped by this instance. This value never changes.
        /// </summary>
        public ObjectId ObjectId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the target is still alive (has not been garbage collected). This must be the same as <c>ObjectId.IsAlive</c>.
        /// </summary>
        public bool IsAlive
        {
            get { return this.ObjectId.IsAlive; }
        }

        /// <summary>
        /// Registers a callback that is called sometime after the target is garbage collected. If the target is already garbage collected, the callback is invoked immediately. It is possible that the callback may never be called, if the target is garbage collected shortly before the application domain is unloaded.
        /// </summary>
        /// <param name="action">The callback to invoke some time after the target is garbage collected. The callback must be callable from any thread (including this one). The callback cannot raise exceptions. The callback should not keep a reference to the target. The callback may run concurrently with the target's finalizer and/or other callbacks.</param>
        public void Register(Action action)
        {
            this.ObjectId.Register(action);
        }

        /// <summary>
        /// Registers a callback that is called sometime after the target is garbage collected. If the target is already garbage collected, the callback is invoked immediately. It is possible that the callback may never be called, if the target is garbage collected shortly before the application domain is unloaded.
        /// </summary>
        /// <param name="action">The callback to invoke some time after the target is garbage collected. The callback must be callable from any thread (including this one). The callback cannot raise exceptions. The callback should not keep a reference to the target. The callback may run concurrently with the target's finalizer and/or other callbacks. The callback takes a single parameter: <see cref="ObjectId"/>.</param>
        public void Register(Action<ObjectId> action)
        {
            this.ObjectId.Register(action);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current object id reference.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param>
        public override bool Equals(object obj)
        {
            var other = obj as ObjectIdReferenceBase;
            if (other == null)
            {
                return false;
            }

            return this.Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object id reference is equal to another object id reference (i.e., they both refer to the same object id).
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object id reference to compare with this object.</param>
        public bool Equals(ObjectIdReferenceBase other)
        {
            return (this.ObjectId == other.ObjectId);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.ObjectId.GetHashCode();
        }
    }

    /// <summary>
    /// A strongly-typed wrapper around <see cref="ObjectId"/>. Uses value equality; two instances are considered equal if they both refer to the same <see cref="ObjectId"/>, even if the type parameter <typeparamref name="T"/> is different. All members are fully threadsafe.
    /// </summary>
    /// <typeparam name="T">A type of the target object. This does not have to be the exact type of the target object.</typeparam>
    internal sealed class ObjectIdReference<T> : ObjectIdReferenceBase, IObjectIdReference<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdReferenceBase"/> class referring to the specified object id.
        /// </summary>
        /// <param name="objectId">The object id that this object id reference refers to.</param>
        internal ObjectIdReference(ObjectId objectId)
            :base(objectId)
        {
        }

        /// <summary>
        /// Gets the target object. Will return null if the object has been garbage collected. This must be the same as <c>(T)ObjectId.Target</c>.
        /// </summary>
        public T Target
        {
            get { return this.ObjectId.TargetAs<T>(); }
        }
    }
}
