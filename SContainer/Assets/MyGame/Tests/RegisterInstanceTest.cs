using SContainer.Runtime.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class RegisterInstanceTest
    {
        private GamePresenter GamePresenter;
        
        [Inject]
        public void InjectGamePresenter(GamePresenter gamePresenter)
        {
            Debug.Log("InjectGamePresenter");
            this.GamePresenter = gamePresenter;
        }
    }
}