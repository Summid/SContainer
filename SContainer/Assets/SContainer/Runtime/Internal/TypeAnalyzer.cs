using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace SContainer.Runtime.Internal
{
    internal sealed class InjectConstructorInfo
    {
        public readonly ConstructorInfo ConstructorInfo;
        public readonly ParameterInfo[] ParameterInfos;

        public InjectConstructorInfo(ConstructorInfo constructorInfo)
        {
            this.ConstructorInfo = constructorInfo;
        }

        public InjectConstructorInfo(ConstructorInfo constructorInfo, ParameterInfo[] parameterInfos)
        {
            this.ConstructorInfo = constructorInfo;
            this.ParameterInfos = parameterInfos;
        }
    }

    internal sealed class InjectMethodInfo
    {
        public readonly MethodInfo MethodInfo;
        public readonly ParameterInfo[] ParameterInfos;

        public InjectMethodInfo(MethodInfo methodInfo, ParameterInfo[] parameterInfos)
        {
            this.MethodInfo = methodInfo;
            this.ParameterInfos = parameterInfos;
        }
    }

    internal sealed class InjectTypeInfo
    {
        public readonly Type Type;
        public readonly InjectConstructorInfo InjectConstructorInfo;
        public readonly IReadOnlyList<InjectMethodInfo> InjectMethodInfos;
        public readonly IReadOnlyList<FieldInfo> InjectFields;
        public readonly IReadOnlyList<PropertyInfo> InjectProperties;

        public InjectTypeInfo(
            Type type,
            InjectConstructorInfo injectConstructorInfo,
            IReadOnlyList<InjectMethodInfo> injectMethodInfos,
            IReadOnlyList<FieldInfo> injectFields,
            IReadOnlyList<PropertyInfo> injectProperties)
        {
            this.Type = type;
            this.InjectConstructorInfo = injectConstructorInfo;
            this.InjectMethodInfos = injectMethodInfos;
            this.InjectFields = injectFields;
            this.InjectProperties = injectProperties;
        }
    }

    internal static class TypeAnalyzer
    {
        public static InjectTypeInfo AnalyzeWithCode(Type type) => Cache.GetOrAdd(type, AnalyzeFunc); 

        private static readonly ConcurrentDictionary<Type, InjectTypeInfo> Cache = new ConcurrentDictionary<Type, InjectTypeInfo>();

        private static readonly Func<Type, InjectTypeInfo> AnalyzeFunc = Analyze;
        
        public static InjectTypeInfo Analyze(Type type)
        {
            //todo
            throw new NotImplementedException();
        }
    }
}