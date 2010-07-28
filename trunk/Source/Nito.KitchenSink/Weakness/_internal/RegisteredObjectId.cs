// <copyright file="RegisteredObjectId.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using Nito.Weakness.ObjectTracking;

    /// <summary>
    /// An <see cref="ObjectId"/> along with a registered action. The action is unregistered when the <see cref="RegisteredObjectId"/> is disposed. This type uses value equality; two instances are considered equal if they refer to the same <see cref="ObjectId"/>.
    /// </summary>
    internal sealed class RegisteredObjectId : IEquatable<RegisteredObjectId>, IDisposable
    {
        /// <summary>
        /// The action registered on <see cref="id"/>.
        /// </summary>
        private readonly Action<ObjectId> registeredAction;

        /// <summary>
        /// The object identifier.
        /// </summary>
        private readonly ObjectId id;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisteredObjectId"/> class, registering the specified action.
        /// </summary>
        /// <param name="id">The object identifier.</param>
        /// <param name="action">The action to register on <paramref cref="id"/>.</param>
        public RegisteredObjectId(ObjectId id, Action<ObjectId> action)
        {
            this.id = id;
            this.registeredAction = id.Register(action);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisteredObjectId"/> class without a registered action.
        /// </summary>
        /// <param name="id">The object identifier.</param>
        public RegisteredObjectId(ObjectId id)
        {
            this.id = id;
        }

        /// <summary>
        /// Gets the object identifier.
        /// </summary>
        public ObjectId Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Unregisters the registered action, if any.
        /// </summary>
        public void Dispose()
        {
            this.id.Unregister(registeredAction);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>. Two instances are considered equal if they are of the same type and refer to the same <see cref="ObjectId"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            var other = obj as RegisteredObjectId;
            if (other == null)
            {
                return false;
            }

            return this.Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type. Two instances are considered equal if they refer to the same <see cref="ObjectId"/>.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(RegisteredObjectId other)
        {
            return this.id == other.id;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }
    }
}
