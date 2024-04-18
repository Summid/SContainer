using System;
using System.Collections;
using System.Collections.Generic;

namespace SContainer.Runtime.Internal
{
    internal sealed class CollectionInstanceProvider : IInstanceProvider, IEnumerable<Registration>
    {
        public static bool Match(Type openGenericType) => openGenericType == typeof(IEnumerable<>) ||
                                                          openGenericType == typeof(IReadOnlyList<>);
        
        public IEnumerator<Registration> GetEnumerator() => this.registrations.GetEnumerator();
        IEnumerator<Registration> IEnumerable<Registration>.GetEnumerator() => this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        
        public Type ImplementationType { get; }
        public IReadOnlyList<Type> InterfaceTypes => this.interfaceTypes;
        public Lifetime Lifetime => Lifetime.Transient; // Collection reference is transient. So its members can have each lifetimes.
        
        public Type ElementType { get; }

        private readonly List<Type> interfaceTypes;
        private readonly List<Registration> registrations = new List<Registration>();

        public CollectionInstanceProvider(Type elementType)
        {
            this.ElementType = elementType;
            this.ImplementationType = elementType.MakeArrayType();
            this.interfaceTypes = new List<Type>
            {
                RuntimeTypeCache.EnumerableTypeOf(elementType),
                RuntimeTypeCache.ReadOnlyListTypeOf(elementType)
            };
        }
        
        public override string ToString()
        {
            var contractTypes = this.InterfaceTypes != null ? string.Join(", ", this.InterfaceTypes) : "";
            return $"CollectionRegistration {this.ImplementationType} ContractTypes=[{contractTypes}] {this.Lifetime}";
        }

        public void Add(Registration registration)
        {
            foreach (var x in this.registrations)
            {
                if (x.Lifetime == Lifetime.Singleton && x.ImplementationType == registration.ImplementationType)
                {
                    throw new SContainerException(registration.ImplementationType, $"Conflict implementation type: {registration}");
                }
            }
            this.registrations.Add(registration);
        }
        
        public object SpawnInstance(IObjectResolver resolver)
        {
            if (resolver is IScopedObjectResolver scope)
            {
                var entirelyRegistrations = this.CollectFromParentScopes(scope);
                return this.SpawnInstance(resolver, entirelyRegistrations);
            }
            return this.SpawnInstance(resolver, this.registrations);
        }

        internal object SpawnInstance(IObjectResolver resolver, IReadOnlyList<Registration> registrations)
        {
            var array = Array.CreateInstance(this.ElementType, registrations.Count);
            for (var i = 0; i < registrations.Count; i++)
            {
                array.SetValue(resolver.Resolve(registrations[i]), i);
            }
            return array;
        }

        internal List<Registration> CollectFromParentScopes(IScopedObjectResolver scope, bool localScopeOnly = false)
        {
            if (scope.Parent == null)
            {
                return this.registrations;
            }

            var finderType = this.InterfaceTypes[0];
            List<Registration> mergedRegistrations = null;

            scope = scope.Parent;
            while (scope != null)
            {
                if (scope.TryGetRegistration(finderType, out var registration) &&
                    registration.Provider is CollectionInstanceProvider parentCollection)
                {
                    if (mergedRegistrations == null)
                    {
                        // TODO: object pooling
                        mergedRegistrations = new List<Registration>(this.registrations);
                    }

                    if (localScopeOnly)
                    {
                        foreach (var x in parentCollection.registrations)
                        {
                            if (x.Lifetime != Lifetime.Singleton)
                            {
                                mergedRegistrations.Add(x);
                            }
                        }
                    }
                    else
                    {
                        mergedRegistrations.AddRange(parentCollection.registrations);
                    }
                }
                scope = scope.Parent;
            }
            return mergedRegistrations ?? this.registrations;
        }
    }
}