using System;
using UnityEngine;

namespace SContainer.Runtime.Unity
{
    [DefaultExecutionOrder(-5000)]
    public class LifetimeScope : MonoBehaviour, IDisposable
    {
        [SerializeField]
        public bool autoRun = true;
        
        public IObjectResolver Container { get; private set; }

        protected virtual void Awake()
        {
            if (this.autoRun)
            {
                this.Build();
            }
        }

        protected virtual void OnDestroy()
        {
            this.DisposeCore();
        }
        
        public void Dispose()
        {
            this.DisposeCore();
            if (this != null)
            {
                Destroy(this.gameObject);
            }
        }

        public void DisposeCore()
        {
            this.Container?.Dispose();
            this.Container = null;
        }

        protected virtual void Configure(IContainerBuilder builder) { }

        public void Build()
        {
            var builder = new ContainerBuilder
            {
                ApplicationOrigin = this,
            };
            this.InstallTo(builder);
            this.Container = builder.Build();
        }

        private void InstallTo(IContainerBuilder builder)
        {
            this.Configure(builder);
        }
    }
}