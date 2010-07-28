// <copyright file="EquatableWeakReference.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A strongly-typed weak reference that can be compared for equality. Note that "equivalence" (and the hash code) may change as objects are GC'ed, so this class may NOT be used as any kind of key (it is unpredictably mutable).
    /// Because of these restrictions, this type is not exposed.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    internal sealed class EquatableWeakReference<T> : IEquatable<EquatableWeakReference<T>>, IDisposable where T : class
    {
        /// <summary>
        /// The underlying weak reference.
        /// </summary>
        private readonly WeakReference<T> weakReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquatableWeakReference&lt;T&gt;"/> class referring to the specified target object.
        /// </summary>
        /// <param name="target">The target object.</param>
        public EquatableWeakReference(T target)
        {
            this.weakReference = new WeakReference<T>(target);
        }

        /// <summary>
        /// Gets the target object.
        /// </summary>
        public T Target
        {
            get { return this.weakReference.Target; }
        }

        /// <summary>
        /// Gets a value indicating whether the target object is alive.
        /// </summary>
        public bool IsAlive
        {
            get { return this.weakReference.IsAlive; }
        }

        /// <summary>
        /// Disposes the weak reference.
        /// </summary>
        public void Dispose()
        {
            this.weakReference.Dispose();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance. Two instances are equal if they are the same type and reference equal targets according to the default equality comparer for <typeparamref name="T"/>. Note that equality is not stable.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            // Note: we're restricting the other object to be precisely our type, which isn't strictly necessary.
            // A weakly-typed base class would do just as well.
            var other = obj as EquatableWeakReference<T>;
            if (other == null)
            {
                return false;
            }

            return this.Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type. Two instances are equal if they reference equal targets according to the default equality comparer for <typeparamref name="T"/>. Note that equality is not stable.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(EquatableWeakReference<T> other)
        {
            var thisTarget = this.Target;
            var otherTarget = other.Target;

            if (thisTarget == null || otherTarget == null)
            {
                return (thisTarget == null && otherTarget == null);
            }

            return EqualityComparer<T>.Default.Equals(thisTarget, otherTarget);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. Note that the hash function is not stable.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            var thisTarget = this.Target;
            if (thisTarget == null)
            {
                return 0;
            }

            return thisTarget.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return this.weakReference.ToString();
        }
    }
}
