using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SContainer.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace SContainer.Tests
{
    public class ContainerTests
    {
        [Test]
        public void TryResolveTransient()
        {
            var builder = new ContainerBuilder();
            
            builder.Register<ServiceA>(Lifetime.Transient);

            var container = builder.Build();

            Assert.That(container.TryResolve<ServiceA>(out var obj1), Is.True);
            Assert.That(container.TryResolve<ServiceA>(out var obj2), Is.True);
            Assert.That(container.TryResolve<ServiceB>(out var obj3), Is.False);

            Assert.That(obj1, Is.TypeOf<ServiceA>());
            Assert.That(obj2, Is.TypeOf<ServiceA>());
            Assert.That(obj1, Is.Not.EqualTo(obj2));
            Assert.That(obj3, Is.Null);
        }
        
        [Test]
        public void TryResolveSingleton()
        {
            var builder = new ContainerBuilder();
            
            builder.Register<ServiceA>(Lifetime.Singleton);

            var container = builder.Build();

            Assert.That(container.TryResolve<ServiceA>(out var obj1), Is.True);
            Assert.That(container.TryResolve<ServiceA>(out var obj2), Is.True);
            Assert.That(container.TryResolve<ServiceB>(out var obj3), Is.False);

            Assert.That(obj1, Is.TypeOf<ServiceA>());
            Assert.That(obj2, Is.TypeOf<ServiceA>());
            Assert.That(obj1, Is.EqualTo(obj2));
            Assert.That(obj3, Is.Null);
        }
        
        [Test]
        public void TryResolveScope()
        {
            var builder = new ContainerBuilder();
            
            builder.Register<DisposableServiceA>(Lifetime.Scoped);

            var container = builder.Build();

            Assert.That(container.TryResolve<DisposableServiceA>(out var obj1), Is.True);
            Assert.That(container.TryResolve<DisposableServiceA>(out var obj2), Is.True);
            Assert.That(container.TryResolve<DisposableServiceB>(out var obj3), Is.False);

            Assert.That(obj1, Is.TypeOf<DisposableServiceA>());
            Assert.That(obj2, Is.TypeOf<DisposableServiceA>());
            Assert.That(obj1, Is.EqualTo(obj2));
            Assert.That(obj3, Is.Null);
            
            container.Dispose();

            Assert.That(obj1.Disposed, Is.True);
        }
    }
}
