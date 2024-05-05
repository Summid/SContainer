using SContainer.Runtime.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SContainer.Runtime.Diagnostics
{
    public sealed class DiagnosticsCollector
    {
        public string ScopeName { get; }

        private readonly List<DiagnosticsInfo> diagnosticsInfos = new List<DiagnosticsInfo>();
        
        /// <summary> 解析调用栈，栈底的对象被（紧邻的）栈顶的依赖 </summary>
        private readonly ThreadLocal<Stack<DiagnosticsInfo>> resolveCallStack
            = new ThreadLocal<Stack<DiagnosticsInfo>>(() => new Stack<DiagnosticsInfo>());

        public DiagnosticsCollector(string scopeName)
        {
            this.ScopeName = scopeName;
        }

        public IReadOnlyList<DiagnosticsInfo> GetDiagnosticsInfos()
        {
            return this.diagnosticsInfos;
        }

        public void Clear()
        {
            lock (this.diagnosticsInfos)
            {
                this.diagnosticsInfos.Clear();
            }
        }

        public void TraceRegister(RegisterInfo registerInfo)
        {
            lock (this.diagnosticsInfos)
            {
                this.diagnosticsInfos.Add(new DiagnosticsInfo(this.ScopeName, registerInfo));
            }
        }

        public void TraceBuild(RegistrationBuilder registrationBuilder, Registration registration)
        {
            lock (this.diagnosticsInfos)
            {
                foreach (var x in this.diagnosticsInfos)
                {
                    if (x.RegisterInfo.RegistrationBuilder == registrationBuilder)
                    {
                        x.ResolveInfo = new ResolveInfo(registration);
                        return;
                    }
                }
            }
        }

        public object TraceResolve(Registration registration, Func<Registration, object> resolving)
        {
            var current = DiagnosticsContext.FindByRegistration(registration);
            var owner = this.resolveCallStack.Value.Count > 0 ? this.resolveCallStack.Value.Peek() : null;

            if (!(registration.Provider is CollectionInstanceProvider) && current != null && current != owner)
            {
                current.ResolveInfo.RefCount += 1;
                current.ResolveInfo.MaxDepth = current.ResolveInfo.MaxDepth < 0
                    ? this.resolveCallStack.Value.Count
                    : Math.Max(current.ResolveInfo.MaxDepth, this.resolveCallStack.Value.Count);

                owner?.Dependencies.Add(current);

                this.resolveCallStack.Value.Push(current);
                var watch = Stopwatch.StartNew();
                var instance = resolving(registration);
                watch.Stop();
                this.resolveCallStack.Value.Pop();

                SetResolveTime(current, watch.ElapsedMilliseconds);

                if (!current.ResolveInfo.Instances.Contains(instance))
                {
                    current.ResolveInfo.Instances.Add(instance);
                }

                return instance;
            }
            return resolving(registration);
        }

        private static void SetResolveTime(DiagnosticsInfo current, long elapseMilliseconds)
        {
            var resolves = current.ResolveInfo.RefCount;
            var resolveTime = current.ResolveInfo.ResolveTime;
            
            switch (current.ResolveInfo.Registration.Lifetime)
            {
                case Lifetime.Transient:
                    resolveTime = (resolveTime * (resolves - 1) + elapseMilliseconds / resolves); // 每个对象的（平均）解析时间之和
                    break;
                case Lifetime.Scoped:
                case Lifetime.Singleton:
                    if (elapseMilliseconds > resolveTime) // maybe first time resolving (resolveTime == 0).
                        resolveTime = elapseMilliseconds;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            current.ResolveInfo.ResolveTime = resolveTime;
        }

        public void NotifyContainerBuilt(IObjectResolver container)
            => DiagnosticsContext.NotifyContainerBuilt(container);
    }
}