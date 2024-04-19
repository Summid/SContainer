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
    }
}