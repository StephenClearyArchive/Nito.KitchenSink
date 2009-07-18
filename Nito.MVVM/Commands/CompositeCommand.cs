using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Nito.MVVM
{
    /// <summary>
    /// Represents a composite command.
    /// </summary>
    /// <remarks>
    /// <para>A composite command contains an ordered list of child commands.</para>
    /// </remarks>
    public sealed class CompositeCommand : ICommand, IList<ICommand>, IDisposable
    {
        /// <summary>
        /// The list of child commands.
        /// </summary>
        private List<ICommand> list = new List<ICommand>();

        /// <summary>
        /// Implementation of <see cref="CanExecuteChanged"/>.
        /// </summary>
        private CanExecuteChangedCore canExecuteChanged = new CanExecuteChangedCore();

        /// <summary>
        /// The subscription for child commands.
        /// </summary>
        private EventHandler childSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeCommand"/> class.
        /// </summary>
        public CompositeCommand()
        {
            this.childSubscription = (sender, e) => this.canExecuteChanged.OnCanExecuteChanged();
        }
        
        /// <summary>
        /// Determines the index of a child command.
        /// </summary>
        /// <param name="item">The child command to find.</param>
        /// <returns>The index of the child command.</returns>
        public int IndexOf(ICommand item)
        {
            return this.list.IndexOf(item);
        }

        /// <summary>
        /// Inserts a child command into the child command list.
        /// </summary>
        /// <param name="index">The index at which to insert the child command.</param>
        /// <param name="item">The child command to insert. This may not be null.</param>
        public void Insert(int index, ICommand item)
        {
            this.list.Insert(index, item);
            item.CanExecuteChanged += this.childSubscription;
            this.canExecuteChanged.OnCanExecuteChanged();
        }

        /// <summary>
        /// Removes a child command from the child command list.
        /// </summary>
        /// <param name="index">The index of the child command to remove.</param>
        public void RemoveAt(int index)
        {
            ICommand item = null;
            if (index >= 0 && index < this.list.Count)
            {
                item = this.list[index];
            }

            this.list.RemoveAt(index);
            item.CanExecuteChanged -= this.childSubscription;
            this.canExecuteChanged.OnCanExecuteChanged();
        }

        /// <summary>
        /// Gets or sets the child command at a given index.
        /// </summary>
        /// <param name="index">The index of the child command.</param>
        /// <returns>The child command at that index.</returns>
        public ICommand this[int index]
        {
            get
            {
                return this.list[index];
            }

            set
            {
                this.list[index] = value;
            }
        }

        /// <summary>
        /// Adds a child command to the child command list.
        /// </summary>
        /// <param name="item">The child command to add. May not be null.</param>
        public void Add(ICommand item)
        {
            this.list.Add(item);
            item.CanExecuteChanged += this.childSubscription;
            this.canExecuteChanged.OnCanExecuteChanged();
        }

        /// <summary>
        /// Removes all child commands from the child command list.
        /// </summary>
        public void Clear()
        {
            foreach (ICommand command in this.list)
            {
                command.CanExecuteChanged -= this.childSubscription;
            }

            this.list.Clear();
            this.canExecuteChanged.OnCanExecuteChanged();
        }

        /// <summary>
        /// Determines whether a child command is contained in the child command list.
        /// </summary>
        /// <param name="item">The child command to find.</param>
        /// <returns>Whether the child command is in the child command list.</returns>
        bool ICollection<ICommand>.Contains(ICommand item)
        {
            return this.list.Contains(item);
        }

        /// <summary>
        /// Copies the child command list to an array.
        /// </summary>
        /// <param name="array">The array to which to copy the list.</param>
        /// <param name="arrayIndex">The index in the array to start copying.</param>
        void ICollection<ICommand>.CopyTo(ICommand[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of child commands in the child command list.
        /// </summary>
        public int Count
        {
            get { return this.list.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the child command list is read-only. Always returns false.
        /// </summary>
        bool ICollection<ICommand>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes a child command from the child command list.
        /// </summary>
        /// <param name="item">The child command to remove.</param>
        /// <returns>Whether the child command was found and removed.</returns>
        public bool Remove(ICommand item)
        {
            bool ret = false;

            if (this.list.Remove(item))
            {
                item.CanExecuteChanged -= this.childSubscription;
                ret = true;
            }

            this.canExecuteChanged.OnCanExecuteChanged();
            return ret;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the child command list.
        /// </summary>
        /// <returns>An enumerator that iterates through the child command list.</returns>
        public IEnumerator<ICommand> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the child command list.
        /// </summary>
        /// <returns>An enumerator that iterates through the child command list.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Determines whether this command can execute. It can execute if there is at least one child command and all child commands can execute.
        /// </summary>
        /// <param name="parameter">The parameter passed to child commands.</param>
        /// <returns>Whether this command can execute.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            foreach (ICommand command in this.list)
            {
                if (!command.CanExecute(parameter))
                {
                    return false;
                }
            }

            return this.list.Count != 0;
        }

        /// <summary>
        /// This is a weak event. Provides notification that the result of <see cref="CanExecute"/> may be different.
        /// </summary>
        event EventHandler ICommand.CanExecuteChanged
        {
            add { this.canExecuteChanged.CanExecuteChanged += value; }
            remove { this.canExecuteChanged.CanExecuteChanged -= value; }
        }

        /// <summary>
        /// Executes all child commands.
        /// </summary>
        /// <param name="parameter">The parameter that is passed to each child command.</param>
        void ICommand.Execute(object parameter)
        {
            foreach (ICommand command in this.list)
            {
                command.Execute(parameter);
            }
        }

        /// <summary>
        /// Frees all weak references to delegates for <see cref="CanExecuteChanged"/> and removes all subscriptions to child commands.
        /// </summary>
        public void Dispose()
        {
            foreach (ICommand command in this.list)
            {
                command.CanExecuteChanged -= this.childSubscription;
            }

            this.list.Clear();
            this.canExecuteChanged.Dispose();
        }
    }
}
