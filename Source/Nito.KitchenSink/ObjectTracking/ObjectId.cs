namespace Nito.KitchenSink.ObjectTracking
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A unique identifier for a tracked object (target). Uses reference equality. Also acts as a weakly-typed weak reference for the tracked object.
    /// </summary>
    public sealed class ObjectId
    {
        /// <summary>
        /// The list of registered actions to invoke when the target is GC'ed. This member must be locked when accessed unless the target has been GC'ed.
        /// </summary>
        private readonly List<Action<ObjectId>> registeredActions;

        /// <summary>
        /// The underlying weak reference to the target object. This may be <c>null</c> if the GC Detection Thread has already detected that the tracked object has been GC'ed. Code that accesses this member must lock <see cref="referenceLock"/>.
        /// </summary>
        private WeakReference<object> reference;

        /// <summary>
        /// The lock protecting <see cref="reference"/>.
        /// </summary>
        private readonly object referenceLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectId"/> class, identifying the specified target object.
        /// </summary>
        /// <param name="target">The target object identified by this object id instance.</param>
        internal ObjectId(object target)
        {
            this.registeredActions = new List<Action<ObjectId>>();
            this.reference = new WeakReference<object>(target);
            this.referenceLock = new object();
        }

        /// <summary>
        /// Gets a value indicating whether the target is still alive (has not been garbage collected).
        /// </summary>
        public bool IsAlive
        {
            get
            {
                lock (this.referenceLock)
                {
                    if (this.reference == null)
                    {
                        return false;
                    }

                    return this.reference.IsAlive;
                }
            }
        }

        /// <summary>
        /// Gets the target object. Will return null if the object has been garbage collected.
        /// </summary>
        public object Target
        {
            get
            {
                lock (this.referenceLock)
                {
                    if (this.reference == null)
                    {
                        return null;
                    }

                    return this.reference.Target;
                }
            }
        }

        /// <summary>
        /// Returns <see cref="Target"/>, cast to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to which to cast <see cref="Target"/>.</typeparam>
        /// <returns><see cref="Target"/>, cast to <typeparamref name="T"/>.</returns>
        public T TargetAs<T>() where T : class
        {
            return (T)this.Target;
        }

        /// <summary>
        /// Registers a callback that is called sometime after the target is garbage collected. If the target is already garbage collected, the callback is invoked immediately. It is possible that the callback may never be called, if the target is garbage collected shortly before the application domain is unloaded.
        /// </summary>
        /// <param name="action">The callback to invoke some time after the target is garbage collected. The callback must be callable from any thread (including this one). The callback cannot raise exceptions.</param>
        public void Register(Action action)
        {
            this.Register(_ => action());
        }

        /// <summary>
        /// Registers a callback that is called sometime after the target is garbage collected. If the target is already garbage collected, the callback is invoked immediately. It is possible that the callback may never be called, if the target is garbage collected shortly before the application domain is unloaded.
        /// </summary>
        /// <param name="action">The callback to invoke some time after the target is garbage collected. The callback must be callable from any thread (including this one). The callback cannot raise exceptions. The callback takes a single parameter: this <see cref="ObjectId"/>.</param>
        public void Register(Action<ObjectId> action)
        {
            // First, determine if the target is alive.
            // If it is alive, we want to keep it alive until the end of this method (see comments at the end of this method).
            var target = this.Target;
            if (target == null)
            {
                // The target is already GC'ed, so just invoke the action and return.
                action(this);
                return;
            }

            // At this point, we know the target is alive (and we keep it alive until the end of the method).
            // Therefore, locking on registeredActions is necessary.

            // By locking on list, we avoid race conditions where more than one thread attempts to register an action for the same target.
            lock (this.registeredActions)
            {
                this.registeredActions.Add(action);
            }

            // By keeping the target alive until after the registeredActions list is updated,
            // we avoid this race condition:
            //   1) User registers action.
            //   2) Register checks if the target is alive (it is).
            //   3) GC runs and collects the target.
            //   4) GC Detector Thread runs and invokes TargetCollected (which calls back all registered actions).
            //   5) Register continues and adds the action to the registeredActions list.
            //   Result: user's registered action is lost.
            GC.KeepAlive(target);
        }

        /// <summary>
        /// Notifies the <see cref="ObjectId"/> that its target has been GC'ed. This can ONLY be called from the ObjectTracker GC Detection Thread. No locks may be held when this method is called. The target must already be GC'ed when this method is called.
        /// </summary>
        internal void TargetCollected()
        {
            // Note: since the target is already GC'ed, no threads executing Register() will add a callback to registeredActions.

            // At this point, any calls to IsAlive or Target are being passed to the WeakReference, which is returning false/null.

            // Release the WeakReference (GCHandle) resources.
            lock (this.referenceLock)
            {
                this.reference.Dispose();
                this.reference = null;
            }

            // At this point, any calls to IsAlive or Target are stubbed out by this class, and will return false/null.

            // Invoke all callbacks.
            foreach (var action in this.registeredActions)
            {
                action(this);
            }
        }
    }
}
