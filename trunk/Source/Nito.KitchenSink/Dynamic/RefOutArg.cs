// <copyright file="RefOutArg.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.Dynamic
{
    /// <summary>
    /// A wrapper around a "ref" or "out" argument invoked dynamically.
    /// </summary>
    public sealed class RefOutArg
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefOutArg"/> class.
        /// </summary>
        private RefOutArg()
        {
        }

        /// <summary>
        /// Gets or sets the wrapped value as an object.
        /// </summary>
        public object ValueAsObject { get; set; }

        /// <summary>
        /// Gets or sets the wrapped value.
        /// </summary>
        public dynamic Value
        {
            get
            {
                return this.ValueAsObject;
            }

            set
            {
                this.ValueAsObject = value;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RefOutArg"/> class wrapping the default value of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of value to wrap.</typeparam>
        /// <returns>A new instance of the <see cref="RefOutArg"/> class wrapping the default value of <typeparamref name="T"/>.</returns>
        public static RefOutArg Create<T>()
        {
            return new RefOutArg { ValueAsObject = default(T) };
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RefOutArg"/> class wrapping the specified value.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <returns>A new instance of the <see cref="RefOutArg"/> class wrapping the specified value.</returns>
        public static RefOutArg Create(object value)
        {
            return new RefOutArg { ValueAsObject = value };
        }
    }
}