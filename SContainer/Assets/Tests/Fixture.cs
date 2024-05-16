using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SContainer.Tests
{
    internal interface I1
    {
    }

    internal interface I2
    {
    }

    internal interface I3
    {
    }

    internal interface GenericI1<T>
    {
    }

    internal class ServiceA : I1, I3
    {
    }

    internal class ServiceB : I1
    {
    }

    internal class ServiceC
    {
    }

    internal class GenericServiceA<T> : GenericI1<T>
    {
    }

    internal class DisposableServiceA : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => this.Disposed = true;
    }
    
    internal class DisposableServiceB : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => this.Disposed = true;
    }
}
