using System;

namespace SContainer.Runtime.Internal
{
    internal sealed class InstanceRegistrationBuilder : RegistrationBuilder
    {
        private readonly object implementationInstance;

        public InstanceRegistrationBuilder(object implementationInstance)
            : base(implementationInstance.GetType(), Lifetime.Singleton)
        {
            this.implementationInstance = implementationInstance;
        }

        public override Registration Build()
        {
            var spawner = new ExistingInstanceProvider(this.implementationInstance);
            return new Registration(this.ImplementationType, this.Lifetime, this.InterfaceTypes, spawner);
        }
    }
}