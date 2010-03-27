// <copyright file="SafeInternetHandle.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents an unmanaged HINTERNET resource.
    /// </summary>
    public sealed class SafeInternetHandle : SafeHandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeInternetHandle"/> class.
        /// </summary>
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
            return UnsafeNativeMethods.InternetCloseHandle(this.handle);
        }
    }
}
