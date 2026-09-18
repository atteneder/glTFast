// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ENTITIES_GRAPHICS

using System;
using System.Collections.Generic;

using Unity.Cloud.Gltfast.Logging;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting.APIUpdating;
#if UNITY_ENTITIES_GRAPHICS
using Unity.Entities.Graphics;
using UnityEngine.Rendering;
#endif

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Generates an Entity hierarchy from a glTF scene.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast.dots")]
    public class EntityInstantiator : IInstantiator
    {

        const float k_Epsilon = .00001f;

        /// <summary>
        /// Logger used by this instantiator. May be <c>null</c> when the caller passed
        /// <see cref="Unity.Cloud.Gltfast.Logging.NullLogger.Instance"/>; subclasses MUST use
        /// null-conditional access (<c>m_Logger?.Error(...)</c>).
        /// </summary>
        protected ICodeLogger m_Logger;

        protected IGltfReadable m_Gltf;

        protected Entity m_Parent;

        protected Dictionary<uint, Entity> m_Nodes;

        protected InstantiationSettings m_Settings;

        EntityManager m_EntityManager;
        EntityArchetype m_NodeArchetype;
        EntityArchetype m_SceneArchetype;
        NodeNameFallback m_NameFallback;

        List<Entity> m_Entities;

        /// <summary>
        /// Constructs an <see cref="EntityInstantiator"/>.
        /// </summary>
        /// <param name="gltf">glTF to instantiate from.</param>
        /// <param name="parent">Generated entities will be children of this entity.</param>
        /// <param name="logger">Custom logger for reporting messages. Defaults to the shared <see cref="Unity.Cloud.Gltfast.Logging.ConsoleLogger.Instance"/> (writes to Unity's Console) when <c>null</c> is passed. Pass <see cref="Unity.Cloud.Gltfast.Logging.NullLogger.Instance"/> (or <c>new NullLogger()</c>) to suppress all output.</param>
        /// <param name="settings">Instantiation settings.</param>
        public EntityInstantiator(
            IGltfReadable gltf,
            Entity parent,
            ICodeLogger logger = null,
            InstantiationSettings settings = null
            )
        {
            m_Gltf = gltf;
            m_Parent = parent;
            m_Logger = logger is NullLogger ? null : (logger ?? ConsoleLogger.Instance);
            m_Settings = settings ?? new InstantiationSettings();
        }

        /// <inheritdoc />
        public void BeginScene(
            string name,
            IReadOnlyList<uint> nodeIndices
        )
        {
            Profiler.BeginSample("BeginScene");
            m_Entities = new List<Entity>();
            m_Nodes = new Dictionary<uint, Entity>();
            m_NameFallback = new NodeNameFallback();
            m_EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            m_NodeArchetype = m_EntityManager.CreateArchetype(
                typeof(Disabled),
                typeof(LocalTransform),
                typeof(Parent),
                typeof(LocalToWorld)
            );
            m_SceneArchetype = m_EntityManager.CreateArchetype(
                typeof(Disabled),
                typeof(LocalTransform),
                typeof(LocalToWorld)
            );

            var dedicatedSceneEntity = m_Settings.SceneObjectCreation == SceneObjectCreation.Always
                || (m_Settings.SceneObjectCreation == SceneObjectCreation.WhenMultipleRootNodes
                    && nodeIndices is { Count: > 1 });

            if (dedicatedSceneEntity)
            {
                var sceneEntity = m_EntityManager.CreateEntity(m_Parent == Entity.Null ? m_SceneArchetype : m_NodeArchetype);
                m_EntityManager.SetComponentData(sceneEntity, LocalTransform.Identity);
                m_EntityManager.SetComponentData(sceneEntity, new LocalToWorld { Value = float4x4.identity });
#if UNITY_EDITOR
                m_EntityManager.SetName(sceneEntity, name ?? "Scene");
#endif
                if (m_Parent != Entity.Null)
                {
                    m_EntityManager.SetComponentData(sceneEntity, new Parent { Value = m_Parent });
                }
                m_Entities.Add(sceneEntity);
            }
            Profiler.EndSample();
        }

#if UNITY_ANIMATION
        /// <inheritdoc />
        public void AddAnimation(AnimationClip[] animationClips)
        {
            if ((m_Settings.Mask & ComponentType.Animation) != 0 && animationClips != null)
            {
                // TODO: Add animation support
            }
        }
#endif // UNITY_ANIMATION

        Entity CreateNodeInternal(
            Parent parent,
            double3 position,
            double4 rotation,
            double3 scale
        )
        {
            Profiler.BeginSample("CreateNode");
            var validParent = parent.Value != Entity.Null;
            var node = m_EntityManager.CreateEntity(validParent ? m_NodeArchetype : m_SceneArchetype);
            m_Entities.Add(node);
            var isUniformScale = IsUniform(scale);
            m_EntityManager.SetComponentData(
                node,
                new LocalTransform
                {
                    Position = (float3)position,
                    Rotation = rotation.ToQuaternion(),
                    Scale = isUniformScale ? (float)scale.x : 1f
                });
            if (!isUniformScale)
            {
                // TODO: Maybe instantiating another archetype instead of adding components here is more performant?
                m_EntityManager.AddComponent<PostTransformMatrix>(node);
                m_EntityManager.SetComponentData(
                    node,
                    new PostTransformMatrix { Value = float4x4.Scale((float3)scale) }
                    );
            }

            if (validParent)
            {
                m_EntityManager.SetComponentData(node, parent);
            }
            Profiler.EndSample();
            return node;
        }

        /// <inheritdoc />
        public virtual void CreateNode(
            uint nodeIndex,
            uint? parentIndex,
            double3 position,
            double4 rotation,
            double3 scale,
            string name
        )
        {
            var parent = new Parent { Value = parentIndex.HasValue ? m_Nodes[parentIndex.Value] : m_Parent };
            var node = CreateNodeInternal(parent, position, rotation, scale);
            m_Nodes[nodeIndex] = node;
            if (name == null)
            {
                m_NameFallback.MarkUnnamed(nodeIndex);
            }
#if UNITY_EDITOR
            m_EntityManager.SetName(node, name ?? NodeNameFallback.DefaultName(nodeIndex));
#endif
        }

        /// <summary>Applies the mesh-name fallback to a node, if it is still unnamed and the mesh carries a name.</summary>
        /// <param name="nodeIndex">Index of the node.</param>
        /// <param name="meshResult">The mesh being assigned to it.</param>
        protected void ApplyMeshNameFallback(uint nodeIndex, MeshResult meshResult)
        {
            if (m_NameFallback.TryTake(nodeIndex, meshResult, out var meshName))
            {
                SetFallbackNodeName(nodeIndex, meshName);
            }
        }

        /// <summary>Names a node the glTF left unnamed, once, with the first non-empty name among its meshes.</summary>
        /// <remarks>Not called when no mesh supplies a name; the <c>Node-{index}</c> placeholder stands instead.</remarks>
        /// <remarks>Entity names exist in the Editor only, so this implementation does nothing in a player build.
        /// An override that stores the name elsewhere still runs.</remarks>
        /// <param name="nodeIndex">Index of the node to name.</param>
        /// <param name="meshName">The resolved fallback name.</param>
        protected virtual void SetFallbackNodeName(uint nodeIndex, string meshName)
        {
#if UNITY_EDITOR
            m_EntityManager.SetName(m_Nodes[nodeIndex], meshName);
#endif
        }

        [Obsolete("AddPrimitive has been renamed to AddMesh. (UnityUpgradable) -> AddMesh(*)", true)]
        public void AddPrimitive(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            IReadOnlyList<uint> joints = null,
            uint? rootJoint = null,
            IReadOnlyList<float> morphTargetWeights = null,
            int meshNumeration = 0
        ) => AddMesh(nodeIndex, meshName, meshResult, joints, rootJoint, morphTargetWeights, meshNumeration);

        /// <inheritdoc />
        public virtual void AddMesh(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            IReadOnlyList<uint> joints = null,
            uint? rootJoint = null,
            IReadOnlyList<float> morphTargetWeights = null,
            int meshNumeration = 0
        )
        {
            ApplyMeshNameFallback(nodeIndex, meshResult);

            if ((m_Settings.Mask & ComponentType.Mesh) == 0)
            {
                return;
            }
            Profiler.BeginSample("AddMesh");

            var materials = new Material[meshResult.materialIndices.Length];
            for (var index = 0; index < meshResult.materialIndices.Length; index++)
            {
                var materialIndex = meshResult.materialIndices[index];
                var material = (materialIndex.HasValue ? m_Gltf.GetMaterial(materialIndex.Value) : null)
                    ?? m_Gltf.GetDefaultMaterial();
                materials[index] = material;
            }

            var filterSettings = RenderFilterSettings.Default;
            filterSettings.ShadowCastingMode = ShadowCastingMode.Off;
            filterSettings.ReceiveShadows = false;
            filterSettings.Layer = m_Settings.Layer;

            var renderMeshDescription = new RenderMeshDescription
            {
                FilterSettings = filterSettings,
                LightProbeUsage = LightProbeUsage.Off,
            };

            var renderMeshArray = new RenderMeshArray(materials, new[] { meshResult.mesh });

            for (ushort index = 0; index < meshResult.materialIndices.Length; index++)
            {
                Entity node;
                if (meshNumeration == 0 && index == 0)
                {
                    // Use node entity for first sub-mesh of first mesh result.
                    node = m_Nodes[nodeIndex];
                }
                else
                {
                    node = CreateNodeInternal(
                        new Parent { Value = m_Nodes[nodeIndex] },
                        float3.zero,
                        Mathematics.k_QuaternionIdentity,
                        new float3(1f)
                        );
#if UNITY_EDITOR
                    m_EntityManager.SetName(node, $"Entity-{meshNumeration}-submesh-{index}");
#endif
                    m_EntityManager.SetEnabled(node, true);
                }

                RenderMeshUtility.AddComponents(
                    node,
                    m_EntityManager,
                    renderMeshDescription,
                    renderMeshArray,
                    MaterialMeshInfo.FromRenderMeshArrayIndices(index, 0, index)
                    );

                // Refine RenderBounds
                // RenderMeshUtility.AddComponents above already sets RenderBounds, but for the entire mesh.
                if (meshResult.mesh.subMeshCount > 1)
                {
                    var subMesh = meshResult.mesh.GetSubMesh(index);
                    m_EntityManager.SetComponentData(node, new RenderBounds { Value = subMesh.bounds.ToAABB() });
                }
            }

            Profiler.EndSample();
        }

        [Obsolete("AddPrimitiveInstanced has been renamed to AddMeshInstanced. (UnityUpgradable) -> AddMeshInstanced(*)", true)]
        public void AddPrimitiveInstanced(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint instanceCount,
            NativeArray<Vector3>? positions,
            NativeArray<Quaternion>? rotations,
            NativeArray<Vector3>? scales,
            int meshNumeration = 0
        ) => AddMeshInstanced(nodeIndex, meshName, meshResult, instanceCount, positions, rotations, scales, meshNumeration);

        /// <inheritdoc />
        public virtual void AddMeshInstanced(
            uint nodeIndex,
            string meshName,
            MeshResult meshResult,
            uint instanceCount,
            NativeArray<Vector3>? positions,
            NativeArray<Quaternion>? rotations,
            NativeArray<Vector3>? scales,
            int meshNumeration = 0
        )
        {
            ApplyMeshNameFallback(nodeIndex, meshResult);

            if ((m_Settings.Mask & ComponentType.Mesh) == 0)
            {
                return;
            }
            Profiler.BeginSample("AddMeshInstanced");
            var materials = new Material[meshResult.materialIndices.Length];
            for (var index = 0; index < meshResult.materialIndices.Length; index++)
            {
                var materialIndex = meshResult.materialIndices[index];
                var material = (materialIndex.HasValue ? m_Gltf.GetMaterial(materialIndex.Value) : null)
                    ?? m_Gltf.GetDefaultMaterial();
                materials[index] = material;
                materials[index].enableInstancing = true;
            }

            var filterSettings = RenderFilterSettings.Default;
            filterSettings.ShadowCastingMode = ShadowCastingMode.Off;
            filterSettings.ReceiveShadows = false;
            filterSettings.Layer = m_Settings.Layer;

            var renderMeshDescription = new RenderMeshDescription
            {
                FilterSettings = filterSettings,
                LightProbeUsage = LightProbeUsage.Off,
            };

            var prototype = m_EntityManager.CreateEntity(m_NodeArchetype);

            if (scales.HasValue)
            {
                var scale = scales.Value[0];
                m_EntityManager.AddComponent<PostTransformMatrix>(prototype);
                m_EntityManager.SetComponentData(
                    prototype,
                    new PostTransformMatrix { Value = float4x4.Scale(scale) }
                    );
            }

            var transform = new LocalTransform
            {
                Position = positions?[0] ?? Vector3.zero,
                Rotation = rotations?[0] ?? Quaternion.identity,
                Scale = 1
            };
            m_EntityManager.AddComponent<LocalTransform>(prototype);
            m_EntityManager.SetComponentData(prototype, transform);

            m_EntityManager.AddComponent<Parent>(prototype);
            m_EntityManager.SetComponentData(prototype, new Parent { Value = m_Nodes[nodeIndex] });

            var renderMeshArray = new RenderMeshArray(materials, new[] { meshResult.mesh });
            RenderMeshUtility.AddComponents(
                prototype,
                m_EntityManager,
                renderMeshDescription,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
            );

            for (ushort index = 0; index < meshResult.materialIndices.Length; index++)
            {
                for (var i = 0; i < instanceCount; i++)
                {

                    var instance = index == 0 && i == 0 ? prototype : m_EntityManager.Instantiate(prototype);
                    m_Entities.Add(instance);

                    if (positions.HasValue)
                        transform.Position = positions.Value[i];
                    if (rotations.HasValue)
                        transform.Rotation = rotations.Value[i];
                    if (scales.HasValue)
                    {
                        m_EntityManager.SetComponentData(
                            instance,
                            new PostTransformMatrix { Value = float4x4.Scale(scales.Value[i]) }
                            );
                    }
                    m_EntityManager.SetComponentData(instance, transform);

                    RenderMeshUtility.AddComponents(
                        instance,
                        m_EntityManager,
                        renderMeshDescription,
                        renderMeshArray,
                        MaterialMeshInfo.FromRenderMeshArrayIndices(index, 0, index)
                    );
                }
            }
            Profiler.EndSample();
        }

        /// <inheritdoc />
        public void AddCamera(uint nodeIndex, uint cameraIndex)
        {
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
        )
        {
            // if ((m_Settings.mask & ComponentType.Light) == 0) {
            //     return;
            // }
            // TODO: Add lights support
        }

        /// <inheritdoc />
        public virtual void EndScene(IReadOnlyList<uint> rootNodeIndices)
        {
            Profiler.BeginSample("EndScene");

            m_NameFallback?.Release();

            if (m_Entities.Count > 0)
            {
                var mainEntity = m_Entities[0];
                var entityGroup = m_EntityManager.AddBuffer<LinkedEntityGroup>(mainEntity);
                entityGroup.Capacity = m_Entities.Count;
                foreach (var entity in m_Entities)
                {
                    entityGroup.Add(new LinkedEntityGroup { Value = entity });
                }
                m_EntityManager.SetEnabled(mainEntity, true);
            }

            Profiler.EndSample();
        }

        static bool IsUniform(double3 scale)
        {
            return Math.Abs(scale.x - scale.y) < k_Epsilon && Math.Abs(scale.x - scale.z) < k_Epsilon;
        }
    }
}

#endif // UNITY_ENTITIES_GRAPHICS
