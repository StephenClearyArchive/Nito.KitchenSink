// <copyright file="InternetHandle.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System;

    /// <summary>
    /// The base class for all internet handles. Note that this wrapper does NOT support asynchronous operations! Multiple threads may safely call <see cref="Dispose"/>.
    /// </summary>
    public abstract class InternetHandle : IDisposable
    {
        /// <summary>
        /// The underlying safe internet handle.
        /// </summary>
        private readonly SafeInternetHandle safeInternetHandle;

        /// <summary>
        /// A local reference to the status callback delegate wrapper (as passed to unmanaged code), to prevent garbage collection.
        /// </summary>
        private object statusCallbackReference;

        /// <summary>
        /// The actual status callback delegate.
        /// </summary>
        private InternetCallback statusCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetHandle"/> class with the specified safe internet handle.
        /// </summary>
        /// <param name="safeInternetHandle">The safe internet handle.</param>
        protected InternetHandle(SafeInternetHandle safeInternetHandle)
        {
            this.safeInternetHandle = safeInternetHandle;
        }

        /// <summary>
        /// An internet status callback delegate, used to report progress.
        /// </summary>
        /// <param name="args">The arguments for the callback delegate. This includes the type of callback optionally with other details.</param>
        public delegate void InternetCallback(InternetCallbackEventArgs args);

        /// <summary>
        /// Gets the safe internet handle. Do not close this handle directly.
        /// </summary>
        public SafeInternetHandle SafeInternetHandle
        {
            get { return this.safeInternetHandle; }
        }

        /// <summary>
        /// Gets or sets the internet status callback delegate, used to report progress.
        /// </summary>
        public InternetCallback StatusCallback
        {
            get
            {
                return this.statusCallback;
            }

            set
            {
                this.statusCallbackReference = NativeMethods.InternetSetStatusCallback(this.SafeInternetHandle, value);
                this.statusCallback = value;
            }
        }

        /// <summary>
        /// Closes the internet handle. This method may be safely invoked by any thread; if a thread is in a blocking operation, another thread may close this handle to abort the operation.
        /// </summary>
        public void Dispose()
        {
            this.safeInternetHandle.Dispose();
        }
    }
}
