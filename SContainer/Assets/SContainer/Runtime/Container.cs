using System;

namespace SContainer.Runtime
{
    public interface IObjectResolver : IDisposable
    {
        object ApplicationOrigin { get; }

        /// <summary>
        /// Resolve from type.
        /// </summary>
        /// <remarks>
        /// This version of resolve looks for all of scopes.
        /// </remarks>
        object Resolve(Type type);

        /// <summary>
        /// Resolve from meta with registration.
        /// </summary>
        /// <remarks>
        /// This version of resolve will look for instances from only the registration information already founds.
        /// </remarks>
        object Resolve(Registration registration);
        void Inject(object instance);
    }

    public enum Lifetime
    {
        Transient,
        Singleton,
        Scoped
    }

    public sealed class Container : IObjectResolver
    {
        public object ApplicationOrigin { get; }
        
        public object Resolve(Type type)
        {
            throw new NotImplementedException();
        }
        
        public object Resolve(Registration registration)
        {
            throw new NotImplementedException();
        }
        
        public void Inject(object instance)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
        }
    }
}