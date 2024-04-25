using System;
using System.Collections.Generic;
using UnityEngine;

namespace SContainer.Runtime.Unity
{
    internal sealed class PrefabComponentProvider : IInstanceProvider
    {
        private readonly IInjector injector;
        private readonly IReadOnlyList<IInjectParameter> customParameters;
        private readonly Func<IObjectResolver, Component> prefabFinder;
        private ComponentDestination destination;

        public PrefabComponentProvider(
            IInjector injector,
            IReadOnlyList<IInjectParameter> customParameters,
            Func<IObjectResolver, Component> prefabFinder,
            in ComponentDestination destination)
        {
            this.injector = injector;
            this.customParameters = customParameters;
            this.prefabFinder = prefabFinder;
            this.destination = destination;
        }

        public object SpawnInstance(IObjectResolver resolver)
        {
            var prefab = this.prefabFinder(resolver);
            var parent = this.destination.GetParent(resolver);

            var wasActive = prefab.gameObject.activeSelf;
            if (wasActive)
            {
                prefab.gameObject.SetActive(false);
            }

            var component = parent != null
                ? UnityEngine.Object.Instantiate(prefab, parent)
                : UnityEngine.Object.Instantiate(prefab);

            try
            {
                this.injector.Inject(component, resolver, this.customParameters);
                this.destination.ApplyDontDestroyOnLoadIfNeeded(component);
            }
            finally
            {
                if (wasActive)
                {
                    prefab.gameObject.SetActive(true);
                    component.gameObject.SetActive(true);
                }
            }

            return component;
        }
    }
}