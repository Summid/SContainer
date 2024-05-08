using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SContainer.Tests
{
    public interface I1
    {
    }

    public interface I2<T>
    {
    }

    public interface I3
    {
    }

    public class ServiceA : I1, I3
    {
    }

    public class ServiceB : I1
    {
    }

    public class GenericClass<T> : I2<T>
    {
    }
}
