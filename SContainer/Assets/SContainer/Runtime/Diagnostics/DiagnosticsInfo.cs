using System.Collections.Generic;

namespace SContainer.Runtime.Diagnostics
{
    public sealed class DiagnosticsInfo
    {
        public string ScopeName { get; }
        public RegisterInfo RegisterInfo { get; }
        public ResolveInfo ResolveInfo { get; set; }
        public List<DiagnosticsInfo> Dependencies { get; } = new List<DiagnosticsInfo>();

        public DiagnosticsInfo(string scopeName, RegisterInfo registerInfo)
        {
            this.ScopeName = scopeName;
            this.RegisterInfo = registerInfo;
        }
    }
}