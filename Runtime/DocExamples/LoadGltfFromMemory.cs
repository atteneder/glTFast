// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast.Logging;

namespace GLTFast.Documentation.Examples
{

    using System;
    using GLTFast;
    using UnityEngine;

    class LoadGltfFromMemory : MonoBehaviour
    {
        #region LoadGltfFromMemory
        public async Task LoadGltfFile(string filePath)
        {
            var gltfDataAsByteArray = await File.ReadAllBytesAsync(filePath);
            var gltf = new GltfImport(logger: new ConsoleLogger());
            var success = await gltf.Load(
                gltfDataAsByteArray,
                // The URI of the original data is important for resolving relative URIs within the glTF
                new Uri(filePath)
                );
            if (success)
            {
                await gltf.InstantiateMainSceneAsync(transform);
            }
        }
        #endregion

        public void LoadViaComponent()
        {
            #region LoadViaComponent
            var gltf = gameObject.AddComponent<GltfAsset>();
            gltf.Url = "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Assets/main/Models/Duck/glTF/Duck.gltf";
            #endregion
            gltf.LoadOnStartup = false;
        }

        public static async Task ImportSettings(string filePath)
        {
            #region ImportSettings
            var gltf = new GltfImport();

            // Create a settings object and configure it accordingly
            var settings = new ImportSettings
            {
                GenerateMipMaps = true,
                AnisotropicFilterLevel = 3,
                NodeNameMethod = NameImportMethod.OriginalUnique
            };
            // Load the glTF and pass along the settings
            var success = await gltf.Load(filePath, settings);

            if (success)
            {
                var gameObject = new GameObject("glTF");
                await gltf.InstantiateMainSceneAsync(gameObject.transform);
            }
            else
            {
                Debug.LogError("Loading glTF failed!");
            }
            #endregion
        }

        public async Task Instantiation(string filePath)
        {
            #region Instantiation
            // First step: load glTF
            var gltf = new GLTFast.GltfImport();
            var success = await gltf.Load(filePath);

            if (success)
            {
                // Here you can customize the post-loading behavior

                // Get the first material
                var material = gltf.GetMaterial();
                Debug.LogFormat("The first material is called {0}", material.name);

                // Instantiate the glTF's main scene
                await gltf.InstantiateMainSceneAsync(new GameObject("Instance 1").transform);
                // Instantiate the glTF's main scene
                await gltf.InstantiateMainSceneAsync(new GameObject("Instance 2").transform);

                // Instantiate each of the glTF's scenes
                for (var sceneId = 0; sceneId < gltf.SceneCount; sceneId++)
                {
                    await gltf.InstantiateSceneAsync(transform, sceneId);
                }
            }
            else
            {
                Debug.LogError("Loading glTF failed!");
            }
            #endregion
        }

#if UNITY_ANIMATION
        public async Task SceneInstanceAccess(string filePath)
        {
            #region SceneInstanceAccess
            var logger = new ConsoleLogger();
            var gltfImport = new GltfImport(logger: logger);
            await gltfImport.Load(filePath);
            var instantiator = new GameObjectInstantiator(gltfImport, transform, logger: logger);
            var success = await gltfImport.InstantiateMainSceneAsync(instantiator);
            if (success)
            {
                // Get the SceneInstance to access the instance's properties
                var sceneInstance = instantiator.SceneInstance;

                // Enable the first imported camera (which are disabled by default)
                if (sceneInstance.Cameras is { Count: > 0 })
                {
                    sceneInstance.Cameras[0].enabled = true;
                }

                // Decrease lights' ranges
                if (sceneInstance.Lights != null)
                {
                    foreach (var gltfLight in sceneInstance.Lights)
                    {
                        gltfLight.range *= 0.1f;
                    }
                }

                // Play the default (i.e. the first) animation clip
                var legacyAnimation = instantiator.SceneInstance.LegacyAnimation;
                if (legacyAnimation is not null)
                {
                    legacyAnimation.Play();
                }
            }
            #endregion
        }
#endif // UNITY_ANIMATION

        public async Task CustomDeferAgent()
        {
            var manyUrls = new[]
            {
                "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Assets/main/Models/Duck/glTF/Duck.gltf"
            };
            #region CustomDeferAgent
            // Recommended: Use a common defer agent across multiple GltfImport instances!
            // TimeBudgetPerFrameDeferAgent for a stable frame rate:
            IDeferAgent deferAgent = gameObject.AddComponent<TimeBudgetPerFrameDeferAgent>();
            // Or alternatively, UninterruptedDeferAgent for low latency loading:
            deferAgent = new UninterruptedDeferAgent();

            var tasks = new List<Task>();

            foreach (var url in manyUrls)
            {
                var gltf = new GltfImport(null, deferAgent);
                var task = gltf.Load(url).ContinueWith(
                    async t =>
                    {
                        if (t.Result)
                        {
                            await gltf.InstantiateMainSceneAsync(transform);
                        }
                    },
                    TaskScheduler.FromCurrentSynchronizationContext()
                );
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            #endregion
        }
    }
}
