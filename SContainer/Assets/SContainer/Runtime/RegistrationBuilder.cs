using System;

namespace SContainer.Runtime
{
    public class RegistrationBuilder
    {
        internal readonly Type ImplementationType;
        internal readonly Lifetime Lifetime;
        
        public RegistrationBuilder(Type implementationType, Lifetime lifetime)
        {
            this.ImplementationType = implementationType;
            this.Lifetime = lifetime;
        }

        public virtual Registration Build()
        {
            throw new NotImplementedException();
        }
    }
}