using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime.Internal
{
    internal static class RuntimeTypeCache
    {
        private static readonly ConcurrentDictionary<Type, Type> OpenGenericTypes = new ConcurrentDictionary<Type, Type>();
        private static readonly ConcurrentDictionary<Type, Type[]> GenericTypeParameters = new ConcurrentDictionary<Type, Type[]>();
        private static readonly ConcurrentDictionary<Type, Type> ArrayTypes = new ConcurrentDictionary<Type, Type>();
        private static readonly ConcurrentDictionary<Type, Type> EnumerableTypes = new ConcurrentDictionary<Type, Type>();
        private static readonly ConcurrentDictionary<Type, Type> ReadOnlyListTypes = new ConcurrentDictionary<Type, Type>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type OpenGenericTypeOf(Type closedGenericType)  // 获取开放（非构造）泛型，e.g. Dictionary<string, TypeClass> => System.Collections.Generic.Dictionary`2[TKey,TValue]，与之相反的为具象泛型
            => OpenGenericTypes.GetOrAdd(closedGenericType, key => key.GetGenericTypeDefinition());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type[] GenericTypeParametersOf(Type closedGenericType)  // 获取泛型参数的类型，e.g Dictionary`2[string,TypeClass] => Type[]{ System.String, TypeClass}
            => GenericTypeParameters.GetOrAdd(closedGenericType, key => key.GetGenericArguments());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ArrayTypeOf(Type elementType)  // TypeClass => TypeClass[]
            => ArrayTypes.GetOrAdd(elementType, key => key.MakeArrayType());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type EnumerableTypeOf(Type elementType)  // TypeClass => IEnumerable<TypeClass>
            => EnumerableTypes.GetOrAdd(elementType, key => typeof(IEnumerable<>).MakeGenericType(key));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ReadOnlyListTypeOf(Type elementType)  // TypeClass => IReadOnlyList<TypeClass>
            => ReadOnlyListTypes.GetOrAdd(elementType, key => typeof(IReadOnlyList<>).MakeGenericType(key));
    }
}