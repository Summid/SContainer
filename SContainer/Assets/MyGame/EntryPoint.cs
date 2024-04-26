using SContainer.Runtime.Annotations;
using SContainer.Runtime.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class EntryPoint : IStartable,ITickable
    {
        [Inject]
        public LifetimeScope Scope;

        [Inject]
        public IReadOnlyList<IDisposable> Disposables;
        
        public void Start()
        {
            Debug.Log("IStartable Start");
            var container = this.Scope.Container;
            // var gamePresenter = container.Resolve(typeof(GamePresenter)) as GamePresenter;
            // gamePresenter.HelloWorld();
            // gamePresenter.CharacterAction();
            
            foreach (var disposable in this.Disposables)
            {
                if (disposable is InjectComponent injectComponent)
                {
                    injectComponent.Dump();
                }
            }
            
            if (this.Scope is GameLifetimeScope gameLifetimeScope)
            {
                this.Scope.Container.Instantiate(gameLifetimeScope.MonoTest, Vector3.zero, Quaternion.identity,null);
            }
            else if (this.Scope is SceneLoaderScope sceneLoaderScope)
            {
                this.Scope.Container.Instantiate(sceneLoaderScope.MonoTest, Vector3.zero, Quaternion.identity,null);
            }
        }

        private SceneLoader sceneLoader;

        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (this.sceneLoader == null)
                    this.sceneLoader = new SceneLoader(this.Scope);
                CoroutineHandler.RunCoroutine(this.sceneLoader.LoadSceneAsync());
            }
        }
    }
}