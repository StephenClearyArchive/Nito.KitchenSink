using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.ExtensibleObjects
{
    /// <summary>
    /// Enables an object to extend any other object through aggregation.
    /// </summary>
    public interface IExtension
    {
        /// <summary>
        /// Enables an extension object to find out when it has been aggregated. Called when the extension is added to the <see cref="IExtensibleObject.Extensions"/> property. This method may not raise an exception.
        /// </summary>
        /// <param name="owner">The extensible object that aggregates this extension. This object may implement <see cref="IExtensibleObject"/>.</param>
        void Attach(object owner);

        /// <summary>
        /// Enables an object to find out when it is no longer aggregated. Called when an extension is removed from the <see cref="IExtensibleObject.Extensions"/> property. This method may not raise an exception and may be called from any thread.
        /// </summary>
        /// <param name="owner">The extensible object that no longer aggregates this extension. This object may implement <see cref="IExtensibleObject"/>.</param>
        void Detach(object owner);
    }

    /// <summary>
    /// Enables an object to be extended through aggregation.
    /// </summary>
    public interface IExtensibleObject
    {
        /// <summary>
        /// Gets the collection of extension objects aggregated by this extensible object.
        /// </summary>
        IExtensionCollection Extensions { get; }
    }

    /// <summary>
    /// A collection of extension objects, which are notified when they are added to or removed from this collection.
    /// </summary>
    public interface IExtensionCollection : ICollection<IExtension>
    {
    }
}
