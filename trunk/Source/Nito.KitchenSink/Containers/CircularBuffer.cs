// <copyright file="CircularBuffer.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nito.Linq;

    /// <summary>
    /// A circular buffer, which provides O(1) indexed access, O(1) insertions to the back, O(1) removals from the front, and O(N) resizing. Inserting items is not supported.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the deque.</typeparam>
    public sealed class CircularBuffer<T> : Nito.Linq.Implementation.ListBase<T>
    {
        /// <summary>
        /// The actual buffer that holds the view. When setting this, <see cref="view"/> should be set to <c>null</c>.
        /// </summary>
        private T[] buffer;

        /// <summary>
        /// The offset into <see cref="buffer"/> where the view begins. When setting this, <see cref="view"/> should be set to <c>null</c>.
        /// </summary>
        private int offset;

        /// <summary>
        /// The number of valid elements in the view.
        /// </summary>
        private int count;

        /// <summary>
        /// The view, cached in a member variable. This may be null, and should be accessed via the <see cref="View"/> method.
        /// </summary>
        private IList<T> view;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer&lt;T&gt;"/> class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity.</param>
        /// <exception cref="InvalidOperationException">Capacity may not be less than or equal to zero.</exception>
        public CircularBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new InvalidOperationException("Capacity may not be less than or equal to zero.");
            }

            this.buffer = new T[capacity];
        }

        /// <summary>
        /// Gets or sets the capacity for this circular buffer. This value is always greater than zero; setting this property to a negative or zero value will have no effect.
        /// </summary>
        public int Capacity
        {
            get
            {
                return this.buffer.Length;
            }

            set
            {
                if (value <= 0)
                {
                    return;
                }

                if (value == this.buffer.Length)
                {
                    return;
                }

                // Truncate the view if necessary (removing old elements from the front)
                if (value < this.count)
                {
                    this.PostIncrement(this.count - value);
                    this.count = value;
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
        /// Gets the number of valid elements contained in this circular buffer. This value is always greater than or equal to zero.
        /// </summary>
        /// <returns>The number of valid elements contained in this circular buffer.</returns>
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
        public bool IsEmpty
        {
            get { return this.count == 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is at full capacity.
        /// </summary>
        public bool IsFull
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
        /// Adds a single element at the back of this circular buffer. If necessary, an element will be removed from the front of the buffer to make room.
        /// </summary>
        /// <param name="value">The element to add.</param>
        public override void Add(T value)
        {
            if (this.IsFull)
            {
                this.DoRemove();
            }

            this.DoAdd(value);
        }

        /// <summary>
        /// Adds a collection of elements at the back of this circular buffer. If necessary, element(s) will be be removed from the front of the buffer to make room.
        /// </summary>
        /// <param name="collection">The collection of elements to add.</param>
        public void AddRange(IEnumerable<T> collection)
        {
            int collectionCount = collection.Count();

            if (collectionCount == 0)
            {
                return;
            }

            if (collectionCount > this.Capacity)
            {
                // The collection is bigger than this buffer, so we can only accept part of it

                // The collection will fill up the buffer, replacing all elements
                this.offset = 0;
                this.count = this.Capacity;

                // Copy from the collection into the buffer
                int index = 0;
                foreach (T item in collection.Skip(collectionCount - this.Capacity))
                {
                    this.buffer[index] = item;
                    ++index;
                }

                this.view = null;
                return;
            }

            // Trim the existing elements, if necessary
            int elementsToRemove = this.count - this.Capacity + collectionCount;
            if (elementsToRemove > 0)
            {
                this.PostIncrement(elementsToRemove);
                this.count -= elementsToRemove;
            }

            // Add the new elements
            IList<T> view = this.View();
            int i = this.count;
            foreach (T item in collection)
            {
                view[i] = item;
                ++i;
            }

            // Adjust valid count
            this.count += collectionCount;
        }

        /// <summary>
        /// Removes and returns the first element of this circular buffer.
        /// </summary>
        /// <returns>The former first element.</returns>
        /// <exception cref="InvalidOperationException">The circular buffer is empty.</exception>
        public T Remove()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException("The circular buffer is empty.");
            }

            return this.DoRemove();
        }

        /// <summary>
        /// Removes a range of elements from the front of this circular buffer.
        /// </summary>
        /// <param name="count">The number of elements to remove.</param>
        public void RemoveRange(int count)
        {
            if (count >= this.count)
            {
                this.Clear();
            }
            else
            {
                this.PostIncrement(count);
                this.count -= count;
            }
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
            if (index == this.count)
            {
                if (this.IsFull)
                {
                    this.DoRemove();
                }

                this.DoAdd(item);
            }
            else
            {
                throw new InvalidOperationException("Cannot insert into a circular buffer");
            }
        }

        /// <summary>
        /// Removes an element at the specified view index.
        /// </summary>
        /// <param name="index">The zero-based view index of the element to remove. This index is guaranteed to be valid.</param>
        protected override void DoRemoveAt(int index)
        {
            if (index == 0)
            {
                this.Remove();
            }
            else
            {
                throw new InvalidOperationException("Cannot remove from a circular buffer");
            }
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
        /// Inserts a single element to the back of the view. <see cref="IsFull"/> must be false when this method is called.
        /// </summary>
        /// <param name="value">The element to insert.</param>
        private void DoAdd(T value)
        {
            this.View()[this.count] = value;
            ++this.count;
        }

        /// <summary>
        /// Removes and returns the last element in the view. <see cref="IsEmpty"/> must be false when this method is called.
        /// </summary>
        /// <returns>The former last element.</returns>
        private T DoRemove()
        {
            T ret = this.buffer[this.PostIncrement(1)];
            --this.count;
            return ret;
        }
    }
}
