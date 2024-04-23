using System;

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
    }
}