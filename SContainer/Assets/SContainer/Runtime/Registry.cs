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

            // 一些特殊处理
            if (interfaceType.IsConstructedGenericType)
            {
                var openGenericType = RuntimeTypeCache.OpenGenericTypeOf(interfaceType);
                var typeParameters = RuntimeTypeCache.GenericTypeParametersOf(interfaceType);
                return this.TryFallbackToSingleElementCollection(interfaceType, openGenericType, typeParameters, out registration) ||
                    this.TryFallbackContainerLocal(interfaceType, openGenericType, typeParameters, out registration);
            }
            return false;
        }

        public bool Exists(Type type)
        {
            if (this.hashTable.TryGet(type, out _))
                return true;

            if (type.IsConstructedGenericType)
            {
                type = RuntimeTypeCache.OpenGenericTypeOf(type);
            }

            return this.hashTable.TryGet(type, out _);
        }

        /// <summary>
        /// 只注册了一个（或者没有注册）类型，但解析时用的列表来接收实例化的对象，特殊处理
        /// </summary>
        private bool TryFallbackToSingleElementCollection(
            Type closedGenericType,
            Type openGenericType,
            IReadOnlyList<Type> typeParameters,
            out Registration newRegistration)
        {
            if (CollectionInstanceProvider.Match(openGenericType))
            {
                var elementType = typeParameters[0];
                var collection = new CollectionInstanceProvider(elementType);
                if (this.hashTable.TryGet(elementType, out var elementRegistration) && elementRegistration != null)
                {
                    collection.Add(elementRegistration);
                }
                newRegistration = new Registration(
                    RuntimeTypeCache.ArrayTypeOf(elementType),
                    Lifetime.Transient,
                    new List<Type>
                    {
                        RuntimeTypeCache.EnumerableTypeOf(elementType),
                        RuntimeTypeCache.ReadOnlyListTypeOf(elementType),
                    }, collection);
                return true;
            }
            newRegistration = null;
            return false;
        }

        /// <summary>
        /// ContainerLocal 包装的类型，特殊处理
        /// </summary>
        private bool TryFallbackContainerLocal(
            Type closedGenericType,
            Type openGenericType,
            IReadOnlyList<Type> typeParameters,
            out Registration newRegistration)
        {
            if (openGenericType == typeof(ContainerLocal<>))
            {
                var valueType = typeParameters[0];
                if (this.TryGet(valueType, out var valueRegistration))
                {
                    var spawner = new ContainerLocalInstanceProvider(closedGenericType, valueRegistration);
                    newRegistration = new Registration(closedGenericType, Lifetime.Scoped, null, spawner);
                    return true;
                }
            }
            newRegistration = null;
            return false;
        }
    }
}