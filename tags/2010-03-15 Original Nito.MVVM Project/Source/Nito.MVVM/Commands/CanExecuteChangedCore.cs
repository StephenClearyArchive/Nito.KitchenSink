// <copyright file="CanExecuteChangedCore.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Windows.Input;
    using Nito.Utility;

    /// <summary>
    /// Class that implements <see cref="CanExecuteChanged"/> as a weak event.
    /// </summary>
    public sealed class CanExecuteChangedCore : IDisposable
    {
        /// <summary>
        /// The weak collection of delegates for <see cref="CanExecuteChanged"/>.
        /// </summary>
        private IWeakCollection<EventHandler> canExecuteChanged = new WeakCollection<EventHandler>();

        /// <summary>
        /// This is a weak event. Provides notification that the result of <see cref="ICommand.CanExecute"/> may be different.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { this.canExecuteChanged.Add(value); }
            remove { this.canExecuteChanged.Remove(value); }
        }

        /// <summary>
        /// Frees all weak references to delegates held by <see cref="CanExecuteChanged"/>.
        /// </summary>
        public void Dispose()
        {
            this.canExecuteChanged.Dispose();
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event for any listeners still alive, and removes any references to garbage collected listeners.
        /// </summary>
        public void OnCanExecuteChanged()
        {
            foreach (EventHandler cb in this.canExecuteChanged.LiveList)
            {
                cb(this, EventArgs.Empty);
            }
        }
    }
}
