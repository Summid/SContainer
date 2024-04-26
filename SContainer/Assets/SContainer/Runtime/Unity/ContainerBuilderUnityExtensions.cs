using System;
using UnityEngine;

namespace SContainer.Runtime.Unity
{
    public readonly struct EntryPointsBuilder
    {
        public static void EnsureDispatcherRegistered(IContainerBuilder containerBuilder)
        {
            if (containerBuilder.Exists(typeof(EntryPointDispatcher), false)) return;
            containerBuilder.Register<EntryPointDispatcher>(Lifetime.Scoped);
            containerBuilder.RegisterBuildCallback(container =>
            {
                container.Resolve<EntryPointDispatcher>().Dispatch();
            });
        }

        private readonly IContainerBuilder containerBuilder;
        private readonly Lifetime lifetime;

        public EntryPointsBuilder(IContainerBuilder containerBuilder, Lifetime lifetime)
        {
            this.containerBuilder = containerBuilder;
            this.lifetime = lifetime;
        }

        public RegistrationBuilder Add<T>()
            => this.containerBuilder.Register<T>(this.lifetime).AsImplementedInterfaces();

        public void OnException(Action<Exception> exceptionHandler)
            => this.containerBuilder.RegisterEntryPointExceptionHandler(exceptionHandler);
    }

    public static class ContainerBuilderUnityExtensions
    {
#region EntryPoint
        public static void UseEntryPoints(
            this IContainerBuilder builder,
            Action<EntryPointsBuilder> configuration)
        {
            builder.UseEntryPoints(Lifetime.Singleton, configuration);
        }
        
        public static void UseEntryPoints(
            this IContainerBuilder builder,
            Lifetime lifetime,
            Action<EntryPointsBuilder> configuration)
        {
            EntryPointsBuilder.EnsureDispatcherRegistered(builder);
            configuration(new EntryPointsBuilder(builder, lifetime));
        }

        public static RegistrationBuilder RegisterEntryPoint<T>(
            this IContainerBuilder builder,
            Lifetime lifetime = Lifetime.Singleton)
        {
            EntryPointsBuilder.EnsureDispatcherRegistered(builder);
            return builder.Register<T>(lifetime).AsImplementedInterfaces();
        }

        public static void RegisterEntryPointExceptionHandler(
            this IContainerBuilder builder,
            Action<Exception> exceptionHandler)
        {
            builder.RegisterInstance(new EntryPointExceptionHandler(exceptionHandler));
        }
#endregion

#region UnityComponent
        public static void UseComponents(this IContainerBuilder builder, Action<ComponentsBuilder> configuration)
        {
            configuration(new ComponentsBuilder(builder));
        }

        public static void UseComponent(
            this IContainerBuilder builder,
            Transform root,
            Action<ComponentsBuilder> configuration)
        {
            configuration(new ComponentsBuilder(builder, root));
        }
        
        public static RegistrationBuilder RegisterComponent<TInterface>(
            this IContainerBuilder builder,
            TInterface component)
        {
            var registrationBuilder = new ComponentRegistrationBuilder(component).As(typeof(TInterface));
            // Force inject execution (invoke provider.SpawnInstance)
            builder.RegisterBuildCallback(container => container.Resolve<TInterface>());
            return builder.Register(registrationBuilder);
        }

        public static ComponentRegistrationBuilder RegisterComponentInHierarchy(
            this IContainerBuilder builder,
            Type type)
        {
            var lifetimeScope = (LifetimeScope)builder.ApplicationOrigin;
            var scene = lifetimeScope.gameObject.scene;

            var registrationBuilder = new ComponentRegistrationBuilder(scene, type);
            // Force inject execution (invoke provider.SpawnInstance)
            builder.RegisterBuildCallback(
                container =>
                {
                    container.Resolve(
                        registrationBuilder.InterfaceTypes != null
                            ? registrationBuilder.InterfaceTypes[0]
                            : registrationBuilder.ImplementationType
                    );
                }
            );
            return builder.Register(registrationBuilder);
        }

        public static ComponentRegistrationBuilder RegisterComponentInHierarchy<T>(this IContainerBuilder builder)
        {
            return builder.RegisterComponentInHierarchy(typeof(T));
        }

        public static ComponentRegistrationBuilder RegisterComponentOnNewGameObject(
            this IContainerBuilder builder,
            Type type,
            Lifetime lifetime,
            string newGameObjectName = null)
        {
            return builder.Register(new ComponentRegistrationBuilder(newGameObjectName, type, lifetime));
        }

        public static ComponentRegistrationBuilder RegisterComponentOnNewGameObject<T>(
            this IContainerBuilder builder,
            Lifetime lifetime,
            string newGameObjectName = null)
            where T : Component
        {
            return builder.RegisterComponentOnNewGameObject(typeof(T), lifetime, newGameObjectName);
        }

        public static ComponentRegistrationBuilder RegisterComponentInNewPrefab(
            this IContainerBuilder builder,
            Type interfaceType,
            Component prefab,
            Lifetime lifetime)
        {
            var componentRegistrationBuilder = builder.Register(new ComponentRegistrationBuilder(_ => prefab, prefab.GetType(), lifetime));
            componentRegistrationBuilder.As(interfaceType);
            return componentRegistrationBuilder;
        }

        public static ComponentRegistrationBuilder RegisterComponentInNewPrefab<T>(
            this IContainerBuilder builder,
            T prefab,
            Lifetime lifetime)
            where T : Component
        {
            return builder.RegisterComponentInNewPrefab(typeof(T), prefab, lifetime);
        }

        public static ComponentRegistrationBuilder RegisterComponentInNewPrefab<T>(
            this IContainerBuilder builder,
            Func<IObjectResolver, T> prefab,
            Lifetime lifetime)
            where T : Component
        {
            return builder.Register(new ComponentRegistrationBuilder(prefab, typeof(T), lifetime));
        }

        public static ComponentRegistrationBuilder RegisterComponentInNewPrefab<TInterface, TImplement>(
            this IContainerBuilder builder,
            Func<IObjectResolver, TImplement> prefab,
            Lifetime lifetime)
            where TImplement : Component, TInterface
        {
            var componentRegistrationBuilder = builder.Register(new ComponentRegistrationBuilder(prefab, typeof(TImplement), lifetime));
            componentRegistrationBuilder.As<TInterface>();
            return componentRegistrationBuilder;
        }
#endregion
    }

    /// <summary>
    /// Supports grouping MonoBehaviour's registration.
    /// </summary>
    public readonly struct ComponentsBuilder
    {
        private readonly IContainerBuilder containerBuilder;
        private readonly Transform parentTransform;

        public ComponentsBuilder(IContainerBuilder containerBuilder, Transform parentTransform = null)
        {
            this.containerBuilder = containerBuilder;
            this.parentTransform = parentTransform;
        }

        public RegistrationBuilder AddInstance<TInterface>(TInterface component)
        {
            return this.containerBuilder.RegisterComponent(component);
        }

        public ComponentRegistrationBuilder AddInHierarchy<T>()
            => this.containerBuilder.RegisterComponentInHierarchy<T>()
                .UnderTransform(this.parentTransform);

        public ComponentRegistrationBuilder AddOnNewGameObject<T>(Lifetime lifetime, string newGameObjectName = null)
            where T : Component
            => this.containerBuilder.RegisterComponentOnNewGameObject<T>(lifetime, newGameObjectName)
                .UnderTransform(this.parentTransform);

        public ComponentRegistrationBuilder AddInNewPrefab<T>(T prefab, Lifetime lifetime)
            where T : Component
            => this.containerBuilder.RegisterComponentInNewPrefab(prefab, lifetime)
                .UnderTransform(this.parentTransform);
    }
}