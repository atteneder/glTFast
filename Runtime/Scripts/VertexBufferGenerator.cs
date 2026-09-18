// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Objects;
using Unity.Cloud.Gltfast.Vertex;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Mesh = UnityEngine.Mesh;

namespace Unity.Cloud.Gltfast
{
    class VertexBufferGenerator<TMainBuffer> :
        VertexBufferGeneratorBase
        where TMainBuffer : unmanaged
    {
        NativeArray<TMainBuffer> m_Data;

        bool m_HasNormals;
        bool m_HasTangents;
        bool m_HasColors;
        bool m_HasBones;

        VertexBufferTexCoordsBase m_TexCoords;
        VertexBufferColors m_Colors;
        VertexBufferBones m_Bones;

        Accessor[] m_PositionAccessors;

        public override int VertexCount => VertexIntervals != null ? VertexIntervals[VertexIntervals.Length - 1] : 0;

        public override int[] VertexIntervals { get; protected set; }

        public override void GetVertexRange(int subMesh, out int baseVertex, out int vertexCount)
        {
            Assert.IsNotNull(VertexIntervals);
            Assert.IsTrue(subMesh >= 0);
            Assert.IsTrue(subMesh < VertexIntervals.Length);

            baseVertex = VertexIntervals[subMesh];
            vertexCount = VertexIntervals[subMesh + 1] - baseVertex;
        }

        public override bool TryGetBounds(int subMesh, ICodeLogger logger, out Bounds bounds)
        {
            Assert.IsNotNull(m_PositionAccessors);
            var boundsOpt = m_PositionAccessors[subMesh].TryGetBounds();
            if (boundsOpt.HasValue)
            {
                bounds = boundsOpt.Value;
                return true;
            }
            logger?.Error(LogCode.MeshBoundsMissing, m_Attributes[subMesh].Position.ToString());
            bounds = default;
            return false;
        }

        public VertexBufferGenerator(int primitiveCount, BufferStore buffers, ICodeLogger logger)
            : base(primitiveCount, buffers, logger)
        { }

        public override void AddPrimitive(Attributes att)
        {
            m_Attributes[m_AttributeCount++] = att;
        }

        public override void Initialize()
        {
            Assert.AreEqual(m_Attributes.Length, m_AttributeCount);
            var vertexCount = 0;
            m_PositionAccessors = new Accessor[m_Attributes.Length];
            VertexIntervals = new int[m_Attributes.Length + 1];
            for (var i = 0; i < m_Attributes.Length; i++)
            {
                VertexIntervals[i] = vertexCount;
                m_PositionAccessors[i] = m_Buffers.GetAccessor(m_Attributes[i].Position.Value);
                vertexCount += m_PositionAccessors[i].Count;
            }
            VertexIntervals[m_Attributes.Length] = vertexCount;
        }

        public override async Task<bool> CreateVertexBufferAsync()
        {
            var jh = CreateVertexBufferHandle();
            if (!jh.HasValue)
                return false;

            while (!jh.Value.IsCompleted)
            {
                await Task.Yield();
            }
            jh.Value.Complete();
            return true;
        }

        unsafe JobHandle? CreateVertexBufferHandle()
        {
            Profiler.BeginSample("AllocateNativeArray");
            m_Data = new NativeArray<TMainBuffer>(VertexCount, defaultAllocator);
            var vDataPtr = (byte*)m_Data.GetUnsafeReadOnlyPtr();
            Profiler.EndSample();

            var jobCount = 0;

            var firstAttributes = m_Attributes[0];

            var uvSetCount = firstAttributes.GetTexCoordsCount();
            if (uvSetCount > 0)
            {
                if (uvSetCount > maxUvSetCount)
                {
                    // More than eight UV sets are not supported yet
                    m_Logger?.Warning(LogCode.UVLimit);
                    uvSetCount = maxUvSetCount;
                }

                jobCount += uvSetCount * m_Attributes.Length;
                m_TexCoords = uvSetCount switch
                {
                    1 => new VertexBufferTexCoords<VTexCoord1>(uvSetCount, VertexCount, m_Logger),
                    2 => new VertexBufferTexCoords<VTexCoord2>(uvSetCount, VertexCount, m_Logger),
                    3 => new VertexBufferTexCoords<VTexCoord3>(uvSetCount, VertexCount, m_Logger),
                    4 => new VertexBufferTexCoords<VTexCoord4>(uvSetCount, VertexCount, m_Logger),
                    5 => new VertexBufferTexCoords<VTexCoord5>(uvSetCount, VertexCount, m_Logger),
                    6 => new VertexBufferTexCoords<VTexCoord6>(uvSetCount, VertexCount, m_Logger),
                    7 => new VertexBufferTexCoords<VTexCoord7>(uvSetCount, VertexCount, m_Logger),
                    _ => new VertexBufferTexCoords<VTexCoord8>(uvSetCount, VertexCount, m_Logger)
                };
            }

            m_HasColors = firstAttributes.GetColor(0).HasValue;
            if (m_HasColors)
            {
                jobCount += m_Attributes.Length;
                m_Colors = new VertexBufferColors(VertexCount, m_Logger);
            }

            m_HasBones = firstAttributes.GetWeight(0).HasValue && firstAttributes.GetJoint(0).HasValue;
            if (m_HasBones)
            {
                jobCount++;
                m_Bones = new VertexBufferBones(VertexCount, m_Logger);
            }

            for (var i = 0; i < m_Attributes.Length; i++)
            {
                jobCount += 1; // Positions

                var att = m_Attributes[i];

                if (m_PositionAccessors[i].IsSparse && m_PositionAccessors[i].BufferView is >= 0)
                    jobCount++;

                if (att.Normal >= 0)
                {
                    jobCount++;
                    m_HasNormals = true;
                }

                m_HasNormals |= calculateNormals;

                if (att.Tangent >= 0)
                {
                    jobCount++;
                    m_HasTangents = true;
                }

                m_HasTangents |= calculateTangents;
            }

            var handles = new NativeArray<JobHandle>(jobCount, defaultAllocator);
            var handleIndex = 0;
            var outputByteStride = Marshal.SizeOf(typeof(TMainBuffer));

            for (var i = 0; i < m_Attributes.Length; i++)
            {
                var att = m_Attributes[i];
                if (!SchedulePositionsJobs(i, vDataPtr, outputByteStride, handles, ref handleIndex))
                    return null;

                if (att.Normal >= 0
                    && !ScheduleNormalsJobs(att, vDataPtr, outputByteStride, i, handles, ref handleIndex, m_Logger)
                    )
                    return null;

                if (att.Tangent >= 0
                    && !ScheduleTangentsJobs(att, vDataPtr, outputByteStride, i, handles, ref handleIndex)
                   )
                    return null;

                if (m_TexCoords != null)
                {
                    handleIndex = ScheduleTexCoordJobs(att, uvSetCount, i, handles, handleIndex);
                }

                if (m_HasColors && !ScheduleColorsJobs(att, i, handles, ref handleIndex))
                    return null;
            }
            if (m_HasBones && !ScheduleVertexBonesJobs(m_Attributes, handles.GetSubArray(handleIndex, 1)))
                return null;

            var handle = jobCount > 1 ? JobHandle.CombineDependencies(handles) : handles[0];
            handles.Dispose();
            return handle;
        }

        unsafe bool SchedulePositionsJobs(int i, byte* vDataPtr, int outputByteStride, NativeArray<JobHandle> handles, ref int handleIndex)
        {
            JobHandle? h = null;

            if (m_PositionAccessors[i].BufferView is >= 0)
            {
                h = GetVector3Job(
                    m_Buffers,
                    m_Attributes[i].Position.Value,
                    m_PositionAccessors[i],
                    (float3*)(vDataPtr + outputByteStride * VertexIntervals[i]),
                    outputByteStride,
                    m_Logger,
                    m_PositionAccessors[i].Normalized,
                    false // positional data never needs to be normalized
                );
            }

            if (m_PositionAccessors[i].IsSparse)
            {
                m_Buffers.GetAccessorSparseIndices(m_PositionAccessors[i].Sparse.Indices, out var posIndexData);
                m_Buffers.GetAccessorSparseValues(m_PositionAccessors[i].Sparse.Values, out var posValueData);
                var sparseJobHandle = GetVector3SparseJob(
                    posIndexData,
                    posValueData,
                    m_PositionAccessors[i].Sparse.Count,
                    m_PositionAccessors[i].Sparse.Indices.ComponentType,
                    m_PositionAccessors[i].ComponentType,
                    (float3*)(vDataPtr + outputByteStride * VertexIntervals[i]),
                    outputByteStride,
                    dependsOn: ref h,
                    m_PositionAccessors[i].Normalized
                );
                if (sparseJobHandle.HasValue)
                {
                    handles[handleIndex] = sparseJobHandle.Value;
                    handleIndex++;
                }
                else
                {
                    Profiler.EndSample();
                    return false;
                }
            }

            if (h.HasValue)
            {
                handles[handleIndex] = h.Value;
                handleIndex++;
            }
            else
            {
                Profiler.EndSample();
                return false;
            }

            return true;
        }

        unsafe bool ScheduleNormalsJobs(
            Attributes att,
            byte* vDataPtr,
            int outputByteStride,
            int i,
            NativeArray<JobHandle> handles,
            ref int handleIndex,
            ICodeLogger logger
            )
        {
            m_Buffers.GetAccessorAndData(
                att.Normal.Value,
                out var nrmAcc,
                out var input,
                out var inputByteStride
            );
            if (nrmAcc.IsSparse)
            {
                m_Logger?.Error(LogCode.SparseAccessor, "normals");
            }

            var h = GetVector3Job(
                m_Buffers,
                att.Normal.Value,
                nrmAcc,
                (float3*)(vDataPtr + outputByteStride * VertexIntervals[i] + 12),
                outputByteStride,
                logger,
                nrmAcc.Normalized

            //, normals need to be unit length
            );
            if (h.HasValue)
            {
                handles[handleIndex] = h.Value;
                handleIndex++;
            }
            else
            {
                Profiler.EndSample();
                return false;
            }

            return true;
        }

        unsafe bool ScheduleTangentsJobs(Attributes att, byte* vDataPtr, int outputByteStride, int i, NativeArray<JobHandle> handles, ref int handleIndex)
        {
            m_Buffers.GetAccessorAndData(
                att.Tangent.Value,
                out var tanAcc,
                out var input,
                out var inputByteStride
            );
            if (tanAcc.IsSparse)
            {
                m_Logger?.Error(LogCode.SparseAccessor, "tangents");
            }

            var h = GetTangentsJob(
                input,
                tanAcc.Count,
                tanAcc.ComponentType,
                inputByteStride,
                (float4*)(vDataPtr + outputByteStride * VertexIntervals[i] + 24),
                outputByteStride,
                tanAcc.Normalized
            );
            if (h.HasValue)
            {
                handles[handleIndex] = h.Value;
                handleIndex++;
            }
            else
            {
                Profiler.EndSample();
                return false;
            }

            return true;
        }

        int ScheduleTexCoordJobs(Attributes att, int uvSetCount, int i, NativeArray<JobHandle> handles, int handleIndex)
        {
            var uvAccessors = new int[uvSetCount];
            for (var uv = 0; uv < uvSetCount; uv++)
            {
                uvAccessors[uv] = att.GetTexCoord(uv) ?? -1;
            }

            m_TexCoords.ScheduleVertexUVJobs(
                VertexIntervals[i],
                uvAccessors,
                handles.GetSubArray(handleIndex, uvAccessors.Length),
                m_Buffers
            );
            handleIndex += uvAccessors.Length;
            return handleIndex;
        }

        bool ScheduleColorsJobs(Attributes att, int i, NativeArray<JobHandle> handles, ref int handleIndex)
        {
            var success = m_Colors.ScheduleVertexColorJob(
                att.GetColor(0).Value,
                VertexIntervals[i],
                handles.GetSubArray(handleIndex, 1),
                m_Buffers
            );
            if (!success)
            {
                Profiler.EndSample();
                return false;
            }
            handleIndex++;
            return true;
        }

        bool ScheduleVertexBonesJobs(Attributes[] attributes, NativeArray<JobHandle> handles)
        {
            if (attributes.Length > 1)
            {
                var boneHandles = new NativeArray<JobHandle>(attributes.Length, Allocator.Temp);
                for (var i = 0; i < attributes.Length; i++)
                {
                    if (!ScheduleVertexBonesJob(i, out var boneHandle))
                        return false;
                    boneHandles[i] = boneHandle;
                }
                handles[0] = JobHandle.CombineDependencies(boneHandles);
                boneHandles.Dispose();
            }
            else
            {
                if (!ScheduleVertexBonesJob(0, out var boneHandle))
                    return false;
                handles[0] = boneHandle;
            }
            handles[0] = m_Bones.ScheduleSortAndNormalizeBoneWeightsJob(handles[0]);
            return true;

            bool ScheduleVertexBonesJob(int i, out JobHandle handle)
            {
                var att = attributes[i];

                var h = m_Bones.ScheduleVertexBonesJob(
                    att.GetWeight(0).Value,
                    att.GetJoint(0).Value,
                    VertexIntervals[i],
                    m_Buffers
                );
                if (!h.HasValue)
                {
                    handle = default;
                    return false;
                }

                handle = h.Value;
                return true;
            }
        }

        void CreateDescriptors()
        {
            int vadLen = 1;
            if (m_HasNormals) vadLen++;
            if (m_HasTangents) vadLen++;
            if (m_TexCoords != null) vadLen += m_TexCoords.UVSetCount;
            if (m_Colors != null) vadLen++;
            if (m_Bones != null) vadLen += 2;
            m_Descriptors = new VertexAttributeDescriptor[vadLen];
            var vadCount = 0;
            int stream = 0;
            m_Descriptors[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, stream);
            vadCount++;
            if (m_HasNormals)
            {
                m_Descriptors[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, stream);
                vadCount++;
            }
            if (m_HasTangents)
            {
                m_Descriptors[vadCount] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, stream);
                vadCount++;
            }
            stream++;

            if (m_Colors != null)
            {
                m_Colors.AddDescriptors(m_Descriptors, vadCount, stream);
                vadCount++;
                stream++;
            }

            if (m_TexCoords != null)
            {
                m_TexCoords.AddDescriptors(m_Descriptors, ref vadCount, stream);
                stream++;
            }

            if (m_Bones != null)
            {
                m_Bones.AddDescriptors(m_Descriptors, vadCount, stream);
                // vadCount+=2;
                // stream++;
            }
        }

        public override void ApplyOnMesh(Mesh msh, MeshUpdateFlags flags = MeshGeneratorBase.defaultMeshUpdateFlags)
        {

            Profiler.BeginSample("ApplyOnMesh");
            if (m_Descriptors == null)
            {
                CreateDescriptors();
            }

            Profiler.BeginSample("SetVertexBufferParams");
            msh.SetVertexBufferParams(m_Data.Length, m_Descriptors);
            Profiler.EndSample();

            Profiler.BeginSample("SetVertexBufferData");
            int stream = 0;
            msh.SetVertexBufferData(m_Data, 0, 0, m_Data.Length, stream, flags);
            stream++;
            Profiler.EndSample();

            if (m_Colors != null)
            {
                m_Colors.ApplyOnMesh(msh, stream, flags);
                stream++;
            }

            if (m_TexCoords != null)
            {
                m_TexCoords.ApplyOnMesh(msh, stream, flags);
                stream++;
            }

            if (m_Bones != null)
            {
                m_Bones.ApplyOnMesh(msh, stream, flags);
                // stream++;
            }

            Profiler.EndSample();
        }

        protected override void Dispose(bool disposing)
        {
            if (m_Data.IsCreated)
            {
                m_Data.Dispose();
            }

            if (disposing)
            {
                m_Colors?.Dispose();
                m_TexCoords?.Dispose();
                m_Bones?.Dispose();
            }
        }
    }
}
