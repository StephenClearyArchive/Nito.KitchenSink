// <copyright file="ExtensibleWrapper.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.ExtensibleObjects
{
    /// <summary>
    /// Combines an object with a collection of extensions.
    /// </summary>
    /// <typeparam name="T">The type of object to wrap.</typeparam>
    public sealed class ExtensibleWrapper<T> : IExtensibleObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensibleWrapper&lt;T&gt;"/> class with a default value for <see cref="Object"/>.
        /// </summary>
        public ExtensibleWrapper()
        {
            this.Extensions = new ExtensionCollection(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensibleWrapper&lt;T&gt;"/> class with the specified value for <see cref="Object"/>.
        /// </summary>
        /// <param name="object">The object to wrap.</param>
        public ExtensibleWrapper(T @object)
            : this()
        {
            this.Object = @object;
        }

        /// <summary>
        /// Gets or sets the object that is made extensible by this wrapper.
        /// </summary>
        public T Object { get; set; }

        /// <summary>
        /// Gets or the extensions applied to this object.
        /// </summary>
        public IExtensionCollection Extensions { get; private set; }
    }
}
