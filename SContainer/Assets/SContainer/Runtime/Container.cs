using SContainer.Runtime.Internal;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime
{
    public interface IObjectResolver : IDisposable
    {
        object ApplicationOrigin { get; }

        /// <summary>
        /// Resolve from type.
        /// </summary>
        /// <remarks>
        /// This version of resolve looks for all of scopes.
        /// </remarks>
        object Resolve(Type type);

        /// <summary>
        /// Resolve from meta with registration.
        /// </summary>
        /// <remarks>
        /// This version of resolve will look for instances from only the registration information already founds.
        /// </remarks>
        object Resolve(Registration registration);
        void Inject(object instance);
    }

    public enum Lifetime
    {
        Transient,
        Singleton,
        Scoped
    }

    public sealed class Container : IObjectResolver
    {
        public object ApplicationOrigin { get; }

        private readonly Registry registry;
        private readonly ConcurrentDictionary<Registration, Lazy<object>> sharedInstances = new ConcurrentDictionary<Registration, Lazy<object>>();
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private readonly Func<Registration, Lazy<object>> createInstance;

        internal Container(Registry registry, object applicationOrigin = null)
        {
            this.registry = registry;

            this.createInstance = registration =>
            {
                return new Lazy<object>(() => registration.SpawnInstance(this));
            };

            this.ApplicationOrigin = applicationOrigin;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Resolve(Type type)
        {
            if (this.registry.TryGet(type, out var registration)) // This version of resolve looks for all of scope
            {
                return this.Resolve(registration);
            }
            throw new SContainerException(type, $"No such registration of type: {type}");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Resolve(Registration registration)
        {
            return this.ResolveCore(registration);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Inject(object instance)
        {
            var injector = InjectorCache.GetOrBuild(instance.GetType());
            injector.Inject(instance, this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            this.disposables.Dispose();
            this.sharedInstances.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object ResolveCore(Registration registration)
        {
            switch (registration.Lifetime)
            {
                case Lifetime.Singleton:
                    var singleton = this.sharedInstances.GetOrAdd(registration, this.createInstance);
                    if (!singleton.IsValueCreated && singleton.Value is IDisposable disposable)
                    {
                        // 第一次调用（先判断IsValueCreated），添加进 disposables
                        this.disposables.Add(disposable);
                    }
                    return singleton.Value;
                    
                case Lifetime.Scoped:
                    return this.Resolve(registration);
                
                default: // Transient : Instance per resolving.
                    return registration.SpawnInstance(this);
            }
        }
    }
}