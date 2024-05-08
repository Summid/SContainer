using SContainer.Runtime.Internal;
using System;
using System.Collections.Generic;

namespace SContainer.Runtime
{
    public class RegistrationBuilder
    {
        internal protected readonly Type ImplementationType;
        internal protected readonly Lifetime Lifetime;

        internal protected List<Type> InterfaceTypes;
        internal protected List<IInjectParameter> Parameters;
        
        public RegistrationBuilder(Type implementationType, Lifetime lifetime)
        {
            this.ImplementationType = implementationType;
            this.Lifetime = lifetime;
        }

        public virtual Registration Build()
        {
            var injector = InjectorCache.GetOrBuild(this.ImplementationType);
            var spawner = new InstanceProvider(injector, this.Parameters);
            return new Registration(
                this.ImplementationType,
                this.Lifetime,
                this.InterfaceTypes,
                spawner);
        }

#region Interface
        public RegistrationBuilder As<TInterface>()
            => this.As(typeof(TInterface));
        
        public RegistrationBuilder As<TInterface1, TInterface2>()
            => this.As(typeof(TInterface1), typeof(TInterface2));

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3>()
            => this.As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));

        public RegistrationBuilder As<TInterface1, TInterface2, TInterface3, TInterface4>()
            => this.As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3), typeof(TInterface4));
        
        public RegistrationBuilder AsSelf()
        {
            this.AddInterfaceType(this.ImplementationType);
            return this;
        }

        public virtual RegistrationBuilder AsImplementedInterfaces()
        {
            this.InterfaceTypes = this.InterfaceTypes ?? new List<Type>();
            this.InterfaceTypes.AddRange(this.ImplementationType.GetInterfaces());
            return this;
        }

        public RegistrationBuilder As(Type interfaceType)
        {
            this.AddInterfaceType(interfaceType);
            return this;
        }

        public RegistrationBuilder As(Type interfaceType1, Type interfaceType2)
        {
            this.AddInterfaceType(interfaceType1);
            this.AddInterfaceType(interfaceType2);
            return this;
        }

        public RegistrationBuilder As(Type interfaceType1, Type interfaceType2, Type interfaceType3)
        {
            this.AddInterfaceType(interfaceType1);
            this.AddInterfaceType(interfaceType2);
            this.AddInterfaceType(interfaceType3);
            return this;
        }

        public RegistrationBuilder As(params Type[] interfaceTypes)
        {
            foreach (var interfaceType in interfaceTypes)
            {
                this.AddInterfaceType(interfaceType);
            }
            return this;
        }

        public RegistrationBuilder WithParameter(string name, object value)
        {
            this.Parameters = this.Parameters ?? new List<IInjectParameter>();
            this.Parameters.Add(new NamedParameter(name, value));
            return this;
        }

        public RegistrationBuilder WithParameter(string name, Func<IObjectResolver, object> value)
        {
            this.Parameters = this.Parameters ?? new List<IInjectParameter>();
            this.Parameters.Add(new FuncNamedParameter(name, value));
            return this;
        }

        public RegistrationBuilder WithParameter(Type type, object value)
        {
            this.Parameters = this.Parameters ?? new List<IInjectParameter>();
            this.Parameters.Add(new TypedParameter(type, value));
            return this;
        }

        public RegistrationBuilder WithParameter(Type type, Func<IObjectResolver, object> value)
        {
            this.Parameters = this.Parameters ?? new List<IInjectParameter>();
            this.Parameters.Add(new FuncTypedParameter(type, value));
            return this;
        }

        public RegistrationBuilder WithParameter<TParam>(TParam value)
        {
            return this.WithParameter(typeof(TParam), value);
        }

        public RegistrationBuilder WithParameter<TParam>(Func<IObjectResolver, TParam> value)
        {
            return this.WithParameter(typeof(TParam), resolver => value(resolver));
        }

        public RegistrationBuilder WithParameter<TParam>(Func<TParam> value)
        {
            return this.WithParameter(typeof(TParam), _ => value);
        }
        
        protected virtual void AddInterfaceType(Type interfaceType)
        {
            if (!interfaceType.IsAssignableFrom(this.ImplementationType))
            {
                throw new SContainerException(interfaceType, $"{this.ImplementationType} is not assignable from {interfaceType}");
            }
            this.InterfaceTypes = this.InterfaceTypes ?? new List<Type>();
            if (!this.InterfaceTypes.Contains(interfaceType))
                this.InterfaceTypes.Add(interfaceType);
        }
#endregion
    }
}