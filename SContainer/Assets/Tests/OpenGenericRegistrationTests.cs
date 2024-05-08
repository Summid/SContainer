using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SContainer.Runtime;
using System;
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

            builder.Register(typeof(GenericServiceA<>), Lifetime.Singleton);

            var container = builder.Build();
            var obj1 = container.Resolve<GenericServiceA<int>>();
            Assert.That(obj1, Is.TypeOf<GenericServiceA<int>>());
        }
        
        [Test]
        public void OpenGenericRegistrationInterface()
        {
            var builder = new ContainerBuilder();

            builder.Register(typeof(I2<>),typeof(GenericServiceA<>), Lifetime.Singleton);

            var container = builder.Build();
            var obj1 = container.Resolve(typeof(I2<int>));
            Assert.That(obj1, Is.TypeOf<GenericServiceA<int>>());
        }

        [Test]
        public void InvalidOperationException()
        {
            var builder = new ContainerBuilder();
            
            builder.Register(typeof(GenericServiceA<>), Lifetime.Singleton);

            var container = builder.Build();
            
            Assert.Catch(typeof(InvalidOperationException), () => container.Resolve(typeof(GenericServiceA<>)));
        }
    }
}