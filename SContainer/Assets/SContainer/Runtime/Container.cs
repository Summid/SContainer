using SContainer.Runtime.Internal;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime
{
    /// <summary>
    /// Container，分为 Root 和 Scope 两类；
    /// 其中 RootContainer 也有 ScopeContainer 的功能（解析 Lifetime == Scope 对象的情况）;
    /// 因此 ScopeContainer 解析单例对象时，（如果需要）会去 Parent Container 里寻找，直到到达 RootContainer
    /// </summary>
    /// <remarks>
    /// Container 主要提供两种功能，Resolve 和 Inject；
    /// Resolve：从注册的 Registration 里解析所需对象；
    /// Inject：将所需对象注入到参数 instance 对象中，instance 一般为 MonoBehaviour/GameObject；（Mono 不支持构造方法，因此只能采用 Fields/Property/Method 的注入方式）
    /// </remarks>
    
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
        IScopedObjectResolver CreateScope(Action<IContainerBuilder> installation = null);
    }

    public interface IScopedObjectResolver : IObjectResolver
    {
        IObjectResolver Root { get; }
        IScopedObjectResolver Parent { get; }
        bool TryGetRegistration(Type type, out Registration registration);
    }

    public enum Lifetime
    {
        Transient,
        Singleton,
        Scoped
    }

    public sealed class ScopedContainer : IScopedObjectResolver
    {
        public IObjectResolver Root { get; }
        public IScopedObjectResolver Parent { get; }
        public object ApplicationOrigin { get; }

        private readonly Registry registry;
        private readonly ConcurrentDictionary<Registration, Lazy<object>> sharedInstances = new ConcurrentDictionary<Registration, Lazy<object>>();
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private readonly Func<Registration, Lazy<object>> createInstance;

        internal ScopedContainer(
            Registry registry,
            IObjectResolver root,
            IScopedObjectResolver parent = null,
            object applicationOrigin = null)
        {
            this.Root = root;
            this.Parent = parent;
            this.ApplicationOrigin = applicationOrigin;
            this.registry = registry;
            this.createInstance = registration =>
            {
                return new Lazy<object>(() => registration.SpawnInstance(this));
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Resolve(Type type) => this.Resolve(this.FindRegistration(type));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Resolve(Registration registration)
        {
            return this.ResolveCore(registration);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IScopedObjectResolver CreateScope(Action<IContainerBuilder> installation = null)
        {
            var containerBuilder = new ScopedContainerBuilder(this.Root, this)
            {
                ApplicationOrigin = this.ApplicationOrigin
            };
            installation?.Invoke(containerBuilder);
            return containerBuilder.BuildScope();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetRegistration(Type type, out Registration registration)
            => this.registry.TryGet(type, out registration);
        
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
                    if (this.Parent is null)
                        return this.Root.Resolve(registration);

                    if (!this.registry.Exists(registration.ImplementationType))
                        return this.Parent.Resolve(registration);

                    return this.CreateTrackedInstance(registration);
                
                case Lifetime.Scoped:
                    return this.CreateTrackedInstance(registration);
                
                default:
                    return registration.SpawnInstance(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object CreateTrackedInstance(Registration registration)
        {
            var lazy = this.sharedInstances.GetOrAdd(registration, this.createInstance);
            var created = lazy.IsValueCreated;
            var instance = lazy.Value;
            if (!created && instance is IDisposable disposable)
            {
                this.disposables.Add(disposable);
            }
            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Registration FindRegistration(Type type)
        {
            IScopedObjectResolver scope = this;
            while (scope != null)
            {
                if (scope.TryGetRegistration(type, out var registration))
                {
                    return registration;
                }
                scope = scope.Parent;
            }
            throw new SContainerException(type, $"No such registration of type: {type}");
        }
    }

    public sealed class Container : IObjectResolver
    {
        public object ApplicationOrigin { get; }

        private readonly Registry registry;
        private readonly IScopedObjectResolver rootScope;
        private readonly ConcurrentDictionary<Registration, Lazy<object>> sharedInstances = new ConcurrentDictionary<Registration, Lazy<object>>();
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private readonly Func<Registration, Lazy<object>> createInstance;

        internal Container(Registry registry, object applicationOrigin = null)
        {
            this.registry = registry;
            this.rootScope = new ScopedContainer(registry, this, applicationOrigin: applicationOrigin);

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
        public IScopedObjectResolver CreateScope(Action<IContainerBuilder> installation = null)
            => this.rootScope.CreateScope(installation);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            this.rootScope.Dispose();
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
                    return this.rootScope.Resolve(registration);
                
                default: // Transient : Instance per resolving.
                    return registration.SpawnInstance(this);
            }
        }
    }
}