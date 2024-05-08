using System;
using System.Collections.Generic;

namespace SContainer.Runtime.Internal
{
    public class OpenGenericRegistrationBuilder : RegistrationBuilder
    {
        public OpenGenericRegistrationBuilder(Type implementationType, Lifetime lifetime)
            : base(implementationType, lifetime)
        {
            if (!implementationType.IsGenericType || implementationType.IsConstructedGenericType)
                throw new SContainerException(implementationType, "Type is not open generic type.");
        }

        public override Registration Build()
        {
            var provider = new OpenGenericInstanceProvider(this.ImplementationType, this.Lifetime, this.Parameters);
            return new Registration(this.ImplementationType, this.Lifetime, this.InterfaceTypes, provider);
        }

        public override RegistrationBuilder AsImplementedInterfaces()
        {
            this.InterfaceTypes = this.InterfaceTypes ?? new List<Type>();
            foreach (var i in this.ImplementationType.GetInterfaces())
            {
                if (!i.IsGenericType)
                    continue;

                this.InterfaceTypes.Add(i.GetGenericTypeDefinition());
            }
            return this;
        }

        protected override void AddInterfaceType(Type interfaceType)
        {
            if (interfaceType.IsConstructedGenericType)
                throw new SContainerException(interfaceType, "Type is not open generic type.");

            foreach (var i in this.ImplementationType.GetInterfaces())
            {
                if (!i.IsGenericType || i.GetGenericTypeDefinition() != interfaceType)
                    continue;

                if (this.InterfaceTypes is null)
                {
                    this.InterfaceTypes = new List<Type>();
                }

                if (!this.InterfaceTypes.Contains(interfaceType))
                    this.InterfaceTypes.Add(interfaceType);

                return;
            }
            // ImplementationType has no interfaces assign from it.
            base.AddInterfaceType(interfaceType);
        }
    }
}