// <copyright file="ManualCommand.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Windows.Input;
    using Nito.Utility;

    /// <summary>
    /// Delegate-based parameterless command, implementing <see cref="CanExecuteChanged"/> as a weak event.
    /// </summary>
    public sealed class ManualCommand : ICommand, IDisposable
    {
        /// <summary>
        /// Implementation of <see cref="CanExecuteChanged"/>.
        /// </summary>
        private CanExecuteChangedCore canExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualCommand"/> class with null delegates.
        /// </summary>
        public ManualCommand()
        {
            this.canExecuteChanged = new CanExecuteChangedCore();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualCommand"/> class with the specified delegates.
        /// </summary>
        /// <param name="execute">The delegate invoked to execute this command.</param>
        /// <param name="canExecute">The delegate invoked to determine if this command may execute. This may be invoked when <see cref="RaiseCanExecuteChanged"/> is invoked.</param>
        public ManualCommand(Action execute, Func<bool> canExecute)
            : this()
        {
            this.Execute = execute;
            this.CanExecute = canExecute;
        }

        /// <summary>
        /// This is a weak event. Provides notification that the result of <see cref="ICommand.CanExecute"/> may be different.
        /// </summary>
        event EventHandler ICommand.CanExecuteChanged
        {
            add { this.canExecuteChanged.CanExecuteChanged += value; }
            remove { this.canExecuteChanged.CanExecuteChanged -= value; }
        }

        /// <summary>
        /// Gets or sets the delegate invoked to execute this command. Setting this does not raise <see cref="CanExecuteChanged"/>.
        /// </summary>
        public Action Execute { get; set; }

        /// <summary>
        /// Gets or sets the delegate invoked to determine if this command may execute. Setting this does not raise <see cref="CanExecuteChanged"/>.
        /// </summary>
        public Func<bool> CanExecute { get; set; }

        /// <summary>
        /// Determines if this command can execute.
        /// </summary>
        /// <param name="parameter">This parameter is ignored.</param>
        /// <returns>Whether this command can execute.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            return this.CanExecute();
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter">This parameter is ignored.</param>
        void ICommand.Execute(object parameter)
        {
            this.Execute();
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event for any listeners still alive, and removes any references to garbage collected listeners.
        /// </summary>
        public void NotifyCanExecuteChanged()
        {
            this.canExecuteChanged.OnCanExecuteChanged();
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