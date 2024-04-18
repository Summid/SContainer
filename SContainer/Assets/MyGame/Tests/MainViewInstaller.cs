using SContainer.Runtime;
using SContainer.Runtime.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame
{
    public class MainViewInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register(new RegistrationBuilder(typeof(MainView), Lifetime.Scoped));
        }
    }
}