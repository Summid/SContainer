using SContainer.Runtime;
using SContainer.Runtime.Unity;
using System;

namespace MyGame
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register(new RegistrationBuilder(typeof(GamePresenter), Lifetime.Singleton));
            builder.Register(new RegistrationBuilder(typeof(HelloWorldService), Lifetime.Singleton).As<IDisposable>());
            builder.Register(new RegistrationBuilder(typeof(CharacterService), Lifetime.Singleton).As<IDisposable>());
            
            // 重复注册的类型不能是单例类型
            builder.Register(new RegistrationBuilder(typeof(EnemyService), Lifetime.Transient));
            builder.Register(new RegistrationBuilder(typeof(EnemyService), Lifetime.Transient));
            builder.Register(new RegistrationBuilder(typeof(EnemyService), Lifetime.Transient));
        }
    }
}