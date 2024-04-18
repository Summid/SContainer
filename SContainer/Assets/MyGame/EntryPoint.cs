using SContainer.Runtime.Unity;
using System;
using UnityEngine;

namespace MyGame
{
    public class EntryPoint : MonoBehaviour
    {
        public LifetimeScope Scope;
        
        private void Start()
        {
            var container = this.Scope.Container;
            var gamePresenter = container.Resolve(typeof(GamePresenter)) as GamePresenter;
            // gamePresenter.HelloWorld();
            // gamePresenter.CharacterAction();
        }

        private SceneLoader sceneLoader;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (this.sceneLoader == null)
                    this.sceneLoader = new SceneLoader(this.Scope);
                this.StartCoroutine(this.sceneLoader.LoadSceneAsync());
            }
        }
    }
}