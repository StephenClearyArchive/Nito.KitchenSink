// <copyright file="AutomaticDelegateProperty.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;

    /// <summary>
    /// Delegate-based property that ties in to <see cref="CommandManager.RequerySuggested"/>.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <remarks>
    /// <para>The delegate used to calculate the property value (<see cref="Evaluate"/>) should not be long-running, since it is invoked each time <see cref="CommandManager.RequerySuggested"/> is raised.</para>
    /// </remarks>
    public sealed class AutomaticDelegateProperty<T> : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// The handler subscribed to <see cref="CommandManager.RequerySuggested"/>. Since that is a weak event, we have to hold the delegate.
        /// </summary>
        private EventHandler canExecuteChangedDelegate;

        /// <summary>
        /// The result of the last evaluation.
        /// </summary>
        private T value;

        /// <summary>
        /// The delegate to execute when evaluating the value of this property.
        /// </summary>
        private Func<T> evaluate;

        /// <summary>
        /// The backing field for the <see cref="PropertyChanged"/> event.
        /// </summary>
        private PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticDelegateProperty{T}"/> class with a default comparer and a null delegate.
        /// </summary>
        public AutomaticDelegateProperty()
        {
            this.canExecuteChangedDelegate = new EventHandler((sender, e) => this.Update());
            CommandManager.RequerySuggested += this.canExecuteChangedDelegate;
            this.Comparer = EqualityComparer<T>.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticDelegateProperty{T}"/> class with the specified comparer and a null delegate.
        /// </summary>
        /// <param name="comparer">The comparer to use when testing for value changes.</param>
        public AutomaticDelegateProperty(IEqualityComparer<T> comparer)
            : this()
        {
            this.Comparer = comparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticDelegateProperty{T}"/> class with a default comparer and the specified delegate.
        /// </summary>
        /// <param name="evaluate">The delegate to execute to re-calculate the value of the property.</param>
        public AutomaticDelegateProperty(Func<T> evaluate)
            : this()
        {
            this.Evaluate = evaluate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticDelegateProperty{T}"/> class with the specified comparer and delegate.
        /// </summary>
        /// <param name="evaluate">The delegate to execute to re-calculate the value of the property.</param>
        /// <param name="comparer">The comparer to use when testing for value changes.</param>
        public AutomaticDelegateProperty(Func<T> evaluate, IEqualityComparer<T> comparer)
            : this()
        {
            this.Evaluate = evaluate;
            this.Comparer = comparer;
        }

        /// <summary>
        /// The event that is invoked when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }

        /// <summary>
        /// Gets the value of the property, as returned by <see cref="Evaluate"/>.
        /// </summary>
        public T Value
        {
            get
            {
                return this.value;
            }

            private set
            {
                if (!this.Comparer.Equals(this.value, value))
                {
                    this.value = value;
                    this.OnPropertyChanged("Value");
                }
            }
        }

        /// <summary>
        /// Gets or sets the delegate used to evaluate this property. Setting this will invoke the new delegate.
        /// </summary>
        public Func<T> Evaluate
        {
            get
            {
                return this.evaluate;
            }

            set
            {
                this.evaluate = value;
                this.OnPropertyChanged("Evaluate");
                this.Update();
            }
        }

        /// <summary>
        /// Gets the comparer that is used to determine the equality of property values.
        /// </summary>
        /// <remarks>
        /// <para>If the comparer was not specified in the constructor, this property is set to <see cref="EqualityComparer{T}.Default"/>.</para>
        /// </remarks>
        public IEqualityComparer<T> Comparer { get; private set; }

        /// <summary>
        /// Updates <see cref="Value"/> by invoking <see cref="Evaluate"/>, using the default value of <paramref name="T"/> if <see cref="Evaluate"/> is null.
        /// </summary>
        /// <remarks>
        /// <para>This method will invoke <see cref="PropertyChanged"/> if the new value is different than the old value, as defined by <see cref="Comparer"/>.</para>
        /// </remarks>
        public void Update()
        {
            // Set to the default value if we don't have a way to calculate the new value
            if (this.evaluate == null)
            {
                this.value = default(T);
                return;
            }

            // Calculate and save the new value
            this.Value = this.evaluate();
        }

        /// <summary>
        /// Cleans up event subscriptions for this <see cref="AutomaticDelegateProperty{T}"/>.
        /// </summary>
        public void Dispose()
        {
            if (this.canExecuteChangedDelegate != null)
            {
                CommandManager.RequerySuggested -= this.canExecuteChangedDelegate;
                this.canExecuteChangedDelegate = null;
            }
        }

        /// <summary>
        /// Invokes <see cref="PropertyChanged"/> for the specified property.
        /// </summary>
        /// <param name="name">The name of the property that changed.</param>
        private void OnPropertyChanged(string name)
        {
            if (this.propertyChanged != null)
            {
                this.propertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
