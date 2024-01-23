using System;
using UnityEngine;

namespace MyGame
{
    public class CharacterService : IDisposable
    {
        public void Action()
        {
            Debug.Log("Character do action");
        }

        public void Dispose()
        {
            Debug.Log("CharacterService Disposed");
        }
    }
}