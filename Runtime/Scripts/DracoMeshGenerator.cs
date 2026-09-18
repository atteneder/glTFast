// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if DRACO_IS_RECENT

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Draco;
using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Objects;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Mesh = UnityEngine.Mesh;

namespace Unity.Cloud.Gltfast
{

    class DracoMeshGenerator : MeshGeneratorBase
    {
        readonly bool m_NeedsNormals;
        readonly bool m_NeedsTangents;

        readonly bool m_HasMorphTargets;
        JobHandle m_MorphTargetsJobHandle;

        public override bool IsCompleted => base.IsCompleted && (!m_HasMorphTargets || m_MorphTargetsJobHandle.IsCompleted);

        public DracoMeshGenerator(
            IReadOnlyList<MeshPrimitive> primitives,
            IReadOnlyList<string> morphTargetNames,
            string meshName,
            IGltfReadable gltf,
            BufferStore buffers,
            IDeferAgent deferAgent,
            ICodeLogger logger
            )
            : base(meshName)
        {
            var morphTargets = primitives[0].Targets;
            m_HasMorphTargets = morphTargets is { Count: > 0 };

            var vertexCount = 0;
            var primitivesCount = primitives.Count;
            var vertexIntervals = m_HasMorphTargets
                ? new int[primitivesCount + 1]
                : null;

            var bounds = new Bounds[primitivesCount];

            for (var index = 0; index < primitivesCount; index++)
            {
                var primitive = primitives[index];
                Assert.IsTrue(primitive.IsDracoCompressed);

                var posAccessor = buffers.GetAccessor(primitive.Attributes.Position.Value);

                if (m_HasMorphTargets)
                {
                    vertexIntervals[index] = vertexCount;
                }
                vertexCount += posAccessor.Count;

                if (bounds != null)
                {
                    var subMeshBounds = posAccessor.TryGetBounds();

                    if (subMeshBounds.HasValue)
                    {
                        bounds[index] = subMeshBounds.Value;
                    }
                    else
                    {
                        logger?.Error(LogCode.MeshBoundsMissing, primitive.Attributes.Position.ToString());
                        bounds = null;
                    }
                }

                if (!primitive.Material.HasValue)
                {
                    m_NeedsNormals = true;
                }
                else
                {
                    var material = gltf.GetSourceMaterial(primitive.Material.Value);
                    m_NeedsNormals |= material.RequiresNormals;
                    m_NeedsTangents |= material.RequiresTangents;
                }
            }

            if (m_HasMorphTargets)
            {
                vertexIntervals[^1] = vertexCount;
                InitializeMorphTargets(
                    primitives,
                    morphTargetNames,
                    vertexIntervals,
                    vertexCount,
                    morphTargets,
                    buffers,
                    deferAgent,
                    logger
                    );
            }

            m_CreationTask = Decode(primitives, buffers, bounds);
        }

        void InitializeMorphTargets(
            IReadOnlyList<MeshPrimitive> primitives,
            IReadOnlyList<string> morphTargetNames,
            int[] vertexIntervals,
            int vertexCount,
            List<MorphTarget> morphTargets,
            BufferStore buffers,
            IDeferAgent deferAgent,
            ICodeLogger logger
            )
        {
            m_MorphTargetsGenerator = new MorphTargetsGenerator(
                vertexCount,
                primitives.Count,
                morphTargets.Count,
                morphTargetNames,
                morphTargets[0].Normal.HasValue,
                morphTargets[0].Tangent.HasValue,
                buffers,
                deferAgent,
                logger

            );
            for (var subMesh = 0; subMesh < primitives.Count; subMesh++)
            {
                var primitive = primitives[subMesh];
                for (var morphTargetIndex = 0; morphTargetIndex < primitive.Targets.Count; morphTargetIndex++)
                {
                    var target = primitive.Targets[morphTargetIndex];
                    m_MorphTargetsGenerator.AddMorphTarget(
                        vertexIntervals[subMesh], subMesh, morphTargetIndex, target, logger);
                }
            }
            m_MorphTargetsJobHandle = m_MorphTargetsGenerator.GetJobHandle();
        }

        async Task<Mesh> Decode(
            IReadOnlyList<MeshPrimitive> primitives,
            BufferStore buffers,
            Bounds[] bounds
            )
        {
            var bufferViews = new NativeArray<byte>.ReadOnly[primitives.Count];
            var attributesArray = new Attributes[primitives.Count];

            for (var index = 0; index < primitives.Count; index++)
            {
                var dracoExt = primitives[index].Extensions.DracoMeshCompression;
                if (dracoExt.BufferView is not { } bufferViewIndex
                    || buffers.TryGetBufferView(bufferViewIndex, out var bufferView, out _) != BufferAccessStatus.Success)
                {
                    return null;
                }

                bufferViews[index] = bufferView.AsNativeArrayReadOnly();
                attributesArray[index] = dracoExt.Attributes;
            }

            var mesh = await StartDecode(bufferViews, attributesArray, bounds == null);

            if (mesh is null)
            {
                return null;
            }

            if (bounds != null)
            {
                UpdateSubMeshBounds(0);
                var overallBounds = bounds[0];
                for (var i = 1; i < mesh.subMeshCount; i++)
                {
                    UpdateSubMeshBounds(i);
                    overallBounds.Encapsulate(bounds[i]);
                }
                mesh.bounds = overallBounds;
            }

            if (m_MorphTargetsGenerator != null)
            {
                while (!m_MorphTargetsJobHandle.IsCompleted)
                    await Task.Yield();
                m_MorphTargetsJobHandle.Complete();
                await m_MorphTargetsGenerator.ApplyOnMeshAndDisposeAsync(mesh);
            }

            mesh.name = m_MeshName;

#if GLTFAST_KEEP_MESH_DATA
            mesh.UploadMeshData(false);
#endif

            return mesh;

            void UpdateSubMeshBounds(int i)
            {
                var subMeshDescriptor = mesh.GetSubMesh(i);
                subMeshDescriptor.bounds = bounds[i];
                mesh.SetSubMesh(
                    i,
                    subMeshDescriptor,
                    MeshUpdateFlags.DontValidateIndices
                    | MeshUpdateFlags.DontResetBoneBounds
                    | MeshUpdateFlags.DontNotifyMeshUsers
                    | MeshUpdateFlags.DontRecalculateBounds
                );
            }
        }

        async Task<Mesh> StartDecode(
            NativeArray<byte>.ReadOnly[] data,
            Attributes[] attributesArray,
            bool calculateBounds
            )
        {
            var decodeSettings = DecodeSettings.ConvertSpace;
            if (m_NeedsTangents)
            {
                decodeSettings |= DecodeSettings.RequireNormalsAndTangents;
            }
            else if (m_NeedsNormals)
            {
                decodeSettings |= DecodeSettings.RequireNormals;
            }
            if (m_MorphTargetsGenerator != null)
            {
                decodeSettings |= DecodeSettings.ForceUnityVertexLayout;
            }
            if (!calculateBounds)
            {
                decodeSettings |= DecodeSettings.DontCalculateBounds;
            }

            return await DracoDecoder.DecodeMesh(data, decodeSettings, GenerateAttributeIdMaps(attributesArray));
        }

        static Dictionary<VertexAttribute, int>[] GenerateAttributeIdMaps(Attributes[] attributesArray)
        {
            var results = new Dictionary<VertexAttribute, int>[attributesArray.Length];
            for (var i = 0; i < attributesArray.Length; i++)
            {
                var attributes = attributesArray[i];
                var result = new Dictionary<VertexAttribute, int>();
                results[i] = result;
                if (attributes.Position.HasValue)
                    result[VertexAttribute.Position] = attributes.Position.Value;
                if (attributes.Normal.HasValue)
                    result[VertexAttribute.Normal] = attributes.Normal.Value;
                if (attributes.Tangent.HasValue)
                    result[VertexAttribute.Tangent] = attributes.Tangent.Value;
                if (attributes.GetColor(0) is { } color)
                    result[VertexAttribute.Color] = color;
                var uvCount = Math.Min(attributes.GetTexCoordsCount(), VertexBufferGeneratorBase.maxUvSetCount);
                for (var uv = 0; uv < uvCount; uv++)
                {
                    if (attributes.GetTexCoord(uv) is { } accessor)
                        result[(VertexAttribute)((int)VertexAttribute.TexCoord0 + uv)] = accessor;
                }
                if (attributes.GetWeight(0) is { } weights)
                    result[VertexAttribute.BlendWeight] = weights;
                if (attributes.GetJoint(0) is { } joints)
                    result[VertexAttribute.BlendIndices] = joints;
            }

            return results;
        }
    }
}
#endif // DRACO_IS_RECENT
