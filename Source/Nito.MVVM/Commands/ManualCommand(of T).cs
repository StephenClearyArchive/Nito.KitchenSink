// <copyright file="ManualCommand(of T).cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Windows.Input;
    using Nito.Utility;

    /// <summary>
    /// Delegate-based single-parameter command with its own backing member for <see cref="CanExecuteChanged"/>.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for this command.</typeparam>
    public sealed class ManualCommand<T> : ICommand, IDisposable
    {
        /// <summary>
        /// The weak collection of delegates for <see cref="CanExecuteChanged"/>.
        /// </summary>
        private IWeakCollection<EventHandler> canExecuteChanged = new WeakCollection<EventHandler>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualCommand{T}"/> class with null delegates.
        /// </summary>
        public ManualCommand()
        {
            this.CanExecuteChangedSubscription = (sender, e) => this.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualCommand{T}"/> class with the specified delegates.
        /// </summary>
        /// <param name="execute">The delegate invoked to execute this command.</param>
        /// <param name="canExecute">The delegate invoked to determine if this command may execute. This may be invoked when <see cref="RaiseCanExecuteChanged"/> is invoked.</param>
        public ManualCommand(Action<T> execute, Func<T, bool> canExecute)
            : this()
        {
            this.Execute = execute;
            this.CanExecute = canExecute;
        }

        /// <summary>
        /// This is a weak event. Provides notification that the result of <see cref="ICommand.CanExecute"/> may be different.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { this.canExecuteChanged.Add(value); }
            remove { this.canExecuteChanged.Remove(value); }
        }

        /// <summary>
        /// Gets or sets the delegate invoked to execute this command. Setting this does not raise <see cref="CanExecuteChanged"/>.
        /// </summary>
        public Action<T> Execute { get; set; }

        /// <summary>
        /// Gets or sets the delegate invoked to determine if this command may execute. Setting this does not raise <see cref="CanExecuteChanged"/>.
        /// </summary>
        public Func<T, bool> CanExecute { get; set; }

        /// <summary>
        /// Gets a delegate that invokes <see cref="NotifyCanExecuteChanged"/>. This delegate exists for the lifetime of this command.
        /// </summary>
        public EventHandler CanExecuteChangedSubscription { get; private set; }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event for any listeners still alive, and removes any references to garbage collected listeners.
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            foreach (EventHandler cb in this.canExecuteChanged.LiveList)
            {
                cb(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Determines if this command can execute.
        /// </summary>
        /// <param name="parameter">The parameter for this command.</param>
        /// <returns>Whether this command can execute.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute((T)parameter);
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter">The parameter for this command.</param>
        void ICommand.Execute(object parameter)
        {
            this.Execute((T)parameter);
        }

        /// <summary>
        /// Frees all weak references to delegates held by <see cref="CanExecuteChanged"/>.
        /// </summary>
        public void Dispose()
        {
            this.canExecuteChanged.Dispose();
        }
    }
}