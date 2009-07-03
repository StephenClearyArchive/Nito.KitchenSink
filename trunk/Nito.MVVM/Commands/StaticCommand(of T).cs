// <copyright file="StaticCommand(of T).cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;

    /// <summary>
    /// Delegate-based single-parameter command that may be executed at any time.
    /// </summary>
    /// <typeparam name="T">The type of the parameter for this command.</typeparam>
    public sealed class StaticCommand<T> : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticCommand{T}"/> class with a null delegate.
        /// </summary>
        public StaticCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticCommand{T}"/> class with the specified delegate.
        /// </summary>
        /// <param name="execute">The delegate invoked to execute this command.</param>
        public StaticCommand(Action<T> execute)
        {
            this.Execute = execute;
        }

        /// <summary>
        /// Unused. <see cref="StaticCommand{T}"/> instances may always execute, so this event is never raised.
        /// </summary>
        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Gets or sets the delegate invoked to execute this command. Setting this does not raise <see cref="CanExecuteChanged"/>.
        /// </summary>
        public Action<T> Execute { get; set; }

        /// <summary>
        /// Determines whether this command can be executed. Always returns true, since <see cref="StaticCommand"/> instances may always execute.
        /// </summary>
        /// <param name="parameter">This parameter is ignored.</param>
        /// <returns>Always returns true.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter">The parameter for this command.</param>
        void ICommand.Execute(object parameter)
        {
            this.Execute((T)parameter);
        }
    }
}