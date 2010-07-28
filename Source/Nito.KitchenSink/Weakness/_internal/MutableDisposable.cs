// <copyright file="MutableDisposable.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;

    internal sealed class MutableDisposable<T> : IDisposable where T : class, IDisposable
    {
        public T Value { get; set; }

        public void Dispose()
        {
            if (this.Value != null)
            {
                this.Value.Dispose();
            }
        }
    }
}
