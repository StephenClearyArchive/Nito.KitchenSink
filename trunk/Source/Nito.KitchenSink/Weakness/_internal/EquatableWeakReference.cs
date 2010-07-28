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
    internal sealed class EquatableWeakReference<T> : IEquatable<EquatableWeakReference<T>>, IDisposable where T : class
    {
        private readonly WeakReference<T> weakReference;

        public EquatableWeakReference(T target)
        {
            this.weakReference = new WeakReference<T>(target);
        }

        public T Target
        {
            get { return this.weakReference.Target; }
        }

        public bool IsAlive
        {
            get { return this.weakReference.IsAlive; }
        }

        public void Dispose()
        {
            this.weakReference.Dispose();
        }

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

        public override int GetHashCode()
        {
            var thisTarget = this.Target;
            if (thisTarget == null)
            {
                return 0;
            }

            return thisTarget.GetHashCode();
        }

        public override string ToString()
        {
            return this.weakReference.ToString();
        }
    }
}
