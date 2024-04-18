using SContainer.Runtime.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class MainSceneEntryPoint : MonoBehaviour
    {
        public LifetimeScope MainSceneScope;

        private void Start()
        {
            var container = this.MainSceneScope.Container;
            var presenter = container.Resolve(typeof(MainScenePresenter)) as MainScenePresenter;
        }
    }
}