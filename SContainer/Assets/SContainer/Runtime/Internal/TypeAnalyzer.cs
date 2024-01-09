using SContainer.Runtime.Annotations;
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

        public InjectMethodInfo(MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
            this.ParameterInfos = methodInfo.GetParameters();
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
            var injectConstructor = default(InjectConstructorInfo);
            var typeInfo = type.GetTypeInfo();

            // Constructor, single [Inject] constructor -> single most parameters constructor.
            var annotatedConstructorCount = 0;
            var maxParameters = -1;
            foreach (var constructorInfo in typeInfo.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) 
            {
                if (constructorInfo.IsDefined(typeof(InjectAttribute), false))
                {
                    if (++annotatedConstructorCount > 1)
                    {
                        throw new SContainerException(type, $"Type found multiple [Inject] marked constructor, type: {type.Name}");
                    }
                    injectConstructor = new InjectConstructorInfo(constructorInfo);
                }
                else if (annotatedConstructorCount <= 0)
                {
                    var parameterInfos = constructorInfo.GetParameters();
                    if (parameterInfos.Length > maxParameters)
                    {
                        injectConstructor = new InjectConstructorInfo(constructorInfo, parameterInfos);
                        maxParameters = parameterInfos.Length;
                    }
                }
            }

            // 一般的 C# 类会有默认构造方法
            if (injectConstructor != null)
            {
                var allowNoConstructor = type.IsEnum;
                
                // It seems that Unity sometimes strips thr constructor of Component at build time.
                // In that case, allow null.
                allowNoConstructor |= type.IsSubclassOf(typeof(UnityEngine.Component));
                if (!allowNoConstructor)
                    throw new SContainerException(type, $"Type dose not found injectable constructor, type: {type.Name}");
            }

            var injectMethods = default(List<InjectMethodInfo>);
            var injectFields = default(List<FieldInfo>);
            var injectProperties = default(List<PropertyInfo>);
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;  // Declared Only

            while (type != null && type != typeof(object))
            {
                // Method, [Inject] Only
                var methods = type.GetMethods(bindingFlags);
                foreach (var methodInfo in methods)
                {
                    if (methodInfo.IsDefined(typeof(InjectAttribute), false))
                    {
                        if (injectMethods == null)
                        {
                            injectMethods = new List<InjectMethodInfo>();
                        }
                        else
                        {
                            // Skip if already exists
                            foreach (var x in injectMethods)
                            {
                                // MyClassA => MyClassB => MyClassC, B and C override method that declared in A.
                                // 也就是说，忽略掉基类中被 [Inject] 标记的同名方法，下面的 Field、Prop 也是一样的
                                if (x.MethodInfo.GetBaseDefinition() == methodInfo.GetBaseDefinition())
                                    goto EndMethod;
                            }
                        }

                        injectMethods.Add(new InjectMethodInfo(methodInfo));
                    }
                }
            EndMethod:
                
                // Fields, [Inject] Only
                var fields = type.GetFields(bindingFlags);
                foreach (var fieldInfo in fields)
                {
                    if (fieldInfo.IsDefined(typeof(InjectAttribute), false))
                    {
                        if (injectFields == null)
                        {
                            injectFields = new List<FieldInfo>();
                        }
                        else
                        {
                            foreach (var x in injectFields)
                            {
                                if(x.Name == fieldInfo.Name)
                                    goto EndFields;
                            }
                            
                            // if (injectFields.Any(x => x.Name == fieldInfo.Name)) // 为什么要重复判断？
                            // {
                            //     continue;
                            // }
                        }
                        injectFields.Add(fieldInfo);
                    }
                }
            EndFields:
                
                // Properties, [Inject] Only
                var props = type.GetProperties(bindingFlags);
                foreach (var propertyInfo in props)
                {
                    if (propertyInfo.IsDefined(typeof(InjectAttribute), false))
                    {
                        if (injectProperties == null)
                        {
                            injectProperties = new List<PropertyInfo>();
                        }
                        else
                        {
                            foreach (var x in injectProperties)
                            {
                                if(x.Name == propertyInfo.Name)
                                    goto EndProperty;
                            }
                        }
                        injectProperties.Add(propertyInfo);
                    }
                }
            EndProperty:

                type = type.BaseType;
            }

            return new InjectTypeInfo(
                type,
                injectConstructor,
                injectMethods,
                injectFields,
                injectProperties);
        }
    }
}