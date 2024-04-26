using SContainer.Runtime.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class InjectComponent : MonoBehaviour,IDisposable
    {
        [Inject]
        private GameLifetimeScope gameLifetimeScope;

        private void Start()
        {
            Debug.Log($"InjectComponent Start:{this.gameLifetimeScope}");
        }

        public void Dump()
        {
            Debug.Log($"InjectComponent dump:{this.gameLifetimeScope}");
        }
        
        public void Dispose()
        {
            Debug.Log($"InjectComponent Dispose");
            
        }
    }
}