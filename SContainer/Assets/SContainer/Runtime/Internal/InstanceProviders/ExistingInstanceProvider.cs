using System.Runtime.CompilerServices;

namespace SContainer.Runtime.Internal
{
    /// <summary>
    /// 不创建对象，对象由外部提供，且该对象是共享的
    /// </summary>
    internal sealed class ExistingInstanceProvider : IInstanceProvider
    {
        private readonly object implementationInstance;

        public ExistingInstanceProvider(object implementationInstance)
        {
            this.implementationInstance = implementationInstance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver) => this.implementationInstance;
    }
}