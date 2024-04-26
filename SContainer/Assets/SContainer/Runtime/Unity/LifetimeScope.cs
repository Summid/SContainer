using System;
using System.Collections.Generic;
using UnityEngine;

namespace SContainer.Runtime.Unity
{
    [DefaultExecutionOrder(-5000)]
    public partial class LifetimeScope : MonoBehaviour, IDisposable
    {
        public readonly struct ParentOverrideScope : IDisposable
        {
            public ParentOverrideScope(LifetimeScope nextParent)
            {
                lock (SyncRoot)
                {
                    GlobalOverrideParents.Push(nextParent);
                }
            }
            
            public void Dispose()
            {
                lock (SyncRoot)
                {
                    GlobalOverrideParents.Pop();
                }
            }
        }
        
        public readonly struct ExtraInstallationScope : IDisposable
        {
            public ExtraInstallationScope(IInstaller installer)
            {
                lock (SyncRoot)
                {
                    GlobalExtraInstallers.Push(installer);
                }    
            }

            void IDisposable.Dispose()
            {
                lock (SyncRoot)
                {
                    GlobalExtraInstallers.Pop();
                }
            }
        }
        
        [SerializeField]
        public ParentReference parentReference;
        
        [SerializeField]
        public bool autoRun = true;

        [SerializeField]
        protected List<GameObject> autoInjectGameObjects;

        private static readonly Stack<LifetimeScope> GlobalOverrideParents = new Stack<LifetimeScope>(); // EnqueueParent(Scope)'s cache
        private static readonly Stack<IInstaller> GlobalExtraInstallers = new Stack<IInstaller>(); // Enqueue(Action<builder>)'s cache
        private static readonly object SyncRoot = new object();

#region Interfaces
        private static LifetimeScope Create(IInstaller installer = null)
        {
            var gameObject = new GameObject("LifetimeScope");
            gameObject.SetActive(false);
            var newScope = gameObject.AddComponent<LifetimeScope>();
            if (installer != null)
            {
                newScope.localExtraInstallers.Add(installer);
            }
            gameObject.SetActive(true);
            return newScope;
        }

        /// <summary> Create a single LifetimeScope </summary>
        public static LifetimeScope Create(Action<IContainerBuilder> configuration)
            => Create(new ActionInstaller(configuration));
        
        /// <summary> Make the param parent be the scope's "parent" which generated in "using block" </summary>
        public static ParentOverrideScope EnqueueParent(LifetimeScope parent)
            => new ParentOverrideScope(parent);

        /// <summary> Add additional registers </summary>
        public static ExtraInstallationScope Enqueue(Action<IContainerBuilder> installing)
            => new ExtraInstallationScope(new ActionInstaller(installing));
        
        /// <summary> Add additional registers </summary>
        public static ExtraInstallationScope Enqueue(IInstaller installer)
            => new ExtraInstallationScope(installer);

        /// <summary> Create a child scope (with extra registrations via "IInstaller") </summary>
        public TScope CreateChild<TScope>(IInstaller installer = null)
            where TScope : LifetimeScope
        {
            var childGameObject = new GameObject("LifetimeScope (Child)");
            childGameObject.SetActive(false);
            if (this.IsRoot)
            {
                DontDestroyOnLoad(childGameObject);
            }
            else
            {
                childGameObject.transform.SetParent(this.transform, false);
            }
            var child = childGameObject.AddComponent<TScope>();
            if (installer != null)
            {
                child.localExtraInstallers.Add(installer);
            }
            child.parentReference.Object = this;
            childGameObject.SetActive(true);
            return child;
        }

        /// <summary> Create a child scope (with extra registrations via "IInstaller") </summary>
        public LifetimeScope CreateChild(IInstaller installer = null)
            => this.CreateChild<LifetimeScope>(installer);

        /// <summary> Create a child scope with extra registrations </summary>
        public TScope CreateChild<TScope>(Action<IContainerBuilder> installation)
            where TScope : LifetimeScope
            => this.CreateChild<TScope>(new ActionInstaller(installation));

        /// <summary> Create a child scope with extra registrations </summary>
        public LifetimeScope CreateChild(Action<IContainerBuilder> installation)
            => this.CreateChild<LifetimeScope>(new ActionInstaller(installation));

        /// <summary> Create a child scope with LifetimeScope prefab </summary>
        public TScope CreateChildFromPrefab<TScope>(TScope prefab, IInstaller installer = null)
            where TScope : LifetimeScope
        {
            var wasActive = prefab.gameObject.activeSelf;
            if (wasActive)
            {
                prefab.gameObject.SetActive(false);
            }
            var child = Instantiate(prefab, this.transform, false);
            if (installer != null)
            {
                child.localExtraInstallers.Add(installer);
            }
            child.parentReference.Object = this;
            if (wasActive)
            {
                prefab.gameObject.SetActive(true);
                child.gameObject.SetActive(true);
            }
            return child;
        }

        /// <summary> Create a child scope with LifetimeScope prefab and extra registrations </summary>
        public TScope CreateChildFromPrefab<TScope>(TScope prefab, Action<IContainerBuilder> installation)
            where TScope : LifetimeScope
            => this.CreateChildFromPrefab(prefab, new ActionInstaller(installation));
#endregion
        
        public IObjectResolver Container { get; private set; }
        public LifetimeScope Parent { get; private set; }
        public bool IsRoot { get; set; } // TODO: assign in settings

        // when create a child scope with a installer, cache the installer in this first.
        private readonly List<IInstaller> localExtraInstallers = new List<IInstaller>();

        protected virtual void Awake()
        {
            try
            {
                this.Parent = this.GetRuntimeParent();
                if (this.autoRun)
                {
                    this.Build();
                }
            }
            catch (SContainerParentTypeReferenceNotFound) when (!this.IsRoot)
            {
                if (WaitingList.Contains(this))
                {
                    throw;
                }
                EnqueueAwake(this);
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
            CancelAwake(this);
        }

        protected virtual void Configure(IContainerBuilder builder) { }

        public void Build()
        {
            if (this.Parent == null)
                this.Parent = this.GetRuntimeParent();

            if (this.Parent != null)
            {
                this.Parent.Container.CreateScope(builder =>
                {
                    builder.RegisterBuildCallback(this.SetContainer);
                    builder.ApplicationOrigin = this;
                    this.InstallTo(builder);
                });
            }
            else
            {
                var builder = new ContainerBuilder
                {
                    ApplicationOrigin = this,
                };
                builder.RegisterBuildCallback(this.SetContainer);
                this.InstallTo(builder);
                builder.Build();
            }

            AwakenWaitingChildren(this);
        }

        private void SetContainer(IObjectResolver container)
        {
            this.Container = container;
            this.AutoInjectAll();
        }

        private void InstallTo(IContainerBuilder builder)
        {
            this.Configure(builder);

            // process child's extra installer
            foreach (var installer in this.localExtraInstallers)
            {
                installer.Install(builder);
            }
            this.localExtraInstallers.Clear();

            lock (SyncRoot)
            {
                // process add installer by 'Enqueue()'
                foreach (var installer in GlobalExtraInstallers)
                {
                    installer.Install(builder);
                }
            }

            builder.RegisterInstance<LifetimeScope>(this).AsSelf();
            EntryPointsBuilder.EnsureDispatcherRegistered(builder);
        }

        private LifetimeScope GetRuntimeParent()
        {
            if (this.IsRoot) return null;
            
            if (this.parentReference.Object != null)
                return this.parentReference.Object;
            
            // Find is scene via type
            if (this.parentReference.Type != null && this.parentReference.Type != this.GetType())
            {
                var found = Find(this.parentReference.Type);
                if (found != null && found.Container != null)
                {
                    return found;
                }
                throw new SContainerParentTypeReferenceNotFound(
                    this.parentReference.Type,
                    $"{this.name} could not found parent reference of type : {this.parentReference.Type}");
            }

            lock (SyncRoot)
            {
                if (GlobalOverrideParents.Count > 0)
                {
                    return GlobalOverrideParents.Peek();
                }
            }
            
            return null;
        }

        private static LifetimeScope Find(Type type)
        {
            return (LifetimeScope)FindObjectOfType(type);
        }

        private void AutoInjectAll()
        {
            if (this.autoInjectGameObjects == null)
                return;

            foreach (var target in this.autoInjectGameObjects)
            {
                if (target != null) // Check miss reference
                {
                    this.Container.InjectGameObject(target);
                }
            }
        }
    }
}