﻿using System.Collections.Generic;

namespace SContainer.Runtime.Diagnostics
{
    public sealed class ResolveInfo
    {
        public Registration Registration { get; }
        public List<object> Instances { get; } = new List<object>();
        public int MaxDepth { get; set; } = -1;
        public int RefCount { get; set; }
        public long ResolveTime { get; set; }

        public ResolveInfo(Registration registration)
        {
            this.Registration = registration;
        }
    }
}