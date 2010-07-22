namespace Nito.KitchenSink.ObjectTracking
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Reflection;

    /// <summary>
    /// An object that tracks the lifetimes of other objects. This class is fully thread-safe.
    /// </summary>
    public sealed class ObjectTracker
    {
        // Note: this class is designed so that in the future, multiple ObjectTracker instances may be possible.
        // However, things get pretty complex once the ObjectTracker itself is eligible for GC.
        // Here's a list of things that would need to change to allow multiple ObjectTracers in a single AppDomain:
        //  1) Make the constructor public.
        //  2) Make the Default instance lazy-constructed.
        //  3) Add a "kill signal" to the GC detection thread.
        //  4) Have ObjectTracker implement IDisposable to clean up GCHandles.
        //  5) (the hard one, #1) Decide on the semantics when tracked objects outlive their tracker (also note the additional race conditions with the GC).
        //  6) (the hard one, #2) Decide on multithreaded semantics during disposal.

        /// <summary>
        /// The structure that keeps track of all tracked objects. There are two tiers (exact type and hash code) to mitigate contention. Once constructed, these collection objects are never GC'ed.
        /// </summary>
        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, List<ObjectId>>> lookup = new ConcurrentDictionary<Type, ConcurrentDictionary<int, List<ObjectId>>>();

        /// <summary>
        /// The GC detection thread. This thread periodically scans all tracked objects, searching for those which have been GC'ed.
        /// </summary>
        private readonly Thread gcDetectionThread;

        /// <summary>
        /// The signal sent to the GC detection thread to tell it to use the updated <see cref="gcDetectionWaitTimeInMilliseconds"/>.
        /// </summary>
        private readonly ManualResetEvent gcDetectionWakeup;

        /// <summary>
        /// The amount of time for the GC detection thread to wait in-between each scan of all tracked objects, in milliseconds.
        /// </summary>
        private volatile int gcDetectionWaitTimeInMilliseconds = 3000;

        /// <summary>
        /// The single global instance.
        /// </summary>
        private static readonly ObjectTracker defaultObjectTracker = new ObjectTracker();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTracker"/> class.
        /// </summary>
        internal ObjectTracker()
        {
            this.gcDetectionWakeup = new ManualResetEvent(false);
            this.gcDetectionThread = new Thread(this.GCDetectionThreadProc)
            {
                Name = "Nito.ObjectTracking GC Detection",
                IsBackground = true
            };

            this.gcDetectionThread.Start();
        }

        /// <summary>
        /// Gets the default instance of <see cref="ObjectTracker"/> for this <see cref="AppDomain"/>.
        /// </summary>
        public static ObjectTracker Default
        {
            get { return defaultObjectTracker; }
        }

        /// <summary>
        /// Gets or sets the amount of time for the GC detection thread to wait in-between each scan of all tracked objects, in milliseconds. Setting this value "resets" the wait time, causing the GC Detection thread to begin waiting with the new timeout value.
        /// </summary>
        /// <remarks>
        /// <para>This may be set to <see cref="Timeout.Infinite"/> to disable GC detection.</para>
        /// </remarks>
        public int GCDetectionWaitTimeInMilliseconds
        {
            get
            {
                return this.gcDetectionWaitTimeInMilliseconds;
            }

            set
            {
                if (value < -1)
                {
                    throw new InvalidOperationException("Invalid GC detection thread wait time value.");
                }

                this.gcDetectionWaitTimeInMilliseconds = value;
                this.gcDetectionWakeup.Set();
            }
        }

        /// <summary>
        /// The main thread procedure for the GC detection thread. This thread does not exit until its AppDomain is unloaded.
        /// </summary>
        private void GCDetectionThreadProc()
        {
            // The list of object references whose targets have been collected.
            var collected = new List<ObjectId>();

            while (true)
            {
                // Pause for the specified time.
                if (this.gcDetectionWakeup.WaitOne(this.gcDetectionWaitTimeInMilliseconds))
                {
                    // The wakeup signal was given, so just update our wait time and wait again.
                    this.gcDetectionWakeup.Reset();
                    continue;
                }

                // Concurrently iterate first two tiers.
                foreach (var list in this.lookup.Select(x => x.Value).SelectMany(dictionary => dictionary.Select(x => x.Value)))
                {
                    // Lock the list and traverse it, removing any references to collected targets.
                    lock (list)
                    {
                        for (int i = list.Count - 1; i >= 0; --i)
                        {
                            var reference = list[i];
                            if (!reference.IsAlive)
                            {
                                collected.Add(reference);
                                list.RemoveAt(i);
                            }
                        }
                    }

                    // Invoke all registered actions and dispose the underlying weak references.
                    foreach (var reference in collected)
                    {
                        reference.TargetCollected();
                    }

                    // Allow the ObjectReference objects themselves to be GC'ed (unless the user has a copy).
                    collected.Clear();
                }
            }
        }

        /// <summary>
        /// Tracks a target object. Creates an <see cref="ObjectId"/> if one doesn't already exist for the target, and returns that object id.
        /// </summary>
        /// <param name="target">The target object to track.</param>
        /// <returns>A unique <see cref="ObjectId"/> that identifies and tracks the target object.</returns>
        public ObjectId TrackObject(object target)
        {
            var type = target.GetType();

            // Attempt to look up the type in the first tier; if it succeeds, then the type must be trackable.
            ConcurrentDictionary<int, List<ObjectId>> dictionary;
            if (!this.lookup.TryGetValue(type, out dictionary))
            {
                // The type is not already present in the first tier, so check it.
                if (!type.UsesReferenceEquality())
                {
                    throw new InvalidOperationException("Cannot track an object unless it uses reference equality.");
                }

                // Add the type to the first tier.
                dictionary = this.lookup.GetOrAdd(type, _ => new ConcurrentDictionary<int, List<ObjectId>>());
            }

            // Look up the hash code in the second tier, adding it if it's not present.
            List<ObjectId> list = dictionary.GetOrAdd(target.GetHashCode(), _ => new List<ObjectId>());

            // Search for an existing ObjectReference.
            lock (list)
            {
                foreach (var reference in list)
                {
                    if (reference.Target == target)
                    {
                        return reference;
                    }
                }

                // No existing ObjectReference was found, so create a new one and return it.
                var ret = new ObjectId(target);
                list.Add(ret);
                return ret;
            }
        }

        /// <summary>
        /// Tracks a target object. Creates an <see cref="ObjectId"/> if one doesn't already exist for the target, and returns a strongly-typed reference to that object id.
        /// </summary>
        /// <param name="target">The target object to track.</param>
        /// <returns>A strongly-typed reference to the unique <see cref="ObjectId"/> that identifies and tracks the target object.</returns>
        public IObjectIdReference<T> Track<T>(T target) where T : class
        {
            return new ObjectIdReference<T>((ObjectId)TrackObject(target));
        }
    }
}
