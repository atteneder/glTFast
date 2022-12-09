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
using UnityEngine.Profiling;

namespace GLTFast {
    public class EntityInstantiator : IInstantiator {

        const float k_Epsilon = .00001f;

        protected ICodeLogger m_Logger;

        protected IGltfReadable m_Gltf;

        protected Entity m_Parent;

        protected Dictionary<uint,Entity> m_Nodes;

        protected InstantiationSettings m_Settings;

        EntityManager m_EntityManager;
        EntityArchetype m_NodeArchetype;
        EntityArchetype m_SceneArchetype;

        Parent m_SceneParent;

        public EntityInstantiator(
            IGltfReadable gltf,
            Entity parent,
            ICodeLogger logger = null,
            InstantiationSettings settings = null
            )
        {
            this.m_Gltf = gltf;
            this.m_Parent = parent;
            this.m_Logger = logger;
            this.m_Settings = settings ?? new InstantiationSettings();
        }

        /// <inheritdoc />
        public void BeginScene(
            string name,
            uint[] nodeIndices
        ) {
            Profiler.BeginSample("BeginScene");
            m_Nodes = new Dictionary<uint, Entity>();
            m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            m_NodeArchetype = m_EntityManager.CreateArchetype(
                typeof(Disabled),
                typeof(Translation),
                typeof(Rotation),
                typeof(Parent),
                typeof(LocalToParent),
                typeof(LocalToWorld)
            );
            m_SceneArchetype = m_EntityManager.CreateArchetype(
                typeof(Disabled),
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld)
            );

            if (m_Settings.SceneObjectCreation == SceneObjectCreation.Never
                || m_Settings.SceneObjectCreation == SceneObjectCreation.WhenMultipleRootNodes && nodeIndices.Length == 1) {
                m_SceneParent = new Parent { Value = m_Parent };
            }
            else {
                var sceneEntity = m_EntityManager.CreateEntity(m_Parent==Entity.Null ? m_SceneArchetype : m_NodeArchetype);
                m_EntityManager.SetComponentData(sceneEntity,new Translation {Value = new float3(0,0,0)});
                m_EntityManager.SetComponentData(sceneEntity,new Rotation {Value = quaternion.identity});
#if UNITY_EDITOR
                m_EntityManager.SetName(sceneEntity, name ?? "Scene");
#endif
                if (m_Parent != Entity.Null) {
                    m_EntityManager.SetComponentData(sceneEntity, new Parent { Value = m_Parent });
                }
                m_SceneParent = new Parent { Value = sceneEntity };
            }
            Profiler.EndSample();
        }

#if UNITY_ANIMATION
        /// <inheritdoc />
        public void AddAnimation(AnimationClip[] animationClips) {
            if ((m_Settings.Mask & ComponentType.Animation) != 0 && animationClips != null) {
                // TODO: Add animation support
            }
        }
#endif // UNITY_ANIMATION

        /// <inheritdoc />
        public void CreateNode(
            uint nodeIndex,
            uint? parentIndex,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale
        ) {
            Profiler.BeginSample("CreateNode");
            var node = m_EntityManager.CreateEntity(m_NodeArchetype);
            m_EntityManager.SetComponentData(node,new Translation {Value = position});
            m_EntityManager.SetComponentData(node,new Rotation {Value = rotation});
            SetEntityScale(node, scale);
            m_Nodes[nodeIndex] = node;
            m_EntityManager.SetComponentData(
                node,
                parentIndex.HasValue
                    ? new Parent { Value = m_Nodes[parentIndex.Value] }
                    : m_SceneParent
                );
            Profiler.EndSample();
        }

        void SetEntityScale(Entity node, Vector3 scale) {
            if (!scale.Equals(Vector3.one)) {
                if (Math.Abs(scale.x - scale.y) < k_Epsilon && Math.Abs(scale.x - scale.z) < k_Epsilon) {
                    m_EntityManager.AddComponentData(node, new Scale { Value = scale.x });
                } else {
                    m_EntityManager.AddComponentData(node, new NonUniformScale { Value = scale });
                }
            }
        }

        public void SetNodeName(uint nodeIndex, string name) {
#if UNITY_EDITOR
            m_EntityManager.SetName(m_Nodes[nodeIndex], name ?? $"Node-{nodeIndex}");
#endif
        }

        /// <inheritdoc />
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
            if ((m_Settings.Mask & ComponentType.Mesh) == 0) {
                return;
            }
            Profiler.BeginSample("AddPrimitive");
            Entity node;
            if(primitiveNumeration==0) {
                // Use Node GameObject for first Primitive
                node = m_Nodes[nodeIndex];
            } else {
                node = m_EntityManager.CreateEntity(m_NodeArchetype);
                m_EntityManager.SetComponentData(node,new Translation {Value = new float3(0,0,0)});
                m_EntityManager.SetComponentData(node,new Rotation {Value = quaternion.identity});
                m_EntityManager.SetComponentData(node, new Parent { Value = m_Nodes[nodeIndex] });
            }

            var hasMorphTargets = mesh.blendShapeCount > 0;

            for (var index = 0; index < materialIndices.Length; index++) {
                var material = m_Gltf.GetMaterial(materialIndices[index]) ?? m_Gltf.GetDefaultMaterial();

                RenderMeshUtility.AddComponents(node,m_EntityManager,new RenderMeshDescription(mesh,material,layer:m_Settings.Layer,subMeshIndex:index));
                 if(joints!=null || hasMorphTargets) {
                     if (joints != null) {
                         var bones = new Entity[joints.Length];
                         for (var j = 0; j < bones.Length; j++)
                         {
                             var jointIndex = joints[j];
                             bones[j] = m_Nodes[jointIndex];
                         }
                         // TODO: Store bone entities array somewhere (pendant to SkinnedMeshRenderer.bones)
                     }
                     // if (morphTargetWeights!=null) {
                     //     for (var i = 0; i < morphTargetWeights.Length; i++) {
                     //         var weight = morphTargetWeights[i];
                     //         // TODO set blend shape weight in proper component (pendant to SkinnedMeshRenderer.SetBlendShapeWeight(i, weight); )
                     //     }
                     // }
                 }
            }
            Profiler.EndSample();
        }

        /// <inheritdoc />
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
            if ((m_Settings.Mask & ComponentType.Mesh) == 0) {
                return;
            }
            Profiler.BeginSample("AddPrimitiveInstanced");
            foreach (var materialIndex in materialIndices) {
                var material = m_Gltf.GetMaterial(materialIndex) ?? m_Gltf.GetDefaultMaterial();
                material.enableInstancing = true;
                var renderMeshDescription = new RenderMeshDescription(mesh, material, subMeshIndex:materialIndex);
                var prototype = m_EntityManager.CreateEntity(m_NodeArchetype);
                RenderMeshUtility.AddComponents(prototype,m_EntityManager,renderMeshDescription);
                if (scales.HasValue) {
                    m_EntityManager.AddComponent<NonUniformScale>(prototype);
                }
                for (var i = 0; i < instanceCount; i++) {
                    var instance = i>0 ? m_EntityManager.Instantiate(prototype) : prototype;
                    m_EntityManager.SetComponentData(instance,new Translation {Value = positions?[i] ?? Vector3.zero });
                    m_EntityManager.SetComponentData(instance,new Rotation {Value = rotations?[i] ?? Quaternion.identity});
                    m_EntityManager.SetComponentData(instance, new Parent { Value = m_Nodes[nodeIndex] });
                    if (scales.HasValue) {
                        m_EntityManager.SetComponentData(instance, new NonUniformScale() { Value = scales.Value[i] });
                    }
                }
            }
            Profiler.EndSample();
        }

        /// <inheritdoc />
        public void AddCamera(uint nodeIndex, uint cameraIndex) {
            // if ((m_Settings.mask & ComponentType.Camera) == 0) {
            //     return;
            // }
            // var camera = m_Gltf.GetSourceCamera(cameraIndex);
            // TODO: Add camera support
        }

        /// <inheritdoc />
        public void AddLightPunctual(
            uint nodeIndex,
            uint lightIndex
        ) {
            // if ((m_Settings.mask & ComponentType.Light) == 0) {
            //     return;
            // }
            // TODO: Add lights support
        }

        /// <inheritdoc />
        public virtual void EndScene(uint[] rootNodeIndices) {
            Profiler.BeginSample("EndScene");
            m_EntityManager.SetEnabled(m_SceneParent.Value, true);
            foreach (var entity in m_Nodes.Values) {
                m_EntityManager.SetEnabled(entity, true);
            }
            Profiler.EndSample();
        }
    }
}

#endif // UNITY_DOTS_HYBRID
