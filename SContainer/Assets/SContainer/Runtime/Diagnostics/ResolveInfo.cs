using System.Collections.Generic;

namespace SContainer.Runtime.Diagnostics
{
    public sealed class ResolveInfo
    {
        public Registration Registration { get; }
        public List<object> Instances { get; } = new List<object>();
        /// <summary> 解析层数，作为被依赖对象解析时，（最长的）解析链上的对象总数 </summary>
        public int MaxDepth { get; set; } = -1;
        public int RefCount { get; set; }
        public long ResolveTime { get; set; }

        public ResolveInfo(Registration registration)
        {
            this.Registration = registration;
        }
    }
}