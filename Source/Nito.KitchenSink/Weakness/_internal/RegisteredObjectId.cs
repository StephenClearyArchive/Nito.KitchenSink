// <copyright file="RegisteredObjectId.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.Weakness
{
    using System;
    using Nito.Weakness.ObjectTracking;

    internal sealed class RegisteredObjectId : IEquatable<RegisteredObjectId>, IDisposable
    {
        private readonly Action<ObjectId> registeredAction;

        private readonly ObjectId id;

        public RegisteredObjectId(ObjectId id, Action<ObjectId> action)
        {
            this.id = id;
            this.registeredAction = id.Register(action);
        }

        public RegisteredObjectId(ObjectId id)
        {
            this.id = id;
        }

        public ObjectId Id
        {
            get { return this.id; }
        }

        public void Dispose()
        {
            this.id.Unregister(registeredAction);
        }

        public override bool Equals(object obj)
        {
            var other = obj as RegisteredObjectId;
            if (other == null)
            {
                return false;
            }

            return this.Equals(other);
        }

        public bool Equals(RegisteredObjectId other)
        {
            return this.id == other.id;
        }

        public override int GetHashCode()
        {
            return this.id.GetHashCode();
        }
    }
}
