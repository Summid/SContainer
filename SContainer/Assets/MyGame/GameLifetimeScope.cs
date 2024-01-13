using SContainer.Runtime;
using SContainer.Runtime.Unity;

namespace MyGame
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register(new RegistrationBuilder(typeof(GamePresenter), Lifetime.Singleton));
            builder.Register(new RegistrationBuilder(typeof(HelloWorldService), Lifetime.Singleton));
            
            // Duplicate implementation type is not supported currently.
            // builder.Register(new RegistrationBuilder(typeof(HelloWorldService), Lifetime.Singleton));
        }
    }
}