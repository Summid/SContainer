using System;
using System.Collections.Concurrent;

namespace SContainer.Runtime.Internal
{
    internal static class InjectorCache
    {
        private static readonly ConcurrentDictionary<Type, IInjector> Injectors = new ConcurrentDictionary<Type, IInjector>();

        public static IInjector GetOrBuild(Type type)
        {
            return Injectors.GetOrAdd(type, ReflectionInjector.Build);
        }
    }
}