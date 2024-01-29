using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SContainer.Runtime.Unity
{
    public sealed class SContainerParentTypeReferenceNotFound : Exception
    {
        public readonly Type ParentType;

        public SContainerParentTypeReferenceNotFound(Type parentType, string message)
            : base(message)
        {
            this.ParentType = parentType;
        }
    }
    
    partial class LifetimeScope
    {
        private static readonly List<LifetimeScope> WaitingList = new List<LifetimeScope>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubscribeSceneEvents()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void EnqueueAwake(LifetimeScope lifetimeScope)
        {
            WaitingList.Add(lifetimeScope);
        }

        private static void CancelAwake(LifetimeScope lifetimeScope)
        {
            WaitingList.Remove(lifetimeScope);
        }

        private static void AwakenWaitingChildren(LifetimeScope awakenParent)
        {
            if (WaitingList.Count <= 0) return;

            var buf = new List<LifetimeScope>();

            for (var i = WaitingList.Count - 1; i >= 0; i--)
            {
                var waitingScope = WaitingList[i];
                if (waitingScope.parentReference.Type == awakenParent.GetType())
                {
                    waitingScope.parentReference.Object = awakenParent;
                    WaitingList.RemoveAt(i);
                    buf.Add(waitingScope);
                }
            }
            
            foreach (var waitingScope in buf)
            {
                waitingScope.Awake();
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (WaitingList.Count <= 0) return;

            var buf = new List<LifetimeScope>();

            for (var i = WaitingList.Count - 1; i >= 0; i--)
            {
                var waitingScope = WaitingList[i];
                if (waitingScope.gameObject.scene == scene)
                {
                    WaitingList.RemoveAt(i);
                    buf.Add(waitingScope);
                }
            }
            
            foreach (var waitingScope in buf)
            {
                waitingScope.Awake(); // Re-throw if parent not found
            }
        }
    }
}