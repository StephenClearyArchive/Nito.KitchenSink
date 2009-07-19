// <copyright file="AutomaticCommand.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;

    /// <summary>
    /// Delegate-based parameterless command that ties in to <see cref="CommandManager.RequerySuggested"/>.
    /// </summary>
    public sealed class AutomaticCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticCommand"/> class with null delegates.
        /// </summary>
        public AutomaticCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomaticCommand"/> class with the specified delegates.
        /// </summary>
        /// <param name="execute">The delegate invoked to execute this command.</param>
        /// <param name="canExecute">The delegate invoked to determine if this command may execute. This is invoked when <see cref="CommandManager.RequerySuggested"/> is raised.</param>
        public AutomaticCommand(Action execute, Func<bool> canExecute)
        {
            this.Execute = execute;
            this.CanExecute = canExecute;
        }

        /// <summary>
        /// This is a weak event. Provides notification that the result of <see cref="ICommand.CanExecute"/> may be different.
        /// </summary>
        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
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
    }
}