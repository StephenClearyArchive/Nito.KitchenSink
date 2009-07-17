// <copyright file="MultiCommand.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Collections;
    using System.Windows.Input;
    using Nito.Utility;

    /// <summary>
    /// Represents a multicommand.
    /// </summary>
    /// <remarks>
    /// <para>A multicommand is defined by a <see cref="SourceCollection"/> and a <see cref="Path"/>. Applying the property path to the source collection results in a collection of individual <see cref="ICommand"/> objects, which are treated like child commands.</para>
    /// </remarks>
    public sealed class MultiCommand : ICommand, IDisposable
    {
        /// <summary>
        /// Implementation of <see cref="CanExecuteChanged"/>.
        /// </summary>
        private CanExecuteChangedCore canExecuteChanged = new CanExecuteChangedCore();

        /// <summary>
        /// The source collection, projected along a property path.
        /// </summary>
        private ProjectedCollection collection = new ProjectedCollection();

        /// <summary>
        /// This is a weak event. Provides notification that the result of <see cref="ICommand.CanExecute"/> may be different.
        /// </summary>
        event EventHandler ICommand.CanExecuteChanged
        {
            add { this.canExecuteChanged.CanExecuteChanged += value; }
            remove { this.canExecuteChanged.CanExecuteChanged -= value; }
        }

        /// <summary>
        /// Gets or sets the source collection of this multiproperty. This class is more efficient if the source collection implements <see cref="IList"/> and <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        public IEnumerable SourceCollection
        {
            get
            {
                return this.collection.SourceCollection;
            }

            set
            {
                this.collection.SourceCollection = value;
            }
        }

        /// <summary>
        /// Gets or sets the property path applied to the elements in the source collection.
        /// </summary>
        public string Path
        {
            get
            {
                return this.collection.Path;
            }

            set
            {
                this.collection.Path = value;
            }
        }

        /// <summary>
        /// Determines if this command can execute. This command may execute if it has at least one child command and all of its child commands may execute.
        /// </summary>
        /// <remarks>
        /// <para>If any of the child objects do not implement <see cref="ICommand"/>, this method returns false.</para>
        /// </remarks>
        /// <param name="parameter">This parameter is passed to all child commands.</param>
        /// <returns>Whether this command can execute.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            foreach (object child in this.collection)
            {
                ICommand command = child as ICommand;
                if (command == null)
                {
                    return false;
                }

                if (!command.CanExecute(parameter))
                {
                    return false;
                }
            }

            return this.collection.Count != 0;
        }

        /// <summary>
        /// Executes all child commands. No child commands should be null when this method is invoked.
        /// </summary>
        /// <param name="parameter">This parameter is passed to all child commands.</param>
        void ICommand.Execute(object parameter)
        {
            foreach (object child in this.collection)
            {
                (child as ICommand).Execute(parameter);
            }
        }

        /// <summary>
        /// Frees all weak references to delegates held by <see cref="CanExecuteChanged"/>.
        /// </summary>
        public void Dispose()
        {
            this.canExecuteChanged.Dispose();
            this.collection.Dispose();
        }
    }
}
