using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink
{
    [CLSCompliant(false)]
    public abstract class InternetHandle : IDisposable
    {
        private readonly SafeInternetHandle safeInternetHandle;

        private object statusCallbackReference;
        private InternetCallback statusCallback;

        protected InternetHandle(SafeInternetHandle safeInternetHandle)
        {
            this.safeInternetHandle = safeInternetHandle;
        }

        public void Dispose()
        {
            this.safeInternetHandle.Dispose();
        }

        public SafeInternetHandle SafeInternetHandle
        {
            get { return this.safeInternetHandle; }
        }

        public delegate void InternetCallback(InternetCallbackEventArgs args);

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
    }
}
