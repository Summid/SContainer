using System;
using System.Collections.Generic;
using UnityEngine;

namespace SContainer.Runtime.Unity
{
    internal sealed class NewGameObjectProvider : IInstanceProvider
    {
        private readonly Type componentType;
        private readonly IInjector injector;
        private readonly IReadOnlyList<IInjectParameter> customParameter;
        private readonly string newGameObjectName;
        private ComponentDestination destination;

        public NewGameObjectProvider(
            Type componentType,
            IInjector injector,
            IReadOnlyList<IInjectParameter> customParameter,
            in ComponentDestination destination,
            string newGameObjectName = null)
        {
            this.componentType = componentType;
            this.injector = injector;
            this.customParameter = customParameter;
            this.newGameObjectName = newGameObjectName;
            this.destination = destination;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var name = string.IsNullOrEmpty(this.newGameObjectName)
                ? this.componentType.Name
                : this.newGameObjectName;
            var gameObject = new GameObject(name);
            gameObject.SetActive(false);

            var parent = this.destination.GetParent(resolver);
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            var component = gameObject.AddComponent(this.componentType);

            this.injector.Inject(component, resolver, this.customParameter);
            this.destination.ApplyDontDestroyOnLoadIfNeeded(component);

            component.gameObject.SetActive(true);
            return component;
        }
    }
}