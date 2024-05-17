using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime
{
    /// <summary>
    /// ContainerBuilder.Configure => register Type via RegistrationBuilders => ContainerBuilder.Build =>
    /// RegistrationBuilder.Build => wrap all registrations into Registry
    /// </summary>
    public sealed class Registration
    {
        public readonly Type ImplementationType;
        public readonly IReadOnlyList<Type> InterfaceTypes;
        public readonly Lifetime Lifetime;
        public readonly IInstanceProvider Provider;
        
        public Registration(
            Type implementationType,
            Lifetime lifetime,
            IReadOnlyList<Type> interfaceTypes,
            IInstanceProvider provider)
        {
            this.ImplementationType = implementationType;
            this.InterfaceTypes = interfaceTypes;
            this.Lifetime = lifetime;
            this.Provider = provider;
        }

        public override string ToString()
        {
            var contractTypes = this.InterfaceTypes != null ? string.Join(", ", this.InterfaceTypes) : "";
            return $"Registration {this.ImplementationType.Name} ContractTypes=[{contractTypes}] {this.Lifetime} {this.Provider}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => this.Provider.SpawnInstance(resolver);
    }
}