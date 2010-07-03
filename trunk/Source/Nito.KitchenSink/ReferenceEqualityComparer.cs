namespace Nito.KitchenSink
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// An object comparer that always compares objects based on reference equality.
    /// </summary>
    /// <typeparam name="T">The type of objects being compared.</typeparam>
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer where T : class
    {
        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj", "Object cannot be null.");
            }

            return obj.GetHashCode();
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            return object.ReferenceEquals(x, y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj", "Object cannot be null.");
            }

            return obj.GetHashCode();
        }
    }
}
