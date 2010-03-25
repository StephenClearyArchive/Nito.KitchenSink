using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Nito.KitchenSink
{
    /// <summary>
    /// Represents an unmanaged HINTERNET resource.
    /// </summary>
    public sealed class SafeInternetHandle : SafeHandle
    {
        public SafeInternetHandle()
            : base(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the handle value is invalid.
        /// </summary>
        /// <returns>true if the handle value is invalid; otherwise, false.</returns>
        public override bool IsInvalid
        {
            get { return this.handle == IntPtr.Zero; }
        }

        /// <summary>
        /// Executes the code required to free the handle.
        /// </summary>
        /// <returns>true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false. In this case, it generates a releaseHandleFailed MDA Managed Debugging Assistant.</returns>
        protected override bool ReleaseHandle()
        {
            return NativeMethods.InternetCloseHandle(this.handle);
        }
    }
}
