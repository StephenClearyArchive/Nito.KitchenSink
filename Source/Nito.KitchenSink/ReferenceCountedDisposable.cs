// <copyright file="ReferenceCountedDisposable.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;
    using System.Disposables;

    /// <summary>
    /// Provides a reference count for a wrapped disposable object.
    /// </summary>
    /// <typeparam name="T">The type of object to wrap.</typeparam>
    public sealed class ReferenceCountedDisposable<T> : IDisposable where T : IDisposable
    {
        /// <summary>
        /// The underlying reference counted disposable.
        /// </summary>
        private readonly RefCountDisposable disposable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceCountedDisposable&lt;T&gt;"/> class, wrapping the specified disposable object.
        /// </summary>
        /// <param name="value">The disposable object to wrap.</param>
        public ReferenceCountedDisposable(T value)
        {
            this.disposable = new RefCountDisposable(value);
            this.Value = value;
        }

        /// <summary>
        /// Gets the underlying disposable object.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Reduces the reference count on the underlying disposable object.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.disposable.Dispose();
        }

        /// <summary>
        /// Adds a reference count to this wrapper.
        /// </summary>
        /// <returns>A reference-counted wrapper around the same disposable object.</returns>
        public Reference AddReference()
        {
            return new Reference(this);
        }

        /// <summary>
        /// A reference-counted wrapper around a disposable object.
        /// </summary>
        public sealed class Reference : IDisposable
        {
            /// <summary>
            /// A reference-counted child disposable object.
            /// </summary>
            private readonly IDisposable disposable;

            /// <summary>
            /// Initializes a new instance of the <see cref="Reference"/> class with the specified parent.
            /// </summary>
            /// <param name="parent">The parent, which shares this reference count.</param>
            public Reference(ReferenceCountedDisposable<T> parent)
            {
                this.disposable = parent.disposable.GetDisposable();
                this.Value = parent.Value;
            }

            /// <summary>
            /// Gets the underlying disposable object.
            /// </summary>
            public T Value { get; private set; }

            /// <summary>
            /// Reduces the reference count on the underlying disposable object.
            /// </summary>
            void IDisposable.Dispose()
            {
                this.disposable.Dispose();
            }
        }
    }
}
