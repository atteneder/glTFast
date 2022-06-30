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

using System;
using System.Collections.Generic;
using GLTFast.Logging;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace GLTFast {
    public class EntityInstantiator : IInstantiator {

        const float k_Epsilon = .00001f;

        protected ICodeLogger logger;
        
        protected IGltfReadable gltf;
        
        protected Entity parent;

        protected Dictionary<uint,Entity> nodes;

        protected InstantiationSettings settings;
        
        EntityManager entityManager;
        EntityArchetype nodeArcheType;
        EntityArchetype sceneArcheType;

        public EntityInstantiator(
            IGltfReadable gltf,
            Entity parent,
            ICodeLogger logger = null,
            InstantiationSettings settings = null
            )
        {
            this.gltf = gltf;
            this.parent = parent;
            this.logger = logger;
            this.settings = settings ?? new InstantiationSettings();
        }

        public virtual void Init() {
            nodes = new Dictionary<uint, Entity>();
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            nodeArcheType = entityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(Parent),
                typeof(LocalToParent),
                typeof(LocalToWorld)
            );
            sceneArcheType = entityManager.CreateArchetype(
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld)
            );
        }

        public void CreateNode(
            uint nodeIndex,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale
        ) {
            var node = entityManager.CreateEntity(nodeArcheType);
            entityManager.SetComponentData(node,new Translation {Value = position});
            entityManager.SetComponentData(node,new Rotation {Value = rotation});
            SetEntityScale(node, scale);
            nodes[nodeIndex] = node;
        }

        void SetEntityScale(Entity node, Vector3 scale) {
            if (!scale.Equals(Vector3.one)) {
                if (Math.Abs(scale.x - scale.y) < k_Epsilon && Math.Abs(scale.x - scale.z) < k_Epsilon) {
                    entityManager.AddComponentData(node, new Scale { Value = scale.x });
                } else {
                    entityManager.AddComponentData(node, new NonUniformScale { Value = scale });
                }
            }
        }

        public void SetParent(uint nodeIndex, uint parentIndex) {
            entityManager.SetComponentData(nodes[nodeIndex],new Parent{Value = nodes[parentIndex]} );
        }

        public void SetNodeName(uint nodeIndex, string name) {
#if UNITY_EDITOR
            entityManager.SetName(nodes[nodeIndex], name ?? $"Node-{nodeIndex}");
#endif
        }

        public virtual void AddPrimitive(
            uint nodeIndex,
            string meshName,
            Mesh mesh,
            int[] materialIndices,
            uint[] joints = null,
            uint? rootJoint = null,
            float[] morphTargetWeights = null,
            int primitiveNumeration = 0
        ) {
            if ((settings.mask & ComponentType.Mesh) == 0) {
                return;
            }
            Entity node;
            if(primitiveNumeration==0) {
                // Use Node GameObject for first Primitive
                node = nodes[nodeIndex];
            } else {
                node = entityManager.CreateEntity(nodeArcheType);
                entityManager.SetComponentData(node,new Translation {Value = new float3(0,0,0)});
                entityManager.SetComponentData(node,new Rotation {Value = quaternion.identity});
                entityManager.SetComponentData(node, new Parent { Value = nodes[nodeIndex] });
            }
            
            var hasMorphTargets = mesh.blendShapeCount > 0;

            for (var index = 0; index < materialIndices.Length; index++) {
                var material = gltf.GetMaterial(materialIndices[index]) ?? gltf.GetDefaultMaterial();
                 
                RenderMeshUtility.AddComponents(node,entityManager,new RenderMeshDescription(mesh,material,layer:settings.layer,subMeshIndex:index));
                 if(joints!=null || hasMorphTargets) {
                     if (joints != null) {
                         var bones = new Entity[joints.Length];
                         for (var j = 0; j < bones.Length; j++)
                         {
                             var jointIndex = joints[j];
                             bones[j] = nodes[jointIndex];
                         }
                         // TODO: Store bone entities array somewhere (pendant to SkinnedMeshRenderer.bones)
                     }
                     if (morphTargetWeights!=null) {
                         for (var i = 0; i < morphTargetWeights.Length; i++) {
                             var weight = morphTargetWeights[i];
                             // TODO set blend shape weight in proper component (pendant to SkinnedMeshRenderer.SetBlendShapeWeight(i, weight); )
                         }
                     }
                 }
            }
        }

        public void AddPrimitiveInstanced(
            uint nodeIndex,
            string meshName,
            Mesh mesh,
            int[] materialIndices,
            uint instanceCount,
            NativeArray<Vector3>? positions,
            NativeArray<Quaternion>? rotations,
            NativeArray<Vector3>? scales,
            int primitiveNumeration = 0
        ) {
            if ((settings.mask & ComponentType.Mesh) == 0) {
                return;
            }
            foreach (var materialIndex in materialIndices) {
                var material = gltf.GetMaterial(materialIndex) ?? gltf.GetDefaultMaterial();
                material.enableInstancing = true;
                var renderMeshDescription = new RenderMeshDescription(mesh, material, subMeshIndex:materialIndex);
                var prototype = entityManager.CreateEntity(nodeArcheType);
                RenderMeshUtility.AddComponents(prototype,entityManager,renderMeshDescription);
                if (scales.HasValue) {
                    entityManager.AddComponent<NonUniformScale>(prototype);
                }
                for (var i = 0; i < instanceCount; i++) {
                    var instance = i>0 ? entityManager.Instantiate(prototype) : prototype;
                    entityManager.SetComponentData(instance,new Translation {Value = positions?[i] ?? Vector3.zero });
                    entityManager.SetComponentData(instance,new Rotation {Value = rotations?[i] ?? Quaternion.identity});
                    entityManager.SetComponentData(instance, new Parent { Value = nodes[nodeIndex] });
                    if (scales.HasValue) {
                        entityManager.SetComponentData(instance, new NonUniformScale() { Value = scales.Value[i] });
                    }
                }
            }
        }

        public void AddCamera(uint nodeIndex, uint cameraIndex) {
            if ((settings.mask & ComponentType.Camera) == 0) {
                return;
            }
            var camera = gltf.GetSourceCamera(cameraIndex);
            // TODO: Add camera support
        }

        public void AddLightPunctual(
            uint nodeIndex,
            uint lightIndex
        ) {
            if ((settings.mask & ComponentType.Light) == 0) {
                return;
            }
            // TODO: Add lights support
        }

        public void AddScene(
            string name,
            uint[] nodeIndices
#if UNITY_ANIMATION
            ,AnimationClip[] animationClips
#endif // UNITY_ANIMATION
            ) {
            Parent sceneParent;
            if (settings.sceneObjectCreation == InstantiationSettings.SceneObjectCreation.Never
                || settings.sceneObjectCreation == InstantiationSettings.SceneObjectCreation.WhenMultipleRootNodes && nodeIndices.Length == 1) {
                sceneParent = new Parent { Value = parent };
            }
            else {
                var sceneEntity = entityManager.CreateEntity(parent==Entity.Null ? sceneArcheType : nodeArcheType);
                entityManager.SetComponentData(sceneEntity,new Translation {Value = new float3(0,0,0)});
                entityManager.SetComponentData(sceneEntity,new Rotation {Value = quaternion.identity});
#if UNITY_EDITOR
                entityManager.SetName(sceneEntity, name ?? "Scene");
#endif
                if (parent != Entity.Null) {
                    entityManager.SetComponentData(sceneEntity, new Parent { Value = parent });
                }
                sceneParent = new Parent { Value = sceneEntity };
            }
            
            foreach(var nodeIndex in nodeIndices) {
                entityManager.SetComponentData(nodes[nodeIndex], sceneParent);
            }

#if UNITY_ANIMATION
            if ((settings.mask & ComponentType.Animation) != 0 && animationClips != null) {
                // TODO: Add animation support
            }
#endif // UNITY_ANIMATION
        }
    }
}

#endif // UNITY_DOTS_HYBRID
