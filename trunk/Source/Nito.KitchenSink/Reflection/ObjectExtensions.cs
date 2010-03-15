// <copyright file="ObjectExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.Reflection
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines extension methods useful when doing reflection.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Reads an untyped property from an object.
        /// </summary>
        /// <param name="this">The object from which to read the property. May not be <c>null</c>.</param>
        /// <param name="name">The name of the property to read. This property must exist.</param>
        /// <returns>The value of the property for that object.</returns>
        /// <exception cref="ArgumentNullException">Either <paramref name="this"/> or <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">The object <paramref name="this"/> does not have a property named <paramref name="name"/>.</exception>
        public static object GetProperty(this object @this, string name)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("@this", "Object passed to GetProperty cannot be null.");
            }

            var property = @this.GetType().GetProperty(name);
            if (property == null)
            {
                throw new KeyNotFoundException("The type " + @this.GetType().Name + " does not contain a property named " + name + ".");
            }

            return property.GetValue(@this, null);
        }

        /// <summary>
        /// Reads a property from an object.
        /// </summary>
        /// <typeparam name="T">The type of the property that is returned.</typeparam>
        /// <param name="this">The object from which to read the property. May not be <c>null</c>.</param>
        /// <param name="name">The name of the property to read. This property must exist.</param>
        /// <returns>The value of the property for that object.</returns>
        /// <exception cref="ArgumentNullException">Either <paramref name="this"/> or <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">The object <paramref name="this"/> does not have a property named <paramref name="name"/>.</exception>
        /// <exception cref="InvalidCastException">The property was found, but is not of type <typeparamref name="T"/>.</exception>
        public static T GetProperty<T>(this object @this, string name)
        {
            var ret = @this.GetProperty(name);
            if (ret == null)
            {
                return default(T);
            }
            else if (ret is T)
            {
                return (T)ret;
            }
            else
            {
                throw new InvalidCastException("The type " + @this.GetType().Name + " does have a property named " + name + ", but it is of type " + ret.GetType().Name + ", not " + typeof(T).Name + ".");
            }
        }
    }
}
