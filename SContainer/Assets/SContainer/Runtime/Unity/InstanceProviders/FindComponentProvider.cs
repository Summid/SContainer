using SContainer.Runtime.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SContainer.Runtime.Unity
{
    internal sealed class FindComponentProvider : IInstanceProvider
    {
        private readonly Type componentType;
        private readonly IReadOnlyList<IInjectParameter> customParameter;
        private ComponentDestination destination;
        private Scene scene;

        public FindComponentProvider(
            Type componentType,
            IReadOnlyList<IInjectParameter> customParameter,
            in Scene scene,
            in ComponentDestination destination)
        {
            this.componentType = componentType;
            this.customParameter = customParameter;
            this.destination = destination;
            this.scene = scene;
        }
        
        public object SpawnInstance(IObjectResolver resolver)
        {
            var component = default(Component);

            var parent = this.destination.GetParent(resolver);
            if (parent != null)
            {
                component = parent.GetComponentInChildren(this.componentType, true);
                if (component == null)
                {
                    throw new SContainerException(this.componentType, $"{this.componentType} is not in the parent {parent.name} : {this}");
                }
            }
            else if (this.scene.IsValid())
            {
                var gameObjectBuffer = UnityEngineObjectListBuffer<GameObject>.Get();
                this.scene.GetRootGameObjects(gameObjectBuffer);
                foreach (var gameObject in gameObjectBuffer)
                {
                    component = gameObject.GetComponentInChildren(this.componentType, true);
                    if (component != null) break;
                }
                if (component == null)
                {
                    throw new SContainerException(this.componentType, $"{this.componentType} is not in this scene {this.scene.path} : {this}");
                }
            }
            else
            {
                throw new SContainerException(this.componentType, $"Invalid Component find target {this}");
            }

            if (component is MonoBehaviour monoBehaviour)
            {
                var injector = InjectorCache.GetOrBuild(monoBehaviour.GetType());
                injector.Inject(monoBehaviour, resolver, this.customParameter);
            }

            this.destination.ApplyDontDestroyOnLoadIfNeeded(component);
            return component;
        }
    }
}