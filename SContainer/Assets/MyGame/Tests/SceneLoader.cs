using SContainer.Runtime;
using SContainer.Runtime.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyGame
{
    public class SceneLoader
    {
        private readonly LifetimeScope parent;

        public SceneLoader(LifetimeScope lifetimeScope)
        {
            this.parent = lifetimeScope;
        }

        public IEnumerator LoadSceneAsync()
        {
            // using (LifetimeScope.Enqueue(builder =>
            //        {
            //            builder.Register(new RegistrationBuilder(typeof(MainView), Lifetime.Scoped));
            //        }))
            var mainViewInstaller = new MainViewInstaller();
            using(LifetimeScope.Enqueue(mainViewInstaller))
            {
                using (LifetimeScope.EnqueueParent(this.parent))
                {
                    Debug.Log("Start loading...");
                    var loading = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
                    while (!loading.isDone)
                    {
                        yield return null;
                    }
                }
            }
        }
    }
}