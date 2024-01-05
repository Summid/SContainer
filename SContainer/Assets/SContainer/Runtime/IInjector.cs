using System.Collections.Generic;

namespace SContainer.Runtime
{
    public interface IInjector
    {
        void Inject(object instance, IObjectResolver resolver);
        object CreateInstance(IObjectResolver resolver);
    }
}