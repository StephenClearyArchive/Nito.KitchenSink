// <copyright file="MultiCommand.cs" company="Nito Programs">
//     Copyright (c) 2009 Nito Programs.
// </copyright>

namespace Nito.MVVM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
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
        /// The subscription to child <see cref="ICommand.CanExecuteChanged"/> events.
        /// </summary>
        private EventHandler commandSubscription;

        /// <summary>
        /// All the child commands we've subscribed to. Null entries represent children that do not implement <see cref="ICommand"/>.
        /// </summary>
        private List<ICommand> subscribedCommands = new List<ICommand>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiCommand"/> class.
        /// </summary>
        public MultiCommand()
        {
            this.collection.CollectionChanged += (sender, e) => this.ProcessSourceCollectionChanged(e);
            this.commandSubscription = (sender, e) => this.canExecuteChanged.OnCanExecuteChanged();
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
        /// Frees all weak references to delegates held by <see cref="CanExecuteChanged"/> and unsubscribes from all collection and property notifications.
        /// </summary>
        public void Dispose()
        {
            foreach (ICommand command in this.subscribedCommands.Where(x => x != null))
            {
                command.CanExecuteChanged -= this.commandSubscription;
            }

            this.canExecuteChanged.Dispose();
            this.collection.Dispose();
        }

        /// <summary>
        /// Adds a list of children to the subscribed list, and subscribes to them if possible.
        /// </summary>
        /// <param name="index">The index at which to add the new children.</param>
        /// <param name="children">The new children to add.</param>
        private void AddChildren(int index, IList children)
        {
            this.subscribedCommands.Capacity = this.subscribedCommands.Count + children.Count;
            this.subscribedCommands.InsertRange(index, children.Cast<object>().Select(x => x as ICommand));
            foreach (ICommand command in children.OfType<ICommand>())
            {
                command.CanExecuteChanged += this.commandSubscription;
            }
        }

        /// <summary>
        /// Removes a list of children from the subscribed list, and unsubscribes from them if possible.
        /// </summary>
        /// <param name="index">The index from which to remove the children.</param>
        /// <param name="children">The children to remove.</param>
        private void RemoveChildren(int index, IList children)
        {
            this.subscribedCommands.RemoveRange(index, children.Count);
            foreach (ICommand command in children.OfType<ICommand>())
            {
                command.CanExecuteChanged -= this.commandSubscription;
            }
        }

        /// <summary>
        /// Unsubscribes from all children in the subscribed list, and rebuilds it from the soruce collection.
        /// </summary>
        private void ResetChildren()
        {
            foreach (ICommand command in this.subscribedCommands.Where(x => x != null))
            {
                command.CanExecuteChanged -= this.commandSubscription;
            }

            this.subscribedCommands.Clear();
            this.subscribedCommands.Capacity = this.collection.Count;

            foreach (ICommand command in this.collection.Cast<object>().Select(x => x as ICommand))
            {
                this.subscribedCommands.Add(command);
                if (command != null)
                {
                    command.CanExecuteChanged += this.commandSubscription;
                }
            }
        }

        /// <summary>
        /// Process source collection changes.
        /// </summary>
        /// <param name="e">What changed and how.</param>
        private void ProcessSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // Note: ProjectedCollection always provides proper index values.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.AddChildren(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    this.subscribedCommands.RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    this.subscribedCommands.InsertRange(e.NewStartingIndex, e.NewItems.Cast<object>().Select(x => x as ICommand));
                    return;
                case NotifyCollectionChangedAction.Remove:
                    this.RemoveChildren(e.OldStartingIndex, e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this.RemoveChildren(e.OldStartingIndex, e.OldItems);
                    this.AddChildren(e.NewStartingIndex, e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.ResetChildren();
                    break;
            }

            this.canExecuteChanged.OnCanExecuteChanged();
        }
    }
}
