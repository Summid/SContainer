using SContainer.Runtime.Internal;
using System;

namespace SContainer.Runtime
{
    public class RegistrationBuilder
    {
        internal protected readonly Type ImplementationType;
        internal protected readonly Lifetime Lifetime;
        
        public RegistrationBuilder(Type implementationType, Lifetime lifetime)
        {
            this.ImplementationType = implementationType;
            this.Lifetime = lifetime;
        }

        public virtual Registration Build()
        {
            var injector = InjectorCache.GetOrBuild(this.ImplementationType);
            var spawner = new InstanceProvider(injector);
            return new Registration(
                this.ImplementationType,
                this.Lifetime,
                spawner);
        }
    }
}