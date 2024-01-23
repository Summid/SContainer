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
        public static Type OpenGenericTypeOf(Type closedGenericType)
            => OpenGenericTypes.GetOrAdd(closedGenericType, key => key.GetGenericTypeDefinition());  // 泛型通用定义 e.g. Dictionary<string, TypeClass> => System.Collections.Generic.Dictionary`2[TKey,TValue]

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type[] GenericTypeParametersOf(Type closedGenericType)
            => GenericTypeParameters.GetOrAdd(closedGenericType, key => key.GetGenericArguments()); // 泛型参数的类型 Dictionary`2[string,TypeClass] => Type[]{ System.String, TypeClass}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ArrayTypeOf(Type elementType)
            => ArrayTypes.GetOrAdd(elementType, key => key.MakeArrayType()); // TypeClass => TypeClass[]

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type EnumerableTypeOf(Type elementType)
            => EnumerableTypes.GetOrAdd(elementType, key => typeof(IEnumerable<>).MakeGenericType(key)); // TypeClass => IEnumerable<TypeClass>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type ReadOnlyListTypeOf(Type elementType)
            => ReadOnlyListTypes.GetOrAdd(elementType, key => typeof(IReadOnlyList<>).MakeGenericType(key)); // TypeClass => IReadOnlyList<TypeClass>
    }
}