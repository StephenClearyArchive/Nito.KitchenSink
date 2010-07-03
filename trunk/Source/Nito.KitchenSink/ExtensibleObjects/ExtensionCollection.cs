using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.ExtensibleObjects
{
    /// <summary>
    /// A collection of extension objects, which are notified as they are added to and removed from this collection.
    /// </summary>
    public sealed class ExtensionCollection : IExtensionCollection
    {
        /// <summary>
        /// The underlying collection of extension objects.
        /// </summary>
        private readonly List<IExtension> list;

        /// <summary>
        /// The owner of this collection of extension objects.
        /// </summary>
        private readonly IExtensibleObject owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionCollection"/> class with the specified owner.
        /// </summary>
        /// <param name="owner">The owner of this collection of extension objects.</param>
        public ExtensionCollection(IExtensibleObject owner)
        {
            this.owner = owner;
            this.list = new List<IExtension>();
        }

        void ICollection<IExtension>.Add(IExtension item)
        {
            item.Attach(this.owner);
            this.list.Add(item);
        }

        void ICollection<IExtension>.Clear()
        {
            this.list.ForEach(x => x.Detach(this.owner));
            this.list.Clear();
        }

        bool ICollection<IExtension>.Contains(IExtension item)
        {
            return this.list.Contains(item);
        }

        void ICollection<IExtension>.CopyTo(IExtension[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        int ICollection<IExtension>.Count
        {
            get { return this.list.Count; }
        }

        bool ICollection<IExtension>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<IExtension>.Remove(IExtension item)
        {
            var ret = this.list.Remove(item);
            if (ret)
            {
                item.Detach(this.owner);
            }

            return ret;
        }

        IEnumerator<IExtension> IEnumerable<IExtension>.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.list.GetEnumerator();
        }
    }
}
