using SContainer.Runtime.Annotations;
using System;
using System.Collections.Generic;

namespace MyGame
{
    public class GamePresenter
    {
        private HelloWorldService helloWorldService;
        private CharacterService characterService;

        [Inject]
        public void InjectService(IEnumerable<IDisposable> disposables)
        {
            foreach (IDisposable disposable in disposables)
            {
                if (disposable is HelloWorldService helloService)
                {
                    this.helloWorldService = helloService;
                }
                if (disposable is CharacterService chaService)
                {
                    this.characterService = chaService;
                }
            }
        }

        [Inject]
        public void InjectEnemy(IEnumerable<EnemyService> services)
        {
            foreach (EnemyService enemyService in services)
            {
                enemyService.Attack();
            }
        }

        public void HelloWorld()
        {
            this.helloWorldService.Hello();
        }

        public void CharacterAction()
        {
            this.characterService.Action();
        }
    }
}