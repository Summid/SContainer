using SContainer.Runtime.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime
{
    /// <summary>
    /// 创建 Container 的小帮手，因此也分 Root 和 Scope 两类；
    /// </summary>
    /// <remarks>
    /// 主要工作是，在创建 Container 前注册 Registrations ，并将它们包装进 Registry，这也是 Container.ctor 的参数之一；
    /// 创建完成后，还需处理回调；
    /// </remarks>
    
    public interface IContainerBuilder
    {
        object ApplicationOrigin { get; set; }
        int Count { get; }
        RegistrationBuilder this[int index] { get; set; }
        
        T Register<T>(T registrationBuilder) where T : RegistrationBuilder;
        void RegisterBuildCallback(Action<IObjectResolver> container);
        bool Exists(Type type, bool includeInterfaceTypes = false, bool findParentScopes = false);
    }

    public sealed class ScopedContainerBuilder : ContainerBuilder
    {
        private readonly IObjectResolver root;
        private readonly IScopedObjectResolver parent;

        internal ScopedContainerBuilder(IObjectResolver root, IScopedObjectResolver parent)
        {
            this.root = root;
            this.parent = parent;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IScopedObjectResolver BuildScope()
        {
            var registry = this.BuildRegistry();
            var container = new ScopedContainer(registry, this.root, this.parent, this.ApplicationOrigin);
            this.EmitCallback(container);
            return container;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IObjectResolver Build() => this.BuildScope();

        public override bool Exists(Type type, bool includeInterfaceTypes = false, bool findParentScopes = false)
        {
            if (base.Exists(type, includeInterfaceTypes, findParentScopes))
            {
                return true;
            }

            if (findParentScopes)
            {
                var next = this.parent;
                while (next != null)
                {
                    if (this.parent.TryGetRegistration(type, out var registration))
                    {
                        if (includeInterfaceTypes || registration.ImplementationType == type)
                        {
                            return true;
                        }
                    }
                    next = next.Parent;
                }
            }
            return false;
        }
    }

    public class ContainerBuilder : IContainerBuilder
    {
        public object ApplicationOrigin { get; set; }

        public int Count => this.registrationBuilders.Count;
        
        public RegistrationBuilder this[int index]
        {
            get => this.registrationBuilders[index];
            set => this.registrationBuilders[index] = value;
        }

        private readonly List<RegistrationBuilder> registrationBuilders = new List<RegistrationBuilder>();
        private Action<IObjectResolver> builderCallback;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Register<T>(T registrationBuilder) where T : RegistrationBuilder
        {
            this.registrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterBuildCallback(Action<IObjectResolver> callback)
        {
            this.builderCallback += callback;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool Exists(Type type, bool includeInterfaceTypes = false, bool findParentScopes = false)
        {
            foreach (var registrationBuilder in this.registrationBuilders)
            {
                if (registrationBuilder.ImplementationType == type ||
                    includeInterfaceTypes && registrationBuilder.InterfaceTypes?.Contains(type) == true)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual IObjectResolver Build()
        {
            var registry = this.BuildRegistry();
            var container = new Container(registry, this.ApplicationOrigin);
            this.EmitCallback(container);
            return container;
        }

        protected Registry BuildRegistry()
        {
            var registrations = new Registration[this.registrationBuilders.Count];

            for (var i = 0; i < this.registrationBuilders.Count; i++)
            {
                var registrationBuilder = this.registrationBuilders[i];
                var registration = registrationBuilder.Build();
                registrations[i] = registration;
            }

            var registry = Registry.Build(registrations);
            TypeAnalyzer.CheckCircularDependency(registrations, registry);

            return registry;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void EmitCallback(IObjectResolver container)
        {
            this.builderCallback?.Invoke(container);
        }
    }
}