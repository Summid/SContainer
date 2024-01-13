namespace MyGame
{
    public class HelloWorldService
    {
        public void Hello()
        {
            UnityEngine.Debug.Log("Hello World");
        }

        // Circular dependency is not allowed.
        // public HelloWorldService(GamePresenter gamePresenter)
        // {
        //     
        // }
    }
}