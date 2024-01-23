using System;
using UnityEngine;

namespace MyGame
{
    public class HelloWorldService : IDisposable
    {
        public void Hello()
        {
            Debug.Log("Hello World");
        }
        
        public void Dispose()
        {
            Debug.Log("HelloWorldService Disposed");
        }
    }
}