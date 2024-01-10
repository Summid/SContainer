using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime
{
    /// <summary>
    /// ContainerBuilder.Configure => registry Type via RegistrationBuilders => ContainerBuilder.Build =>
    /// RegistrationBuilder.Build => wrap all registrations into Registry
    /// </summary>
    public sealed class Registration
    {
        public readonly Type ImplementationType;
        public readonly Lifetime Lifetime;
        public readonly IInstanceProvider Provider;
        
        public Registration(
            Type implementationType,
            Lifetime lifetime,
            IInstanceProvider provider)
        {
            this.ImplementationType = implementationType;
            this.Lifetime = lifetime;
            this.Provider = provider;
        }

        public override string ToString()
        {
            return $"Registration {this.ImplementationType.Name} {this.Lifetime} {this.Provider}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => this.Provider.SpawnInstance(resolver);
    }
}