using System;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime
{
    public static class ContainerBuilderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register(
            this IContainerBuilder builder,
            Type type,
            Lifetime lifetime) =>
            builder.Register(new RegistrationBuilder(type, lifetime));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register(
            this IContainerBuilder builder,
            Type interfaceType,
            Type implementationType,
            Lifetime lifetime) =>
            builder.Register(implementationType, lifetime).As(interfaceType);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<T>(
            this IContainerBuilder builder,
            Lifetime lifetime) =>
            builder.Register(typeof(T), lifetime);
    }
}