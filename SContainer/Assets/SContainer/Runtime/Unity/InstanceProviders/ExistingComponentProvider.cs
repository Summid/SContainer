using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime.Unity
{
    internal sealed class ExistingComponentProvider : IInstanceProvider
    {
        private readonly object instance;
        private readonly IInjector injector;
        private readonly IReadOnlyList<IInjectParameter> customParameters;
        private bool dontDestroyOnLoad;

        public ExistingComponentProvider(
            object instance,
            IInjector injector,
            IReadOnlyList<IInjectParameter> customParameters,
            bool dontDestroyOnLoad = false)
        {
            this.instance = instance;
            this.injector = injector;
            this.customParameters = customParameters;
            this.dontDestroyOnLoad = dontDestroyOnLoad;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object SpawnInstance(IObjectResolver resolver)
        {
            this.injector.Inject(this.instance, resolver, this.customParameters);
            if (this.dontDestroyOnLoad)
            {
                if (this.instance is UnityEngine.Object component)
                {
                    UnityEngine.Object.DontDestroyOnLoad(component);
                }
                else
                {
                    throw new SContainerException(this.instance.GetType(),
                        $"Cannot apply `DontDestroyOnLoad`. {this.instance.GetType().Name} is not a UnityEngine.Object");
                }
            }
            return this.instance;
        }
    }
}