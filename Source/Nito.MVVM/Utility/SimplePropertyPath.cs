// <copyright file="SimplePropertyPath.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.Utility
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;

    // FUTURE: Add support for per-property change events.

    /// <summary>
    /// Provides a way to monitor changes along a property path.
    /// </summary>
    public sealed class SimplePropertyPath : NotifyPropertyChangedBase<SimplePropertyPath>, IDisposable
    {
        /// <summary>
        /// The actual root object for this property path.
        /// </summary>
        private object root;

        /// <summary>
        /// The path to evaluate.
        /// </summary>
        private string path;

        /// <summary>
        /// The result of evaluating <see cref="path"/> on <see cref="root"/>.
        /// </summary>
        private object value;

        /// <summary>
        /// The list of subscription steps (individual <see cref="INotifyPropertyChanged"/> subscriptions) used to keep <see cref="Value"/> up-to-date.
        /// </summary>
        private SubscriptionStep[] subscriptions;

        /// <summary>
        /// Gets or sets the root object for this property path.
        /// </summary>
        public object Root
        {
            get
            {
                return this.root;
            }

            set
            {
                if (!object.ReferenceEquals(this.root, value))
                {
                    // This could be made more efficient by not doing a full teardown/rebuild, but that's optimizing an edge case.
                    this.Dismantle();
                    this.root = value;
                    this.Construct();

                    this.OnPropertyChanged(x => x.Root);
                }
            }
        }

        /// <summary>
        /// Gets or sets the property path to evaluate.
        /// </summary>
        public string Path
        {
            get
            {
                return this.path;
            }

            set
            {
                if (this.path != value)
                {
                    this.Dismantle();
                    this.path = value;
                    this.Construct();

                    this.OnPropertyChanged(x => x.Path);
                }
            }
        }

        /// <summary>
        /// Gets or sets the result of <see cref="Path"/> evaluated on <see cref="Root"/>.
        /// </summary>
        public object Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (this.subscriptions == null)
                {
                    PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 0, "Property path not active; ignoring request to set Value.");
                    return;
                }

                if (!this.subscriptions[this.subscriptions.Length - 1].IsSubscribed)
                {
                    PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 0, "Property path not fully subscribed; ignoring request to set Value.");
                    return;
                }

                this.subscriptions[this.subscriptions.Length - 1].Overwrite(value);
            }
        }

        /// <summary>
        /// Reevaluates <see cref="Path"/> on <see cref="Root"/>, updating <see cref="Value"/> if necessary. May invoke <see cref="PropertyChanged"/>.
        /// </summary>
        /// <remarks>
        /// <para>Normally, this function will not be needed. It is only needed if an object evaluated by <see cref="Path"/> does not support <see cref="INotifyPropertyChanged"/>.</para>
        /// </remarks>
        public void RefreshValue()
        {
            // Start out at the root
            object root = this.root;

            for (int i = 0; i != this.subscriptions.Length; ++i)
            {
                // Test whether the evaluated object at this step is different than what was there before
                if (object.ReferenceEquals(this.subscriptions[i].Object, root))
                {
                    // If the new evaluated object is null (and the old one was, too), just return.
                    if (root == null)
                    {
                        // Note that this.Value is already null, and this.Dismantle(i) is unnecessary, because the old value at this step was
                        //  also null.
                        return;
                    }

                    if (i == this.subscriptions.Length - 1)
                    {
                        // At the end of the subscription, set Value.
                        this.UpdateValue(this.subscriptions[i].Evaluate());
                    }
                    else
                    {
                        // Evaluate the root of the next subscription step
                        root = this.subscriptions[i].Evaluate();
                    }
                }
                else
                {
                    // The evaluated object has changed, so dismantle the rest of the subscriptions and rebuild them.
                    this.Dismantle(i);
                    this.Construct(i, root);
                }
            }
        }

        /// <summary>
        /// Unsubscribes from all objects in the property path.
        /// </summary>
        public void Dispose()
        {
            this.Path = null;
        }

        /// <summary>
        /// Updates the evaluated value and raises <see cref="PropertyChanged"/> if necessary.
        /// </summary>
        /// <param name="newValue">The newly-evaluated value.</param>
        private void UpdateValue(object newValue)
        {
            if (!object.Equals(this.value, newValue))
            {
                this.value = newValue;
                this.OnPropertyChanged(x => x.Value);
            }
        }

        /// <summary>
        /// Completely tear down all subscription steps.
        /// </summary>
        private void Dismantle()
        {
            // If no subscriptions are possible yet, we just skip it
            if (this.root == null || this.path == null || this.subscriptions == null)
            {
                return;
            }

            // Unsubscribe each subscription step
            this.Dismantle(0);

            // Free the subscription steps themselves
            this.subscriptions = null;
        }

        /// <summary>
        /// Builds all subscription steps, if possible.
        /// </summary>
        private void Construct()
        {
            // If no subscriptions are possible yet, we just skip it
            if (this.root == null || this.path == null)
            {
                return;
            }

            // Get the names from the property path
            string[] propertyNames = this.path.Split('.');
            this.subscriptions = new SubscriptionStep[propertyNames.Length];

            // Create all the subscription steps
            for (int i = 0; i != propertyNames.Length; ++i)
            {
                this.subscriptions[i] = new SubscriptionStep(propertyNames[i]);
            }

            // Evaluate and subscribe to them all
            this.Construct(0, this.root);
        }

        /// <summary>
        /// Unsubscribes from each subscription step starting at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index at which to start dismantling the subscriptions.</param>
        private void Dismantle(int index)
        {
            for (int i = index; i != this.subscriptions.Length; ++i)
            {
                this.subscriptions[i].Unsubscribe();
            }
        }

        /// <summary>
        /// Evaluates and subscribes to (if possible) each subscription step starting at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index at which to start evaluating and subscribing.</param>
        /// <param name="root">The context object for the subscription step at <paramref name="index"/>.</param>
        private void Construct(int index, object root)
        {
            for (int i = index; i != this.subscriptions.Length; ++i)
            {
                if (root == null)
                {
                    // At this point, there are more subscription steps, but we've encountered a null result.
                    this.UpdateValue(null);
                    return;
                }

                // We assume here that any existing subscriptions have already been removed.
                this.subscriptions[i].Object = root;
                root = this.subscriptions[i].Evaluate();

                // Make a copy of the index, so that the lambdas will bind to the copy.
                int id = i;
                if (id == this.subscriptions.Length - 1)
                {
                    // The last subscription updates Value
                    this.subscriptions[i].Subscribe(() =>
                    {
                        this.UpdateValue(this.subscriptions[id].Evaluate());
                    });
                    
                    // Update Value directly; this is the last thing done by this function
                    this.UpdateValue(root);
                }
                else
                {
                    // Each subscription step updates the next one
                    this.subscriptions[i].Subscribe(() =>
                    {
                        // When the property changes, get the new value
                        object newRoot = this.subscriptions[id].Evaluate();

                        // If it's different than what was evaluated before
                        if (!object.ReferenceEquals(root, newRoot))
                        {
                            // Tear down all successive substitution steps and rebuild with the new object
                            Dismantle(id + 1);
                            Construct(id + 1, newRoot);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Represents a single step in a property path subscription: a single property on a single object.
        /// </summary>
        private sealed class SubscriptionStep
        {
            /// <summary>
            /// The actual delegate that is subscribed. This is null if <see cref="Object"/> is null or if there is no subscription.
            /// </summary>
            private PropertyChangedEventHandler eventHandler;

            /// <summary>
            /// Initializes a new instance of the <see cref="SubscriptionStep"/> class with the given property name.
            /// </summary>
            /// <param name="name">The name of the property used for evaluation.</param>
            public SubscriptionStep(string name)
            {
                this.Name = name;
            }

            /// <summary>
            /// Gets or sets the actual object at this subscription step. This may be null.
            /// </summary>
            public object Object { get; set; }

            /// <summary>
            /// Gets a value indicating whether this subscription step is subscribed.
            /// </summary>
            public bool IsSubscribed
            {
                get { return this.eventHandler != null; }
            }

            /// <summary>
            /// Gets the object used for subscriptions. This is null if <see cref="Object"/> does not support <see cref="INotifyPropertyChanged"/>.
            /// </summary>
            public INotifyPropertyChanged NotifyPropertyChangedObject
            {
                get { return this.Object as INotifyPropertyChanged; }
            }

            /// <summary>
            /// Gets the name of the property subscribed to.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Subscribes to <see cref="Object"/> if it is not null.
            /// </summary>
            /// <param name="action">The action to subscribe.</param>
            public void Subscribe(Action action)
            {
                if (this.NotifyPropertyChangedObject != null)
                {
                    this.eventHandler = (sender, e) =>
                    {
                        if (e.PropertyName == this.Name)
                        {
                            action();
                        }
                    };
                    this.NotifyPropertyChangedObject.PropertyChanged += this.eventHandler;
                }
            }

            /// <summary>
            /// Unsubscribes from <see cref="Object"/> if it is not null.
            /// </summary>
            public void Unsubscribe()
            {
                if (this.NotifyPropertyChangedObject != null && this.eventHandler != null)
                {
                    this.NotifyPropertyChangedObject.PropertyChanged -= this.eventHandler;
                    this.eventHandler = null;
                }
            }

            /// <summary>
            /// Returns the value of the property named <see cref="Name"/> for <see cref="Object"/>. This cannot be called if <see cref="Object"/> is null.
            /// </summary>
            /// <returns>The result of evaluating this subscription step.</returns>
            public object Evaluate()
            {
                PropertyInfo info = this.Object.GetType().GetProperty(this.Name);
                if (info == null)
                {
                    PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 0, "Property '" + this.Name + "' not found for type '" + this.Object.GetType().Name + "', object '" + this.Object.ToString() + "'");
                    return null;
                }

                object ret = info.GetValue(this.Object, null);
                if (ret == null)
                {
                    PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 0, "Property '" + this.Name + "' is null for type '" + this.Object.GetType().Name + "', object '" + this.Object.ToString() + "'");
                }

                return ret;
            }

            /// <summary>
            /// Writes the value of the property named <see cref="Name"/> for <see cref="Object"/>. This cannot be called if <see cref="Object"/> is null.
            /// </summary>
            /// <param name="value">The new value of the property.</param>
            public void Overwrite(object value)
            {
                PropertyInfo info = this.Object.GetType().GetProperty(this.Name);
                if (info == null)
                {
                    PresentationTraceSources.DataBindingSource.TraceEvent(TraceEventType.Error, 0, "Property '" + this.Name + "' not found for type '" + this.Object.GetType().Name + "', object '" + this.Object.ToString() + "'");
                    return;
                }

                info.SetValue(this.Object, value, null);
            }
        }
    }
}
