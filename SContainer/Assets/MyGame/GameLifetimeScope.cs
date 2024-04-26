using SContainer.Runtime;
using SContainer.Runtime.Unity;
using System;

namespace MyGame
{
    public class GameLifetimeScope : LifetimeScope
    {

        // public InjectComponent InjectComponent;
        // public IDisposable DisposableInjectComponent;
        
        public MonoTest MonoTest;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register(new RegistrationBuilder(typeof(GamePresenter), Lifetime.Singleton))
                .WithParameter<string>("https://example.com");
            // builder.Register(new RegistrationBuilder(typeof(HelloWorldService), Lifetime.Singleton).As<IDisposable>());
            // builder.Register(new RegistrationBuilder(typeof(CharacterService), Lifetime.Singleton).As<IDisposable>());
            
            // 重复注册的类型不能是单例类型
            // builder.Register(new RegistrationBuilder(typeof(EnemyService), Lifetime.Scoped));
            // builder.Register(new RegistrationBuilder(typeof(EnemyService), Lifetime.Scoped));
            // builder.Register(new RegistrationBuilder(typeof(EnemyService), Lifetime.Scoped));

            // builder.RegisterInstance<RegisterInstanceTest>(new RegisterInstanceTest());

            // this.DisposableInjectComponent = this.InjectComponent;
            // builder.RegisterComponent(this.DisposableInjectComponent);
            // builder.RegisterComponentInHierarchy(typeof(InjectComponent)).AsImplementedInterfaces();
            builder.RegisterEntryPoint<EntryPoint>();
            
            // builder.RegisterBuildCallback(container =>
            // {
            //     container.Resolve<RegisterInstanceTest>();
            // });
        }
    }
}