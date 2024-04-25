using SContainer.Runtime.Internal;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SContainer.Runtime.Unity
{
    internal struct ComponentDestination
    {
        public Transform Parent;
        public Func<IObjectResolver, Transform> ParentFinder;
        public bool DontDestroyOnLoad;

        public Transform GetParent(IObjectResolver resolver)
        {
            if (this.Parent != null)
                return this.Parent;
            if (this.ParentFinder != null)
                return this.ParentFinder(resolver);
            return null;
        }

        public void ApplyDontDestroyOnLoadIfNeeded(Component component)
        {
            if (this.DontDestroyOnLoad)
            {
                UnityEngine.Object.DontDestroyOnLoad(component);
            }
        }
    }
    
    public sealed class ComponentRegistrationBuilder : RegistrationBuilder
    {
        private readonly object instance;
        private readonly Func<IObjectResolver, Component> prefabFinder;
        private readonly string gameObjectName;

        private ComponentDestination destination;
        private Scene scene;
        
        internal ComponentRegistrationBuilder(object instance)
            : base(instance.GetType(), Lifetime.Singleton)
        {
            this.instance = instance;
        }

        internal ComponentRegistrationBuilder(in Scene scene, Type implementationType)
            : base(implementationType, Lifetime.Scoped)
        {
            this.scene = scene;
        }

        internal ComponentRegistrationBuilder(
            Func<IObjectResolver, Component> prefabFinder,
            Type implementationType,
            Lifetime lifetime)
            : base(implementationType, lifetime)
        {
            this.prefabFinder = prefabFinder;
        }

        internal ComponentRegistrationBuilder(
            string gameObjectName,
            Type implementationType,
            Lifetime lifetime)
            : base(implementationType, lifetime)
        {
            this.gameObjectName = gameObjectName;
        }

        public override Registration Build()
        {
            IInstanceProvider provider;

            if (this.instance != null)
            {
                var injector = InjectorCache.GetOrBuild(this.ImplementationType);
                provider = new ExistingComponentProvider(this.instance, injector, this.Parameters, this.destination.DontDestroyOnLoad);
            }
            else if (this.scene.IsValid())
            {
                provider = new FindComponentProvider(this.ImplementationType, this.Parameters, in this.scene, in this.destination);
            }
            else if (this.prefabFinder != null)
            {
                var injector = InjectorCache.GetOrBuild(this.ImplementationType);
                provider = new PrefabComponentProvider(injector, this.Parameters, this.prefabFinder, in this.destination);
            }
            else
            {
                var injector = InjectorCache.GetOrBuild(this.ImplementationType);
                provider = new NewGameObjectProvider(this.ImplementationType, injector, this.Parameters, in this.destination, this.gameObjectName);
            }
            return new Registration(this.ImplementationType, this.Lifetime, this.InterfaceTypes, provider);
        }

        public ComponentRegistrationBuilder UnderTransform(Transform parent)
        {
            this.destination.Parent = parent;
            return this;
        }

        public ComponentRegistrationBuilder UnderTransform(Func<Transform> parentFinder)
        {
            this.destination.ParentFinder = _ => parentFinder();
            return this;
        }

        public ComponentRegistrationBuilder UnderTransform(Func<IObjectResolver, Transform> parentFinder)
        {
            this.destination.ParentFinder = parentFinder;
            return this;
        }

        public ComponentRegistrationBuilder DontDestroyOnLoad()
        {
            this.destination.DontDestroyOnLoad = true;
            return this;
        }
    }
}