using SContainer.Runtime.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SContainer.Runtime
{
    /// <summary>
    /// Get Registration from Registry which wrapped into this.
    /// </summary>
    public sealed class Registry
    {
        [ThreadStatic]  // this field is unique to every thread
        private static IDictionary<Type, Registration> buildBuffer = new Dictionary<Type, Registration>(128);

        private FixedTypeKeyHashTable<Registration> hashTable;
        
        public static Registry Build(Registration[] registrations)
        {
            if (buildBuffer == null)
                buildBuffer = new Dictionary<Type, Registration>(128);
            buildBuffer.Clear();
            
            foreach (var registration in registrations)
            {
                AddToBuildBuffer(buildBuffer, registration.ImplementationType, registration);
            }

            var hashTable = new FixedTypeKeyHashTable<Registration>(buildBuffer.ToArray());
            return new Registry(hashTable);
        }

        private static void AddToBuildBuffer(IDictionary<Type, Registration> buf, Type service, Registration registration)
        {
            if (buf.TryGetValue(service, out var exists))
            {
                throw new SContainerException(service, $"Duplicate implementation type is not supported currently.");
            }
            else
            {
                buf.Add(service, registration);
            }
        }

        private Registry(FixedTypeKeyHashTable<Registration> hashTable)
        {
            this.hashTable = hashTable;
        }

        public bool TryGet(Type interfaceType, out Registration registration)
        {
            if (this.hashTable.TryGet(interfaceType, out registration))
                return registration != null;

            return false;
        }
    }
}