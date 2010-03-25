using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    [CLSCompliant(false)]
    public sealed class InternetException : Exception
    {
        public InternetException(uint code, string message)
            : base(message)
        {
            this.Code = code;
        }

        public uint Code { get; private set; }
    }
}
