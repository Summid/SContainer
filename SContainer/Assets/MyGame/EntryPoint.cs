using SContainer.Runtime.Unity;
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
            gamePresenter.HelloWorld();
            gamePresenter.CharacterAction();
        }
    }
}