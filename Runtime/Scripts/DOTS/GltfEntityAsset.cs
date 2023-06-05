// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ENTITIES_GRAPHICS || UNITY_DOTS_HYBRID

using System.IO;
using System.Threading.Tasks;
using GLTFast.Loading;
using GLTFast.Logging;
using GLTFast.Materials;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
#if UNITY_ENTITIES_GRAPHICS
using Unity.Mathematics;
#endif

namespace GLTFast {

    /// <summary>
    /// Loads a glTF from a MonoBehaviour but instantiates Entities.
    /// Intermediate solution and drop-in replacement for GltfAsset
    /// TODO: To be replaced with a pure ECS concept
    /// </summary>
    public class GltfEntityAsset : GltfAssetBase {

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

        /// <inheritdoc cref="GLTFast.InstantiationSettings"/>
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

        protected virtual async void Start() {
            if(loadOnStartup && !string.IsNullOrEmpty(url)) {
                // Automatic load on startup
                await Load(FullUrl);
            }
        }

        public override async Task<bool> Load(
            string gltfUrl,
            IDownloadProvider downloadProvider=null,
            IDeferAgent deferAgent=null,
            IMaterialGenerator materialGenerator=null,
            ICodeLogger logger = null
        )
        {
            logger = logger ?? new ConsoleLogger();
            var success = await base.Load(gltfUrl, downloadProvider, deferAgent, materialGenerator, logger);
            if(success) {
                if (deferAgent != null) await deferAgent.BreakPoint();
                // Auto-Instantiate
                if (sceneId>=0) {
                    await InstantiateScene(sceneId,logger);
                } else {
                    await Instantiate(logger);
                }
            }
            return success;
        }

        protected override IInstantiator GetDefaultInstantiator(ICodeLogger logger) {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var sceneArchetype = entityManager.CreateArchetype(
#if UNITY_DOTS_HYBRID
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(LocalToWorld)
#else
                typeof(LocalTransform),
                typeof(LocalToWorld)
#endif
                // typeof(LinkedEntityGroup)
            );
            m_SceneRoot = entityManager.CreateEntity(sceneArchetype);
#if UNITY_EDITOR
            entityManager.SetName(m_SceneRoot, string.IsNullOrEmpty(name) ? "glTF" : name);
#endif
#if UNITY_DOTS_HYBRID
            entityManager.SetComponentData(m_SceneRoot,new Translation {Value = transform.position});
            entityManager.SetComponentData(m_SceneRoot,new Rotation {Value = transform.rotation});
            entityManager.SetComponentData(m_SceneRoot,new Scale {Value = transform.localScale.x});
            // entityManager.AddBuffer<LinkedEntityGroup>(sceneRoot);
#else
            entityManager.SetComponentData(
                m_SceneRoot,
                new LocalTransform
                {
                    Position = transform.position,
                    Rotation = transform.rotation,
                    Scale = transform.localScale.x,
                });
            entityManager.SetComponentData(m_SceneRoot, new LocalToWorld{Value = float4x4.identity});
#endif
            return new EntityInstantiator(Importer, m_SceneRoot, logger, instantiationSettings);
        }

        protected override void PostInstantiation(IInstantiator instantiator, bool success) {
            CurrentSceneId = success ? Importer.DefaultSceneIndex : null;
        }

        /// <summary>
        /// Removes previously instantiated scene(s)
        /// </summary>
        public override void ClearScenes() {
            if (m_SceneRoot != Entity.Null) {
                DestroyEntityHierarchy(m_SceneRoot);
                m_SceneRoot = Entity.Null;
            }
        }

        [BurstCompile]
        static void DestroyEntityHierarchy(Entity rootEntity) {
            var world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            void DestroyEntity(Entity entity) {
                if (entityManager.HasComponent<Child>(entity)) {
                    var children = entityManager.GetBuffer<Child>(entity);
                    foreach (var child in children) {
                        DestroyEntity(child.Value);
                    }
                }

                ecb.DestroyEntity(entity);
            }

            DestroyEntity(rootEntity);
            
            ecb.Playback(entityManager);
            ecb.Dispose();
        }
    }
}
#endif // UNITY_DOTS_HYBRID
