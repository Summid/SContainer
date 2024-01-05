using System;

namespace SContainer.Runtime
{
    public interface IContainerBuilder
    {
        object ApplicationOrigin { get; set; }
        int Count { get; }
        RegistrationBuilder this[int index] { get; set; }
        
        T Register<T>(T registrationBuilder) where T : RegistrationBuilder;
        void RegisterBuildCallback(Action<IObjectResolver> container);
        bool Exists(Type type, bool includeInterfaceTypes = false);
    }
}