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
        bool Exists(Type type);
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
        private List<Action<IObjectResolver>> builderCallbacks;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Register<T>(T registrationBuilder) where T : RegistrationBuilder
        {
            this.registrationBuilders.Add(registrationBuilder);
            return registrationBuilder;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RegisterBuildCallback(Action<IObjectResolver> callback)
        {
            if (this.builderCallbacks == null)
                this.builderCallbacks = new List<Action<IObjectResolver>>();
            this.builderCallbacks.Add(callback);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(Type type)
        {
            foreach (var registrationBuilder in this.registrationBuilders)
            {
                if (registrationBuilder.ImplementationType == type)
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
            if (this.builderCallbacks == null) return;

            foreach (var  callback in this.builderCallbacks)
            {
                callback.Invoke(container);
            }
        }
    }
}