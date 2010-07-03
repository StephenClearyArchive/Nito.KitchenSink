using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.ExtensibleObjects
{
    /// <summary>
    /// Extension methods for extension collections.
    /// </summary>
    public static class ExtensionCollectionExtensions
    {
        /// <summary>
        /// Adds a sequence of extension objects to an extension collection.
        /// </summary>
        /// <param name="this">The extension collection to which to add the extensions.</param>
        /// <param name="extensions">The sequence of extension objects to add to the extension collection.</param>
        public static void AddRange(this IExtensionCollection @this, IEnumerable<IExtension> extensions)
        {
            foreach (var extension in extensions)
            {
                @this.Add(extension);
            }
        }

        /// <summary>
        /// Removes all extension objects from this collection and moves them to a new collection.
        /// </summary>
        /// <param name="from">The extension collection from which to remove the extensions.</param>
        /// <param name="to">The extension collection to which to add the extensions.</param>
        public static void MoveAllExtensionsTo(this IExtensionCollection from, IExtensionCollection to)
        {
            var list = from.ToList();
            from.Clear();
            to.AddRange(list);
        }
    }
}
