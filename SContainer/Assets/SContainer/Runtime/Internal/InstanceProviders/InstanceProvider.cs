using System.Runtime.CompilerServices;

namespace SContainer.Runtime.Internal
{
    internal sealed class InstanceProvider : IInstanceProvider
    {
        private readonly IInjector injector;

        public InstanceProvider(IInjector injector)
        {
            this.injector = injector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => this.injector.CreateInstance(resolver);
    }
}