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
                    throw new NotImplementedException("Resolving scope is not supported currently.");
                    // return this.Resolve(registration);
                
                default: // Transient : Instance per resolving.
                    return registration.SpawnInstance(this);
            }
        }
    }
}