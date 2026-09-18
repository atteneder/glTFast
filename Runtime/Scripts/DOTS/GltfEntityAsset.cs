// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ENTITIES_GRAPHICS

using System.IO;
using System.Threading.Tasks;
using Unity.Cloud.Gltfast.Loading;
using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Materials;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Unity.Collections;
#if UNITY_ENTITIES_GRAPHICS
using Unity.Mathematics;
#endif

namespace Unity.Cloud.Gltfast
{

    /// <summary>
    /// Loads a glTF from a MonoBehaviour but instantiates Entities.
    /// Intermediate solution and drop-in replacement for GltfAsset
    /// TODO: To be replaced with a pure ECS concept
    /// </summary>
    [BurstCompile]
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast.dots")]
    public class GltfEntityAsset : GltfAssetBase
    {

        public string Url => url;

        /// <summary>
        /// Automatically load at start
        /// </summary>
        public bool LoadOnStartup
        {
            get => loadOnStartup;
            set => loadOnStartup = value;
        }

        /// <summary>
        /// Scene to load (-1 loads glTFs default scene)
        /// </summary>
        protected int SceneId => sceneId;

        /// <summary>
        /// If true, url is treated as relative StreamingAssets path
        /// </summary>
        public bool StreamingAsset => streamingAsset;

        /// <inheritdoc cref="Unity.Cloud.Gltfast.InstantiationSettings"/>
        public InstantiationSettings InstantiationSettings
        {
            get => instantiationSettings;
            set => instantiationSettings = value;
        }

        [SerializeField]
        [Tooltip("URL to load the glTF from.")]
        string url;

        [SerializeField]
        [Tooltip("Automatically load at start.")]
        bool loadOnStartup = true;

        [SerializeField]
        [Tooltip("Override scene to load (-1 loads glTFs default scene)")]
        int sceneId = -1;

        [SerializeField]
        [Tooltip("If checked, url is treated as relative StreamingAssets path.")]
        bool streamingAsset;

        [SerializeField]
        InstantiationSettings instantiationSettings;

        Entity m_SceneRoot;

        public string FullUrl => streamingAsset
            ? Path.Combine(Application.streamingAssetsPath, url)
            : url;

        protected virtual async void Start()
        {
            if (loadOnStartup && !string.IsNullOrEmpty(url))
            {
                // Automatic load on startup
                await LoadAsync(FullUrl);
            }
        }

        /// <inheritdoc />
        public override async Task<bool> LoadAsync(
            string gltfUrl,
            IDownloadProvider downloadProvider = null,
            IDeferAgent deferAgent = null,
            IMaterialGenerator materialGenerator = null,
            ICodeLogger logger = null
        )
        {
            logger ??= ConsoleLogger.Instance;
            var success = await base.LoadAsync(gltfUrl, downloadProvider, deferAgent, materialGenerator, logger);
            if (success)
            {
                if (deferAgent != null) await deferAgent.BreakPointAsync();
                // Auto-Instantiate
                if (sceneId >= 0)
                {
                    await InstantiateSceneAsync(sceneId, logger);
                }
                else
                {
                    await InstantiateAsync(logger);
                }
            }
            return success;
        }

        protected override IInstantiator GetDefaultInstantiator(ICodeLogger logger)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;
            m_SceneRoot = EntityUtils.CreateSceneRootEntity(world, name);
            var transformCached = transform;
            entityManager.SetComponentData(
                m_SceneRoot,
                new LocalTransform
                {
                    Position = transformCached.position,
                    Rotation = transformCached.rotation,
                    Scale = transformCached.localScale.x,
                });
            entityManager.SetComponentData(m_SceneRoot, new LocalToWorld { Value = float4x4.identity });
            return new EntityInstantiator(Importer, m_SceneRoot, logger, instantiationSettings);
        }

        protected override void PostInstantiation(IInstantiator instantiator, bool success)
        {
            CurrentSceneId = success ? Importer.DefaultSceneIndex : null;
        }

        /// <summary>
        /// Removes previously instantiated scene(s)
        /// </summary>
        public override void ClearScenes()
        {
            if (m_SceneRoot != Entity.Null)
            {
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                EntityUtils.DestroyChildren(ref m_SceneRoot, ref entityManager);
                entityManager.DestroyEntity(m_SceneRoot);
                m_SceneRoot = Entity.Null;
            }
        }
    }
}
#endif // UNITY_ENTITIES_GRAPHICS
