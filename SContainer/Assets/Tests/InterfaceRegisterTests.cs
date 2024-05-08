using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SContainer.Runtime;
using SContainer.Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace SContainer.Tests
{
    public class InterfaceRegisterTests
    {
        [Test]
        public void RegisterMultiInterfaces()
        {
            var builder = new ContainerBuilder();

            builder.Register(typeof(I1), typeof(ServiceA), Lifetime.Singleton);
            builder.Register(typeof(I3), typeof(ServiceA), Lifetime.Singleton);

            var container = builder.Build();

            var obj1 = container.Resolve<I1>();
            var obj2 = container.Resolve<I3>();
            Assert.That(obj1, Is.TypeOf<ServiceA>());
            Assert.That(obj2, Is.TypeOf<ServiceA>());
        }

        [Test]
        public void OverwrittenByTheLaterRegistration()
        {
            var builder = new ContainerBuilder();
            
            builder.Register(typeof(I1), typeof(ServiceA), Lifetime.Singleton);
            builder.Register(typeof(I1), typeof(ServiceB), Lifetime.Singleton);
            
            var container = builder.Build();

            var obj = container.Resolve<I1>();
            Assert.That(obj,Is.Not.TypeOf<ServiceA>());
            Assert.That(obj,Is.TypeOf<ServiceB>());
        }
    }
}
