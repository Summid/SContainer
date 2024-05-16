using NUnit.Framework;
using SContainer.Runtime;
using System;

namespace SContainer.Tests
{
    internal class Foo
    {
        public int Param1 { get; set; }
        public int Param2 { get; set; }
        public int Param3 { get; set; }
        public int Param4 { get; set; }
        public I2 Service2 { get; set; }
        public I3 Service3 { get; set; }
    }

    public class FactoryTests
    {
        [Test]
        public void RegisterFactoryWithParams()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterFactory(() => new Foo());
            builder.RegisterFactory<int, Foo>(param1 => new Foo { Param1 = param1 });
            builder.RegisterFactory<int, int, Foo>((param1, param2) => new Foo
            {
                Param1 = param1,
                Param2 = param2
            });
            builder.RegisterFactory<int, int, int, Foo>((param1, param2, param3) => new Foo
            {
                Param1 = param1,
                Param2 = param2,
                Param3 = param3
            });
            builder.RegisterFactory<int, int, int, int, Foo>((param1, param2, param3, param4) => new Foo
            {
                Param1 = param1,
                Param2 = param2,
                Param3 = param3,
                Param4 = param4
            });

            IObjectResolver container = builder.Build();

            Func<Foo> func0 = container.Resolve<Func<Foo>>();
            Assert.That(func0(), Is.TypeOf<Foo>());

            Func<int, Foo> func1 = container.Resolve<Func<int, Foo>>();
            Foo foo1 = func1(100);
            Assert.That(func1, Is.TypeOf<Func<int, Foo>>());
            Assert.That(foo1.Param1, Is.EqualTo(100));
        }
    }
}