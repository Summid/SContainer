using System;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime.Internal
{
    internal sealed class FuncInstanceProvider : IInstanceProvider
    {
        private readonly Func<IObjectResolver, object> implementationProvider;

        public FuncInstanceProvider(Func<IObjectResolver, object> implementationProvider)
        {
            this.implementationProvider = implementationProvider;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => this.implementationProvider(resolver);
    }
}