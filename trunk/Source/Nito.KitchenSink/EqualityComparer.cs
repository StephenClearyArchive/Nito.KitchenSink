// <copyright file="EqualityComparer.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System.Collections;

    /// <summary>
    /// Provides the <see cref="Default"/> property, which is a default implementation of <see cref="IEqualityComparer"/>.
    /// </summary>
    public static class EqualityComparer
    {
        /// <summary>
        /// An instance of <see cref="DefaultEqualityComparer"/>.
        /// </summary>
        private static readonly IEqualityComparer valueDefault = new DefaultEqualityComparer();

        /// <summary>
        /// Gets the default implementation of <see cref="IEqualityComparer"/>.
        /// </summary>
        /// <remarks>
        /// <para>The default implementation uses <see cref="Object.Equals(Object,Object)"/> and <see cref="Object.GetHashCode"/> to implement <see cref="IEqualityComparer"/>.</para>
        /// </remarks>
        public static IEqualityComparer Default
        {
            get { return valueDefault; }
        }

        /// <summary>
        /// The default implementation of <see cref="IEqualityComparer"/>.
        /// </summary>
        private sealed class DefaultEqualityComparer : IEqualityComparer
        {
            /// <summary>
            /// Compares two objects for equality.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>Whether the two objects are equal.</returns>
            public new bool Equals(object x, object y)
            {
                return object.Equals(x, y);
            }

            /// <summary>
            /// Gets the hash code for an object.
            /// </summary>
            /// <param name="obj">The object to hash.</param>
            /// <returns>The hash code for the object.</returns>
            public int GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
