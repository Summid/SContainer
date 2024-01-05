using System;
using System.Collections.Generic;

namespace SContainer.Runtime
{
    /// <summary>
    /// Get Registration from Registry which wrapped into this.
    /// </summary>
    public sealed class Registry
    {
        private static IDictionary<Type, Registration> buildBuffer = new Dictionary<Type, Registration>(128);

        public static Registry Build(Registration[] registrations)
        {
            throw new NotImplementedException();
        }
    }
}