using SContainer.Runtime.Internal;
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
            builder.Register(type.IsGenericType && type.IsGenericTypeDefinition
                ? new OpenGenericRegistrationBuilder(type, lifetime)
                : new RegistrationBuilder(type, lifetime));

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            where TImplement : TInterface =>
            builder.Register<TImplement>(lifetime).As<TInterface>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface1, TInterface2, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            where TImplement : TInterface1, TInterface2 =>
            builder.Register<TImplement>(lifetime).As(typeof(TInterface1), typeof(TInterface2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder Register<TInterface1, TInterface2, TInterface3, TImplement>(
            this IContainerBuilder builder,
            Lifetime lifetime)
            where TImplement : TInterface1, TInterface2, TInterface3 =>
            builder.Register<TImplement>(lifetime).As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));

#region RegisterInstance
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterInstance<TInterface>(
            this IContainerBuilder builder,
            TInterface instance)
            => builder.Register(new InstanceRegistrationBuilder(instance)).As(typeof(TInterface));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterInstance<TInterface1, TInterface2>(
            this IContainerBuilder builder,
            TInterface1 instance)
            => builder.RegisterInstance(instance).As(typeof(TInterface1), typeof(TInterface2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterInstance<TInterface1, TInterface2, TInterface3>(
            this IContainerBuilder builder,
            TInterface1 instance)
            => builder.RegisterInstance(instance).As(typeof(TInterface1), typeof(TInterface2), typeof(TInterface3));
#endregion
    }
}