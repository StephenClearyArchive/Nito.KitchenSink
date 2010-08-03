// <copyright file="ExtensibleObject.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.ExtensibleObjects
{
    /// <summary>
    /// A simple base class for extensible objects.
    /// </summary>
    public abstract class ExtensibleObject : IExtensibleObject
    {
        /// <summary>
        /// The collection of extensions.
        /// </summary>
        private readonly ExtensionCollection extensions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensibleObject"/> class.
        /// </summary>
        protected ExtensibleObject()
        {
            this.extensions = new ExtensionCollection(this);
        }

        /// <summary>
        /// Gets the collection of extension objects aggregated by this extensible object.
        /// </summary>
        public IExtensionCollection Extensions
        {
            get
            {
                return this.extensions;
            }
        }
    }
}
