using System;

namespace SContainer.Runtime.Internal
{
    internal sealed class FuncRegistrationBuilder : RegistrationBuilder
    {
        private readonly Func<IObjectResolver, object> implementationProvider;

        public FuncRegistrationBuilder(
            Func<IObjectResolver, object> implementationProvider,
            Type implementationType,
            Lifetime lifetime) : base(implementationType, lifetime)
        {
            this.implementationProvider = implementationProvider;
        }

        public override Registration Build()
        {
            var spawner = new FuncInstanceProvider(this.implementationProvider);
            return new Registration(this.ImplementationType, this.Lifetime, this.InterfaceTypes, spawner);
        }
    }
}