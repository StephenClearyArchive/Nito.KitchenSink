using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Nito.Weakness
{
    internal sealed class CountdownSemaphore : IDisposable
    {
        private readonly ManualResetEvent mre;

        private int count;

        public CountdownSemaphore()
        {
            this.mre = new ManualResetEvent(true);
        }

        public WaitHandle WaitHandle
        {
            get { return this.mre; }
        }

        public IDisposable Increment()
        {
            lock (this.mre)
            {
                if (count == 0)
                {
                    mre.Reset();
                }

                ++count;
            }

            return new DecrementOnDispose(this);
        }

        private void Decrement()
        {
            lock (this.mre)
            {
                --count;
                if (count == 0)
                {
                    mre.Set();
                }
            }
        }

        public void Dispose()
        {
            this.mre.Dispose();
        }

        private sealed class DecrementOnDispose : IDisposable
        {
            private readonly CountdownSemaphore parent;

            public DecrementOnDispose(CountdownSemaphore parent)
            {
                this.parent = parent;
            }

            public void Dispose()
            {
                this.parent.Decrement();
            }
        }
    }
}
