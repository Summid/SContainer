﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime
{
    public static class IObjectResolverExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Resolve<T>(this IObjectResolver resolver) => (T)resolver.Resolve(typeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryResolve<T>(this IObjectResolver resolver, out T resolved)
        {
            if (resolver.TryResolve(typeof(T), out var r))
            {
                resolved = (T)r;
                return true;
            }

            resolved = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ResolveOrDefault<T>(this IObjectResolver resolver, T defaultValue = default)
        {
            if (resolver.TryResolve(typeof(T), out var value))
            {
                return (T)value;
            }

            return defaultValue;
        }

        public static object ResolveOrParameter(
            this IObjectResolver resolver,
            Type parameterType,
            string parameterName,
            IReadOnlyList<IInjectParameter> parameters)
        {
            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.Match(parameterType, parameterName))
                    {
                        return parameter.GetValue(resolver);
                    }
                }
            }
            return resolver.Resolve(parameterType);
        }
    }
}