using SContainer.Runtime;
using SContainer.Runtime.Unity;

namespace MyGame
{
    public class SceneLoaderScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register(new RegistrationBuilder(typeof(MainScenePresenter), Lifetime.Singleton));
            
            builder.Register(new RegistrationBuilder(typeof(EnemyService), Lifetime.Scoped));
        }
    }
}