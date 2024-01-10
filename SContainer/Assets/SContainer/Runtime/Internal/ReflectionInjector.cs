﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SContainer.Runtime.Internal
{
    /// <summary>
    /// 通过反射将注册的类型解析出来，并注入到 injectTypeInfo 所对应的对象中
    /// </summary>
    internal sealed class ReflectionInjector : IInjector
    {
        public static ReflectionInjector Build(Type type)
        {
            var injectTypeInfo = TypeAnalyzer.AnalyzeWithCode(type);
            return new ReflectionInjector(injectTypeInfo);
        }

        private readonly InjectTypeInfo injectTypeInfo;

        private ReflectionInjector(InjectTypeInfo injectTypeInfo)
        {
            this.injectTypeInfo = injectTypeInfo;
        }
        
        /// <summary>
        /// 创建实例，并注入[Inject]依赖
        /// </summary>
        public object CreateInstance(IObjectResolver resolver)
        {
            var parameterInfos = this.injectTypeInfo.InjectConstructorInfo.ParameterInfos;
            var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(parameterInfos.Length);
            try
            {
                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    var parameterInfo = parameterInfos[i];
                    parameterValues[i] = resolver.Resolve(parameterInfo.ParameterType);
                }
                var instance = this.injectTypeInfo.InjectConstructorInfo.ConstructorInfo.Invoke(parameterValues);
                this.Inject(instance, resolver);
                return instance;
            }
            catch (SContainerException ex)
            {
                throw new SContainerException(ex.InvalidType, $"Failed to resolve {this.injectTypeInfo.Type} : {ex.Message}");
            }
            finally
            {
                CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
            }
        }
        
        /// <summary>
        /// 只注入字段、属性和方法
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Inject(object instance, IObjectResolver resolver)
        {
            this.InjectFields(instance, resolver);
            this.InjectProperties(instance, resolver);
            this.InjectMethods(instance, resolver);
        }

        private void InjectFields(object obj, IObjectResolver resolver)
        {
            if (this.injectTypeInfo.InjectFields == null)
                return;

            foreach (var x in this.injectTypeInfo.InjectFields)
            {
                var fieldValue = resolver.Resolve(x.FieldType);
                x.SetValue(obj, fieldValue);
            }
        }

        private void InjectProperties(object obj, IObjectResolver resolver)
        {
            if (this.injectTypeInfo.InjectProperties == null)
                return;

            foreach (var x in this.injectTypeInfo.InjectProperties)
            {
                var propValue = resolver.Resolve(x.PropertyType);
                x.SetValue(obj, propValue);
            }
        }

        private void InjectMethods(object obj, IObjectResolver resolver)
        {
            if (this.injectTypeInfo.InjectMethodInfos == null)
                return;

            foreach (var method in this.injectTypeInfo.InjectMethodInfos)
            {
                var parameterInfos = method.ParameterInfos;
                var parameterValues = CappedArrayPool<object>.Shared8Limit.Rent(parameterInfos.Length);
                try
                {
                    for (var i = 0; i < parameterInfos.Length; i++)
                    {
                        var parameterInfo = parameterInfos[i];
                        parameterValues[i] = resolver.Resolve(parameterInfo.ParameterType);
                    }
                    method.MethodInfo.Invoke(obj, parameterValues);
                }
                catch (SContainerException ex)
                {
                    throw new SContainerException(ex.InvalidType, $"Failed to resolve {this.injectTypeInfo.Type} : {ex.Message}");
                }
                finally
                {
                    CappedArrayPool<object>.Shared8Limit.Return(parameterValues);
                }
            }
        }
    }
}