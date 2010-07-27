// <copyright file="Extensions.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Various extension methods, only used internally.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Returns <c>true</c> if this type uses reference equality (i.e., does not override <see cref="object.Equals(object)"/>); returns <c>false</c> if this type or any of its base types override <see cref="object.Equals(object)"/>. This method returns <c>false</c> for any interface type, and returns <c>true</c> for any reference-equatable base class even if a derived class is not reference-equatable; the best way to determine if an object uses reference equality is to pass the exact type of that object.
        /// </summary>
        /// <param name="type">The type to test for reference equality.</param>
        /// <returns>Returns <c>true</c> if this type uses reference equality (i.e., does not override <see cref="object.Equals(object)"/>); returns <c>false</c> if this type or any of its base types override <see cref="object.Equals(object)"/>.</returns>
        public static bool IsReferenceEquatable(this Type type)
        {
            // Only reference types can use reference equality.
            if (!type.IsClass || type.IsPointer)
            {
                return false;
            }

            // Find all methods called "Equals" defined in the type's hierarchy (except object.Equals), and retrieve the base definitions.
            var equalsMethods = from method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                                where method.Name == "Equals" && method.DeclaringType != typeof(object)
                                select method.GetBaseDefinition();

            // Take those base definitions and check if any of them are object.Equals. If there are any, then we know that the type overrides
            //  object.Equals or derives from a type that overrides object.Equals.
            var objectEqualsMethod = equalsMethods.Any(method => method.DeclaringType == typeof(object));

            return !objectEqualsMethod;
        }

        /// <summary>
        /// Returns a mutable disposable wrapper around the source disposable.
        /// </summary>
        /// <typeparam name="T">The type of the source disposable.</typeparam>
        /// <param name="value">The source disposable.</param>
        /// <returns>A mutable disposable wrapper around the source disposable.</returns>
        public static MutableDisposable<T> MutableWrapper<T>(this T value) where T : class, IDisposable
        {
            return new MutableDisposable<T> { Value = value };
        }
    }
}
