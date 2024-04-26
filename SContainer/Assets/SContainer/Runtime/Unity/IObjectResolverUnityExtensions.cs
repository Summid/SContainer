using UnityEngine;

namespace SContainer.Runtime.Unity
{
    public static class IObjectResolverUnityExtensions
    {
        public static void InjectGameObject(this IObjectResolver resolver, GameObject gameObject)
        {
            void InjectGameObjectRecursive(GameObject current)
            {
                if (current == null) return;

                using (UnityEngineObjectListBuffer<MonoBehaviour>.Get(out var buffer))
                {
                    buffer.Clear();
                    current.GetComponents(buffer);
                    foreach (var monoBehaviour in buffer)
                    {
                        if (monoBehaviour != null)
                        {
                            // Can be null if the MonoBehaviour's type wasn't found (e.g. if it was stripped)
                            resolver.Inject(monoBehaviour);
                        }
                    }
                }

                var transform = current.transform;
                for (var i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    InjectGameObjectRecursive(child.gameObject);
                }
            }

            InjectGameObjectRecursive(gameObject);
        }
    }
}