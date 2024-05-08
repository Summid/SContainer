using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SContainer.Runtime.Internal
{
    public class OpenGenericInstanceProvider : IInstanceProvider
    {
        private class TypeParametersEqualityCompare : IEqualityComparer<Type[]>
        {
            public bool Equals(Type[] x, Type[] y)
            {
                if (x == null || y == null) return x == y;
                if (x.Length != y.Length) return false;
                
                for (var i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i]) return false;
                }
                return true;
            }
            
            public int GetHashCode(Type[] typeParameters)
            {
                // DJB 算法
                // unsigned long
                // hash(unsigned char *str)
                // {
                //     unsigned long hash = 5381;  // 经过大量实验，5381和33碰撞少
                //     int c;
                //
                //     while (c = *str++)
                //         hash = ((hash << 5) + hash) + c; /* hash * 33 + c */
                //
                //     return hash;
                // }
                var hash = 5381;
                foreach (var typeParameter in typeParameters)
                {
                    hash = ((hash << 5) + hash) ^ typeParameter.GetHashCode();
                }
                return hash;
            }
        }

        private readonly Lifetime lifetime;
        private readonly Type implementationType;
        private readonly IReadOnlyList<IInjectParameter> customParameters;

        private readonly ConcurrentDictionary<Type[], Registration> constructedRegistrations = new ConcurrentDictionary<Type[], Registration>(new TypeParametersEqualityCompare());
        private readonly Func<Type[], Registration> createRegistrationFunc;

        public OpenGenericInstanceProvider(Type implementationType, Lifetime lifetime, List<IInjectParameter> injectParameters)
        {
            this.implementationType = implementationType;
            this.lifetime = lifetime;
            this.customParameters = injectParameters;
            this.createRegistrationFunc = this.CreateRegistration;
        }

        public Registration GetClosedRegistration(Type closedInterfaceType, Type[] typeParameters)
        {
            return this.constructedRegistrations.GetOrAdd(typeParameters, this.createRegistrationFunc);
        }

        private Registration CreateRegistration(Type[] typeParameters)
        {
            var newType = this.implementationType.MakeGenericType(typeParameters);
            var injector = InjectorCache.GetOrBuild(newType);
            var spawner = new InstanceProvider(injector, this.customParameters);
            return new Registration(newType, this.lifetime, new List<Type>(1) { newType }, spawner);
        }
        
        public object SpawnInstance(IObjectResolver resolver)
        {
            // container.Resolve(typeof(GenericClass<>)) is invalid
            throw new InvalidOperationException();
        }
    }
}