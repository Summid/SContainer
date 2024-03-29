﻿using SContainer.Runtime.Internal;
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
                if (registration.InterfaceTypes is IReadOnlyList<Type> interfaceTypes) // if registration.InterfaceTypes is not null
                {
                    for (var i = 0; i < interfaceTypes.Count; i++)
                    {
                        AddToBuildBuffer(buildBuffer, interfaceTypes[i], registration);
                    }

                    // Mark the implementationType with a guard because we need to check if it exists later.
                    if (!buildBuffer.ContainsKey(registration.ImplementationType))
                    {
                        buildBuffer.Add(registration.ImplementationType, null);
                    }
                }
                else
                {
                    AddToBuildBuffer(buildBuffer, registration.ImplementationType, registration);
                }
            }

            var hashTable = new FixedTypeKeyHashTable<Registration>(buildBuffer.ToArray());
            return new Registry(hashTable);
        }

        private static void AddToBuildBuffer(IDictionary<Type, Registration> buf, Type service, Registration registration)
        {
            if (buf.TryGetValue(service, out var exists))
            {
                // already add service
                CollectionInstanceProvider collection;
                if (buf.TryGetValue(RuntimeTypeCache.EnumerableTypeOf(service), out var found) &&
                    found.Provider is CollectionInstanceProvider foundProvider)
                {
                    // already add collection provider
                    collection = foundProvider;
                }
                else
                {
                    // encounter first duplicate, add IEnumerable<> instead of typeof(service)
                    collection = new CollectionInstanceProvider(service) { exists }; // invoke collProvider.Add
                    var newRegistration = new Registration(
                        RuntimeTypeCache.ArrayTypeOf(service),
                        Lifetime.Transient,
                        new List<Type>
                        {
                            RuntimeTypeCache.EnumerableTypeOf(service),
                            RuntimeTypeCache.ReadOnlyListTypeOf(service)
                        }, collection);
                    AddCollectionToBuildBuffer(buf, newRegistration);
                }
                collection.Add(registration);

                // Overwritten by the later registration（多次注册同个类型后，如果只解析单个对象，解析出来的是最后注册的对象，注册接口同理）
                buf[service] = registration;
            }
            else
            {
                buf.Add(service, registration);
            }
        }

        private static void AddCollectionToBuildBuffer(IDictionary<Type, Registration> buf, Registration collectionRegistration)
        {
            for (var i = 0; i < collectionRegistration.InterfaceTypes.Count; i++)
            {
                var collectionType = collectionRegistration.InterfaceTypes[i];
                try
                {
                    // Add IEnumerable<typeof<service>>, IReadOnlyList<typeof<service>> to buf as key
                    buf.Add(collectionType, collectionRegistration);
                }
                catch (ArgumentException)
                {
                    throw new SContainerException(collectionType, $"Registration with the same key already exists: {collectionRegistration}");
                }
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