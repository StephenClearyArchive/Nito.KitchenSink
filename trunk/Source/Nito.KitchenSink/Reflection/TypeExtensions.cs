namespace Nito.KitchenSink.Reflection
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Extension methods for the <see cref="Type"/> type.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if this type uses reference equality; returns <c>false</c> if this type or a base type overrides <see cref="object.Equals(object)"/>.
        /// </summary>
        /// <param name="type">The type to test for reference equality.</param>
        /// <returns>Returns <c>true</c> if this type uses reference equality; returns <c>false</c> if this type or a base type overrides <see cref="object.Equals(object)"/>.</returns>
        public static bool UsesReferenceEquality(this Type type)
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
            //  object.Equals or inherits from a type that overrides object.Equals.
            var objectEqualsMethod = (from method in equalsMethods
                                      where method.DeclaringType == typeof(object)
                                      select method).Any();

            return !objectEqualsMethod;
        }
    }
}
