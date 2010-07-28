// <copyright file="ObjectTracker.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness.ObjectTracking
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

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
        /// The 
        /// </summary>
        private readonly BlockingCollection<Command> gcThreadCommands;

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
            this.gcThreadCommands = new BlockingCollection<Command>();
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
        public int GCDetectionWaitTimeInMilliseconds
        {
            get
            {
                return this.gcDetectionWaitTimeInMilliseconds;
            }

            set
            {
                if (value < 0)
                {
                    throw new InvalidOperationException("Invalid GC detection thread wait time value.");
                }

                var command = new UpdateGCDetectionWaitTimeCommand { GCDetectionWaitTimeInMilliseconds = value };
                this.gcThreadCommands.Add(command);
                command.Wait();
            }
        }

        /// <summary>
        /// Pauses the GC detection thread. The returned disposable object must be disposed to unpause the GC detection thread. Multiple invokations of this method are valid; each returned object must be disposed to unpause the GC detection thread. The GC detection thread is paused before this method returns.
        /// </summary>
        /// <returns>A disposable which, when disposed, unpauses the GC detection thread.</returns>
        public IDisposable PauseGCDetectionThread()
        {
            var command = new IncrementPauseCountCommand();
            this.gcThreadCommands.Add(command);
            command.Wait();

            return new AnonymousDisposable { Dispose = () => this.gcThreadCommands.Add(new DecrementPauseCountCommand()) };
        }

        /// <summary>
        /// The main thread procedure for the GC detection thread. This thread does not exit until its AppDomain is unloaded.
        /// </summary>
        private void GCDetectionThreadProc()
        {
            // The list of object references whose targets have been collected.
            var collected = new List<ObjectId>();
            int pauseCount = 0;

            while (true)
            {
                // Wait for the specified time or until a command arrives.
                Command command;
                if (this.gcThreadCommands.TryTake(out command, pauseCount == 0 ? this.gcDetectionWaitTimeInMilliseconds : Timeout.Infinite))
                {
                    if (command is IncrementPauseCountCommand)
                    {
                        ++pauseCount;
                    }
                    else if (command is DecrementPauseCountCommand)
                    {
                        --pauseCount;
                    }
                    else
                    {
                        var cmd = command as UpdateGCDetectionWaitTimeCommand;
                        this.gcDetectionWaitTimeInMilliseconds = cmd.GCDetectionWaitTimeInMilliseconds;
                    }

                    command.Completed();
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
        /// <exception cref="InvalidOperationException">The target object is not reference-equatable.</exception>
        public ObjectId TrackObject(object target)
        {
            var type = target.GetType();

            // Attempt to look up the type in the first tier; if it succeeds, then the type must be trackable.
            ConcurrentDictionary<int, List<ObjectId>> dictionary;
            if (!this.lookup.TryGetValue(type, out dictionary))
            {
                // The type is not already present in the first tier, so check it.
                if (!type.IsReferenceEquatable())
                {
                    throw new InvalidOperationException("The target object is not reference-equatable.");
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
        /// <exception cref="InvalidOperationException">The target object is not reference-equatable.</exception>
        public IObjectIdReference<T> Track<T>(T target) where T : class
        {
            return new ObjectIdReference<T>((ObjectId)TrackObject(target));
        }

        /// <summary>
        /// A command for the GC detection thread.
        /// </summary>
        private abstract class Command
        {
            /// <summary>
            /// Marks this command as completed.
            /// </summary>
            virtual public void Completed()
            {
            }
        }

        /// <summary>
        /// A GC detection thread command that can be waited on by the sender.
        /// </summary>
        private abstract class WaitableCommand : Command
        {
            /// <summary>
            /// The MRE that is set when this command is completed.
            /// </summary>
            private readonly ManualResetEventSlim mre;

            /// <summary>
            /// Initializes a new instance of the <see cref="WaitableCommand"/> class.
            /// </summary>
            protected WaitableCommand()
            {
                this.mre = new ManualResetEventSlim(false);
            }

            /// <summary>
            /// Marks this command as completed.
            /// </summary>
            public override void Completed()
            {
                this.mre.Set();
            }

            /// <summary>
            /// Waits for the command to complete.
            /// </summary>
            public void Wait()
            {
                this.mre.Wait();
            }
        }

        /// <summary>
        /// Instructs the GC detection thread to update its wait poll time.
        /// </summary>
        private sealed class UpdateGCDetectionWaitTimeCommand : WaitableCommand
        {
            /// <summary>
            /// Gets or sets the amount of time for the GC detection thread to wait in-between each scan of all tracked objects, in milliseconds.
            /// </summary>
            public int GCDetectionWaitTimeInMilliseconds { get; set; }
        }

        /// <summary>
        /// Instructs the GC detection thread to increment its pause count, causing it to pause.
        /// </summary>
        private sealed class IncrementPauseCountCommand : WaitableCommand
        {
        }

        /// <summary>
        /// Instructs the GC detection thread to decrement its pause count, possibly causing it to unpause.
        /// </summary>
        private sealed class DecrementPauseCountCommand : Command
        {
        }
    }
}
