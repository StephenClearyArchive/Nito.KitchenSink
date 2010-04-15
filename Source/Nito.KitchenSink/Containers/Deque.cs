// <copyright file="Deque.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nito.Linq;

    /// <summary>
    /// A double-ended queue (deque), which provides O(1) indexed access, O(1) removals from the front and back, amortized O(1) insertions to the front and back, and O(N) insertions and removals anywhere else (with the operations getting slower as the index approaches the middle).
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the deque.</typeparam>
    public sealed class Deque<T> : Nito.Linq.Implementation.ListBase<T>
    {
        /// <summary>
        /// The default capacity.
        /// </summary>
        private const int DefaultCapacity = 8;

        /// <summary>
        /// The circular buffer that holds the view. When setting this, <see cref="view"/> should be set to <c>null</c>.
        /// </summary>
        private T[] buffer;

        /// <summary>
        /// The offset into <see cref="buffer"/> where the view begins. When setting this, <see cref="view"/> should be set to <c>null</c>.
        /// </summary>
        private int offset;

        /// <summary>
        /// The number of elements in the view.
        /// </summary>
        private int count;

        /// <summary>
        /// The view, cached in a member variable. This may be null, and should be accessed via the <see cref="View"/> method.
        /// </summary>
        private IList<T> view;

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity.</param>
        public Deque(int capacity)
        {
            this.buffer = new T[capacity];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class with the elements from the specified collection.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public Deque(IEnumerable<T> collection)
        {
            int count = collection.Count();
            if (count > 0)
            {
                this.buffer = new T[count];
                this.DoInsertRange(0, collection, count);
            }
            else
            {
                this.buffer = new T[DefaultCapacity];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque&lt;T&gt;"/> class.
        /// </summary>
        public Deque()
            : this(DefaultCapacity)
        {
        }

        /// <summary>
        /// Gets or sets the capacity for this deque. This value is always greater than zero; setting it to a value of zero will set it to the default capacity.
        /// </summary>
        /// <exception cref="InvalidOperationException"><c>Capacity</c> cannot be set to a value less than <see cref="Count"/>.</exception>
        public int Capacity
        {
            get
            {
                return this.buffer.Length;
            }

            set
            {
                if (value < this.count)
                {
                    throw new InvalidOperationException("Capacity cannot be set to a value less than Count");
                }

                if (value == 0)
                {
                    value = DefaultCapacity;
                }

                if (value == this.buffer.Length)
                {
                    return;
                }

                // Create the new buffer and copy our existing range.
                T[] newBuffer = new T[value];
                if (this.IsSplit)
                {
                    // The existing buffer is split, so we have to copy it in parts
                    int length = this.Capacity - this.offset;
                    Array.Copy(this.buffer, this.offset, newBuffer, 0, length);
                    Array.Copy(this.buffer, 0, newBuffer, length, this.count - length);
                }
                else
                {
                    // The existing buffer is whole
                    Array.Copy(this.buffer, this.offset, newBuffer, 0, this.count);
                }

                // Set up to use the new buffer.
                this.buffer = newBuffer;
                this.offset = 0;
                this.view = null;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in this deque.
        /// </summary>
        /// <returns>The number of elements contained in this deque.</returns>
        public override int Count
        {
            get
            {
                return this.count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        private bool IsEmpty
        {
            get { return this.count == 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is at full capacity.
        /// </summary>
        private bool IsFull
        {
            get { return this.count == this.Capacity; }
        }

        /// <summary>
        /// Gets a value indicating whether the buffer is "split" (meaning the beginning of the view is at a later index in <see cref="buffer"/> than the end).
        /// </summary>
        private bool IsSplit
        {
            get { return this.offset > (this.Capacity - this.count); }
        }

        /// <summary>
        /// Inserts a single element at the back of this deque.
        /// </summary>
        /// <param name="value">The element to insert.</param>
        public void AddToBack(T value)
        {
            this.EnsureCapacityForOneElement();
            this.DoAddToBack(value);
        }

        /// <summary>
        /// Inserts a single element at the front of this deque.
        /// </summary>
        /// <param name="value">The element to insert.</param>
        public void AddToFront(T value)
        {
            this.EnsureCapacityForOneElement();
            this.DoAddToFront(value);
        }

        /// <summary>
        /// Inserts a collection of elements into this deque.
        /// </summary>
        /// <param name="index">The index at which the collection is inserted.</param>
        /// <param name="collection">The collection of elements to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index to an insertion point for the source.</exception>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            int collectionCount = collection.Count();
            Nito.Linq.Implementation.ListHelper.CheckNewIndexArgument(this.count, index);

            // Overflow-safe check for "this.Count + collectionCount > this.Capacity"
            if (collectionCount > this.Capacity - this.count)
            {
                this.Capacity = checked(this.Count + collectionCount);
            }

            if (collectionCount == 0)
            {
                return;
            }

            this.DoInsertRange(index, collection, collectionCount);
        }

        /// <summary>
        /// Removes a range of elements from this deque.
        /// </summary>
        /// <param name="offset">The index into the deque at which the range begins.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Either <paramref name="offset"/> or <paramref name="count"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The range [<paramref name="offset"/>, <paramref name="offset"/> + <paramref name="count"/>) is not within the range [0, <see cref="Count"/>).</exception>
        public void RemoveRange(int offset, int count)
        {
            Nito.Linq.Implementation.ListHelper.CheckRangeArguments(this.count, offset, count);

            if (count == 0)
            {
                return;
            }

            this.DoRemoveRange(offset, count);
        }

        /// <summary>
        /// Removes and returns the last element of this deque.
        /// </summary>
        /// <returns>The former last element.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        public T RemoveFromBack()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException("The deque is empty.");
            }

            return this.DoRemoveFromBack();
        }

        /// <summary>
        /// Removes and returns the first element of this deque.
        /// </summary>
        /// <returns>The former first element.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        public T RemoveFromFront()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException("The deque is empty.");
            }

            return this.DoRemoveFromFront();
        }

        /// <summary>
        /// Removes all items from this deque.
        /// </summary>
        public override void Clear()
        {
            this.offset = 0;
            this.count = 0;
            this.view = null;
        }

        /// <summary>
        /// Gets an element at the specified view index.
        /// </summary>
        /// <param name="index">The zero-based view index of the element to get. This index is guaranteed to be valid.</param>
        /// <returns>The element at the specified index.</returns>
        protected override T DoGetItem(int index)
        {
            return this.View()[index];
        }

        /// <summary>
        /// Sets an element at the specified view index.
        /// </summary>
        /// <param name="index">The zero-based view index of the element to get. This index is guaranteed to be valid.</param>
        /// <param name="item">The element to store in the list.</param>
        protected override void DoSetItem(int index, T item)
        {
            this.View()[index] = item;
        }

        /// <summary>
        /// Inserts an element at the specified view index.
        /// </summary>
        /// <param name="index">The zero-based view index at which the element should be inserted. This index is guaranteed to be valid.</param>
        /// <param name="item">The element to store in the list.</param>
        protected override void DoInsert(int index, T item)
        {
            this.EnsureCapacityForOneElement();

            if (index == 0)
            {
                this.DoAddToFront(item);
                return;
            }
            else if (index == this.count)
            {
                this.DoAddToBack(item);
                return;
            }

            this.DoInsertRange(index, EnumerableSource.Return(item), 1);
        }

        /// <summary>
        /// Removes an element at the specified view index.
        /// </summary>
        /// <param name="index">The zero-based view index of the element to remove. This index is guaranteed to be valid.</param>
        protected override void DoRemoveAt(int index)
        {
            if (index == 0)
            {
                this.DoRemoveFromFront();
                return;
            }
            else if (index == this.count - 1)
            {
                this.DoRemoveFromBack();
                return;
            }

            this.DoRemoveRange(index, 1);
        }

        /// <summary>
        /// Retrieves the rotated view for this deque, with a length equal to <see cref="Capacity"/>. The last <c><see cref="Capacity"/> - <see cref="Count"/></c> elements of this view are not valid elements.
        /// </summary>
        /// <returns>The rotated view of the deque.</returns>
        private IList<T> View()
        {
            if (this.view == null)
            {
                this.view = this.buffer.Rotate(this.offset);
            }

            return this.view;
        }

        /// <summary>
        /// Increments <see cref="offset"/> by <paramref name="value"/> using modulo-<see cref="Capacity"/> arithmetic.
        /// </summary>
        /// <param name="value">The value by which to increase <see cref="offset"/>. May not be negative.</param>
        /// <returns>The value of <see cref="offset"/> after it was incremented.</returns>
        private int PostIncrement(int value)
        {
            int ret = this.offset;
            this.offset += value;
            this.offset %= this.Capacity;
            this.view = null;
            return ret;
        }

        /// <summary>
        /// Decrements <see cref="offset"/> by <paramref name="value"/> using modulo-<see cref="Capacity"/> arithmetic.
        /// </summary>
        /// <param name="value">The value by which to reduce <see cref="offset"/>. May not be negative or greater than <see cref="Capacity"/>.</param>
        /// <returns>The value of <see cref="offset"/> before it was decremented.</returns>
        private int PreDecrement(int value)
        {
            this.offset -= value;
            if (this.offset < 0)
            {
                this.offset += this.Capacity;
            }

            this.view = null;
            return this.offset;
        }

        /// <summary>
        /// Inserts a single element to the back of the view. <see cref="IsFull"/> must be false when this method is called.
        /// </summary>
        /// <param name="value">The element to insert.</param>
        private void DoAddToBack(T value)
        {
            this.View()[this.count] = value;
            ++this.count;
        }

        /// <summary>
        /// Inserts a single element to the front of the view. <see cref="IsFull"/> must be false when this method is called.
        /// </summary>
        /// <param name="value">The element to insert.</param>
        private void DoAddToFront(T value)
        {
            this.buffer[this.PreDecrement(1)] = value;
            ++this.count;
        }

        /// <summary>
        /// Removes and returns the last element in the view. <see cref="IsEmpty"/> must be false when this method is called.
        /// </summary>
        /// <returns>The former last element.</returns>
        private T DoRemoveFromBack()
        {
            T ret = this.View()[this.count - 1];
            --this.count;
            return ret;
        }

        /// <summary>
        /// Removes and returns the first element in the view. <see cref="IsEmpty"/> must be false when this method is called.
        /// </summary>
        /// <returns>The former first element.</returns>
        private T DoRemoveFromFront()
        {
            --this.count;
            return this.buffer[this.PostIncrement(1)];
        }

        /// <summary>
        /// Inserts a range of elements into the view.
        /// </summary>
        /// <param name="index">The index into the view at which the elements are to be inserted.</param>
        /// <param name="collection">The elements to insert.</param>
        /// <param name="collectionCount">The number of elements in <paramref name="collection"/>. Must be greater than zero, and the sum of <paramref name="collectionCount"/> and <see cref="Count"/> must be less than or equal to <see cref="Capacity"/>.</param>
        private void DoInsertRange(int index, IEnumerable<T> collection, int collectionCount)
        {
            // Make room in the existing list
            IList<T> view = this.View();
            if (index < this.count / 2)
            {
                // Inserting into the first half of the list

                // Move lower items down: [0, index) -> [Capacity - collectionCount, Capacity - collectionCount + index)
                // This clears out the low "index" number of items, moving them "collectionCount" places down;
                //   after rotation, there will be a "collectionCount"-sized hole at "index".
                view.CopyTo(0, view, this.Capacity - collectionCount, index);

                // Rotate to the new view
                this.PreDecrement(collectionCount);
                view = this.View();
            }
            else
            {
                // Inserting into the second half of the list

                // Move higher items up: [index, count) -> [index + collectionCount, collectionCount + count)
                view.CopyBackward(index, view, index + collectionCount, this.count - index);
            }

            // Copy new items into place
            int i = index;
            foreach (T item in collection)
            {
                view[i] = item;
                ++i;
            }

            // Adjust valid count
            this.count += collectionCount;
        }

        /// <summary>
        /// Removes a range of elements from the view.
        /// </summary>
        /// <param name="index">The index into the view at which the range begins.</param>
        /// <param name="collectionCount">The number of elements in the range. This must be greater than 0 and less than or equal to <see cref="Count"/>.</param>
        private void DoRemoveRange(int index, int collectionCount)
        {
            if (index == 0)
            {
                // Removing from the beginning: rotate to the new view
                this.PostIncrement(collectionCount);
                this.count -= collectionCount;
                return;
            }
            else if (index == this.count - collectionCount)
            {
                // Removing from the ending: trim the existing view
                this.count -= collectionCount;
                return;
            }

            IList<T> view = this.View();
            if ((index + (collectionCount / 2)) < this.count / 2)
            {
                // Removing from first half of list

                // Move lower items up: [0, index) -> [collectionCount, collectionCount + index)
                view.CopyBackward(0, view, collectionCount, index);

                // Rotate to new view
                this.PostIncrement(collectionCount);
            }
            else
            {
                // Removing from second half of list

                // Move higher items down: [index + collectionCount, count) -> [index, count - collectionCount)
                view.CopyTo(index + collectionCount, view, index, this.count - collectionCount - index);
            }

            // Adjust valid count
            this.count -= collectionCount;
        }

        /// <summary>
        /// Doubles the capacity if necessary to make room for one more element. When this method returns, <see cref="IsFull"/> is false.
        /// </summary>
        private void EnsureCapacityForOneElement()
        {
            if (this.IsFull)
            {
                this.Capacity = this.Capacity * 2;
            }
        }
    }
}
