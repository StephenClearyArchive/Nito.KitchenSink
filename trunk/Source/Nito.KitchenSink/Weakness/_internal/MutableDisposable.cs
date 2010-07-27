using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.Weakness
{
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
