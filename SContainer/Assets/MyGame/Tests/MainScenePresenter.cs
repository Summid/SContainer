using SContainer.Runtime.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class MainScenePresenter
    {
        [Inject]
        public void InjectEnemy(IEnumerable<EnemyService> services)
        {
            foreach (EnemyService enemyService in services)
            {
                enemyService.Attack();
            }
        }

        // [Inject]
        // public void InjectMainView(MainView mainView)
        // {
        //     mainView.Dump();
        // }
    }
}