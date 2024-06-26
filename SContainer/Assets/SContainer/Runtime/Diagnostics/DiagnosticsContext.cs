﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SContainer.Runtime.Diagnostics
{
    /// <summary> 所有作用域的诊断信息 </summary>
    public static class DiagnosticsContext
    {
        private static readonly Dictionary<string, DiagnosticsCollector> collectors = new Dictionary<string, DiagnosticsCollector>();

        public static event Action<IObjectResolver> OnContainerBuilt;

        public static DiagnosticsCollector GetCollector(string name)
        {
            lock (collectors)
            {
                if (!collectors.TryGetValue(name, out var collector))
                {
                    collector = new DiagnosticsCollector(name);
                    collectors.Add(name, collector);
                }
                return collector;
            }
        }

        public static ILookup<string, DiagnosticsInfo> GetGroupedDiagnosticsInfos()
        {
            lock (collectors)
            {
                return collectors
                    .SelectMany(x => x.Value.GetDiagnosticsInfos())
                    .Where(x => x.ResolveInfo.MaxDepth <= 1) // 只取最多两层解析链
                    .ToLookup(x => x.ScopeName);
            }
        }

        public static IEnumerable<DiagnosticsInfo> GetDiagnosticsInfos()
        {
            lock (collectors)
            {
                return collectors.SelectMany(x => x.Value.GetDiagnosticsInfos());
            }
        }

        public static void NotifyContainerBuilt(IObjectResolver container)
        {
            OnContainerBuilt?.Invoke(container);
        }

        internal static DiagnosticsInfo FindByRegistration(Registration registration)
        {
            return GetDiagnosticsInfos().FirstOrDefault(x => x.ResolveInfo.Registration == registration);
        }

        public static void RemoveCollector(string name)
        {
            lock (collectors)
            {
                collectors.Remove(name);
            }
        }
    }
}