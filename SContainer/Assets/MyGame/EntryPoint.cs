using SContainer.Runtime.Annotations;
using SContainer.Runtime.Unity;
using System;
using UnityEngine;

namespace MyGame
{
    public class EntryPoint : IStartable,ITickable
    {
        [Inject]
        public GameLifetimeScope Scope;
        
        public void Start()
        {
            Debug.Log("IStartable Start");
            var container = this.Scope.Container;
            var gamePresenter = container.Resolve(typeof(GamePresenter)) as GamePresenter;
            gamePresenter.HelloWorld();
            gamePresenter.CharacterAction();
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