using SContainer.Runtime.Annotations;

namespace MyGame
{
    public class GamePresenter
    {
        private HelloWorldService helloWorldService;

        public GamePresenter(HelloWorldService helloWorldService)
        {
            this.helloWorldService = helloWorldService;
        }

        // [Inject]
        // public void InjectHelloWorldService(HelloWorldService helloWorldService)
        // {
        //     this.helloWorldService = helloWorldService;
        // }

        public void HelloWorld()
        {
            this.helloWorldService.Hello();
        }
    }
}