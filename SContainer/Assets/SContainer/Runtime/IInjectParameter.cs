using System;

namespace SContainer.Runtime
{
    public interface IInjectParameter
    {
        bool Match(Type parameterType, string parameterName);
        object GetValue(IObjectResolver resolver);
    }
}