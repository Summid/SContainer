using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SContainer.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace SContainer.Tests
{
    public class OpenGenericRegistrationTests
    {
        
        [Test]
        public void OpenGenericRegistration()
        {
            var builder = new ContainerBuilder();

            builder.Register(typeof(GenericClass<>), Lifetime.Singleton);

            var container = builder.Build();
            var obj1 = container.Resolve<GenericClass<int>>();
            Assert.That(obj1, Is.TypeOf<GenericClass<int>>());
        }
        
        [Test]
        public void OpenGenericRegistrationInterface()
        {
            var builder = new ContainerBuilder();

            builder.Register(typeof(I2<>),typeof(GenericClass<>), Lifetime.Singleton);

            var container = builder.Build();
            var obj1 = container.Resolve(typeof(I2<int>));
            Assert.That(obj1, Is.TypeOf<GenericClass<int>>());
        }

    }
}