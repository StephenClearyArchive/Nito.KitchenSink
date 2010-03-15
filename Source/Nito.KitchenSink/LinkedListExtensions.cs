// <copyright file="LinkedListExtensions.cs" company="Nito Programs">
//     Copyright (c) 2009-2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink
{
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods for <see cref="LinkedList{T}"/>.
    /// </summary>
    public static class LinkedListExtensions
    {
        /// <summary>
        /// The nodes in this linked list, as a sequence. The nodes enumerated by this sequence are safe to pass to <see cref="LinkedList{T}.Remove(LinkedListNode{T})"/> without disturbing the sequence iteration.
        /// </summary>
        /// <typeparam name="T">The type of elements contained in the linked list.</typeparam>
        /// <param name="list">The linked list.</param>
        /// <returns>A sequence containing each node in this linked list.</returns>
        public static IEnumerable<LinkedListNode<T>> Nodes<T>(this LinkedList<T> list)
        {
            LinkedListNode<T> node = list.First;
            while (node != null)
            {
                LinkedListNode<T> next = node.Next;
                yield return node;
                node = next;
            }
        }
    }
}
