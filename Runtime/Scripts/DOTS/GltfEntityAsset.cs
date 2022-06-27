// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#if UNITY_DOTS_HYBRID

using System.IO;
using System.Threading.Tasks;
using GLTFast.Loading;
using GLTFast.Logging;
using GLTFast.Materials;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace GLTFast {
    
    /// <summary>
    /// Loads a glTF from a MonoBehaviour but instantiates Entities.
    /// Intermediate solution and drop-in replacement for GltfAsset
    /// TODO: To be replaced with a pure ECS concept
    /// </summary>
    public class GltfEntityAsset : GltfAssetBase {

        [Tooltip("URL to load the glTF from.")]
        public string url;
        
        [Tooltip("Automatically load at start.")]
        public bool loadOnStartup = true;
        
        [Tooltip("Override scene to load (-1 loads glTFs default scene)")]
        public int sceneId = -1;
        
        [Tooltip("If checked, url is treated as relative StreamingAssets path.")]
        public bool streamingAsset = false;

        public InstantiationSettings instantiationSettings;
        
        Entity sceneRoot;

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
            string url,
            IDownloadProvider downloadProvider=null,
            IDeferAgent deferAgent=null,
            IMaterialGenerator materialGenerator=null,
            ICodeLogger logger = null
        )
        {
            logger = logger ?? new ConsoleLogger();
            var success = await base.Load(url, downloadProvider, deferAgent, materialGenerator, logger);
            if(success) {
                if (deferAgent != null) await deferAgent.BreakPoint();
                // Auto-Instantiate
                if (sceneId>=0) {
                    InstantiateScene(sceneId,logger);
                } else {
                    Instantiate(logger);
                }
            }
            return success;
        }
        
        protected override IInstantiator GetDefaultInstantiator(ICodeLogger logger) {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var sceneArchetype = entityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Scale),
                typeof(LocalToWorld)
                // typeof(LinkedEntityGroup)
            );
            sceneRoot = entityManager.CreateEntity(sceneArchetype);
#if UNITY_EDITOR
            entityManager.SetName(sceneRoot, string.IsNullOrEmpty(name) ? "glTF" : name);
#endif
            entityManager.SetComponentData(sceneRoot,new Translation {Value = transform.position});
            entityManager.SetComponentData(sceneRoot,new Rotation {Value = transform.rotation});
            entityManager.SetComponentData(sceneRoot,new Scale {Value = transform.localScale.x});
            // entityManager.AddBuffer<LinkedEntityGroup>(sceneRoot);
            return new EntityInstantiator(importer, sceneRoot, logger, instantiationSettings);
        }
        
        protected override void PostInstantiation(IInstantiator instantiator, bool success) {
            currentSceneId = success ? importer.defaultSceneIndex : (int?)null;
        }
        
        /// <summary>
        /// Removes previously instantiated scene(s)
        /// </summary>
        public override void ClearScenes() {
            if (sceneRoot != Entity.Null) {
                DestroyEntityHierarchy(sceneRoot);
                sceneRoot = Entity.Null;
            }
        }

        [BurstCompile]
        static void DestroyEntityHierarchy(Entity rootEntity) {
            var world = World.DefaultGameObjectInjectionWorld;
            var entityManager = world.EntityManager;
            var entityCommandBufferSystem = world.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
            var ecb = entityCommandBufferSystem.CreateCommandBuffer();

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
        }
    }
}
#endif // UNITY_DOTS_HYBRID 
