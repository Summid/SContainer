using System;

namespace SContainer.Runtime.Internal
{
    internal sealed class TypedParameter : IInjectParameter
    {
        public readonly Type Type;
        public readonly object Value;

        public TypedParameter(Type type, object value)
        {
            this.Type = type;
            this.Value = value;
        }

        public bool Match(Type parameterType, string _) => parameterType == this.Type;
        
        public object GetValue(IObjectResolver _)
        {
            return this.Value;
        }
    }

    internal sealed class FuncTypedParameter : IInjectParameter
    {
        public readonly Type Type;
        public readonly Func<IObjectResolver, object> Func;

        public FuncTypedParameter(Type type, Func<IObjectResolver, object> func)
        {
            this.Type = type;
            this.Func = func;
        }

        public bool Match(Type parameterType, string _) => parameterType == this.Type;
        
        public object GetValue(IObjectResolver resolver)
        {
            return this.Func(resolver);
        }
    }

    internal sealed class NamedParameter : IInjectParameter
    {
        public readonly string Name;
        public readonly object Value;

        public NamedParameter(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public bool Match(Type _, string parameterName) => parameterName == this.Name;
        
        public object GetValue(IObjectResolver _)
        {
            return this.Value;
        }
    }
    
    internal sealed class FuncNamedParameter : IInjectParameter
    {
        public readonly string Name;
        public readonly Func<IObjectResolver, object> Func;

        public FuncNamedParameter(string name, Func<IObjectResolver, object> func)
        {
            this.Name = name;
            this.Func = func;
        }

        public bool Match(Type _, string parameterName) => parameterName == this.Name;
        
        public object GetValue(IObjectResolver resolver)
        {
            return this.Func(resolver);
        }
    }
}