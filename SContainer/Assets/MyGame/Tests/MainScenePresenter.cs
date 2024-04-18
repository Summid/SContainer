using SContainer.Runtime.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class MainScenePresenter
    {
        // [Inject]
        // public void InjectEnemy(EnemyService enemyService)
        // {
        //     enemyService.Attack();
        // }

        [Inject]
        public void InjectMainView(MainView mainView)
        {
            mainView.Dump();
        }
    }
}