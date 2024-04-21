using System;

namespace SContainer.Runtime.Internal
{
    /// <summary>
    /// 解析 <see cref="ContainerLocal{T}"/> 对象
    /// </summary>
    internal sealed class ContainerLocalInstanceProvider : IInstanceProvider
    {
        private readonly Type wrappedType;
        private readonly Registration valueRegistration;

        public ContainerLocalInstanceProvider(Type wrappedType, Registration valueRegistration)
        {
            this.wrappedType = wrappedType;
            this.valueRegistration = valueRegistration;
        }
        
        public object SpawnInstance(IObjectResolver resolver)
        {
            object value;

            if (resolver is ScopedContainer scope &&
                this.valueRegistration.Provider is CollectionInstanceProvider collectionProvider)
            {
                var entireRegistrations = collectionProvider.CollectFromParentScopes(scope, localScopeOnly: true);
                value = collectionProvider.SpawnInstance(resolver, entireRegistrations);
            }
            else
            {
                value = resolver.Resolve(this.valueRegistration);
            }
            var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(1);
            try
            {
                parameterValues[0] = value;
                return Activator.CreateInstance(this.wrappedType, parameterValues);
            }
            finally
            {
                CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
            }
        }
    }
}