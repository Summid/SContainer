using System;

namespace SContainer.Runtime
{
    public class SContainerException : Exception
    {
        public readonly Type InvalidType;

        public SContainerException(Type invalidType, string message) : base(message)
        {
            this.InvalidType = invalidType;
        }
    }
}