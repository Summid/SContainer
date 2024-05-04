using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SContainer.Runtime.Unity
{
    public class SContainerSettings : ScriptableObject
    {
        public static SContainerSettings Instance { get; private set; }
        public static bool DiagnosticsEnabled => Instance != null && Instance.EnableDiagnostics;

        private static LifetimeScope rootLifetimeScopeInstance;

        [SerializeField]
        [Tooltip("Set the Prefab to be the parent of the entire Project.")]
        public LifetimeScope RootLifetimeScope;

        [SerializeField]
        [Tooltip("Enable the collection of information that can be viewed in the SContainerDiagnosticsWindow. Note: Performance degradation")]
        public bool EnableDiagnostics;

        [SerializeField]
        [Tooltip("Disables script modification for LifetimeScope scripts.")]
        public bool DisableScriptModifier;

        [SerializeField]
        [Tooltip("Removes (Clone) postfix in IObjectResolver.Instantiate() and IContainerBuilder.RegisterComponentInNewPrefab().")]
        public bool RemoveClonePostfix;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/Create/SContainer/SContainer Settings")]
        public static void CreateAsset()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                "Save SContainerSettings",
                "SContainerSettings",
                "asset",
                string.Empty);

            if (string.IsNullOrEmpty(path))
                return;

            var newSettings = CreateInstance<SContainerSettings>();
            UnityEditor.AssetDatabase.CreateAsset(newSettings, path);

            var preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
            preloadedAssets.RemoveAll(x => x is SContainerSettings);
            preloadedAssets.Add(newSettings);
            UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
        }

        public static void LoadInstanceFromPreloadAssets()
        {
            var preloadAsset = UnityEditor.PlayerSettings.GetPreloadedAssets().FirstOrDefault(x => x is SContainerSettings);
            if (preloadAsset is SContainerSettings instance)
            {
                instance.OnDisable();
                instance.OnEnable();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitialize()
        {
            // For editor, we need to load the Preload asset manually.
            LoadInstanceFromPreloadAssets();
        }
#endif
        
        public LifetimeScope GetOrCreateRootLifetimeScopeInstance()
        {
            if (this.RootLifetimeScope != null && rootLifetimeScopeInstance == null)
            {
                var activeBefore = this.RootLifetimeScope.gameObject.activeSelf;
                this.RootLifetimeScope.gameObject.SetActive(false);

                rootLifetimeScopeInstance = Instantiate(this.RootLifetimeScope);
                DontDestroyOnLoad(rootLifetimeScopeInstance);
                rootLifetimeScopeInstance.gameObject.SetActive(true);

                this.RootLifetimeScope.gameObject.SetActive(activeBefore);
            }
            return rootLifetimeScopeInstance;
        }

        public bool IsRootLifetimeScopeInstance(LifetimeScope lifetimeScope) =>
            this.RootLifetimeScope == lifetimeScope || rootLifetimeScopeInstance == lifetimeScope;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                Instance = this;

                var activeScene = SceneManager.GetActiveScene();
                if (activeScene.isLoaded)
                {
                    this.OnFirstSceneLoaded(activeScene, default);
                }
                else
                {
                    SceneManager.sceneLoaded -= this.OnFirstSceneLoaded;
                    SceneManager.sceneLoaded += this.OnFirstSceneLoaded;
                }
            }
        }

        private void OnDisable()
        {
            Instance = null;
        }

        private void OnFirstSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (this.RootLifetimeScope != null &&
                this.RootLifetimeScope.autoRun &&
                (rootLifetimeScopeInstance == null || rootLifetimeScopeInstance.Container == null))
            {
                this.GetOrCreateRootLifetimeScopeInstance();
            }
            SceneManager.sceneLoaded -= this.OnFirstSceneLoaded;
        }
    }
}