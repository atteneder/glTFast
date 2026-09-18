// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Objects;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Mesh = UnityEngine.Mesh;

namespace Unity.Cloud.Gltfast
{

    class MorphTargetsGenerator
    {
        readonly IReadOnlyList<string> m_MorphTargetNames;
        readonly BufferStore m_Buffers;
        readonly IDeferAgent m_DeferAgent;

        MorphTargetGenerator[] m_Contexts;
        NativeArray<JobHandle> m_Handles;

        public MorphTargetsGenerator(
            int vertexCount,
            int subMeshCount,
            int morphTargetCount,
            IReadOnlyList<string> morphTargetNames,
            bool hasNormals,
            bool hasTangents,
            BufferStore buffers,
            IDeferAgent deferAgent,
            ICodeLogger logger
            )
        {
            m_MorphTargetNames = morphTargetNames;
            m_Buffers = buffers;
            m_DeferAgent = deferAgent;

            m_Contexts = new MorphTargetGenerator[morphTargetCount];
            for (var i = 0; i < morphTargetCount; i++)
            {
                m_Contexts[i] = new MorphTargetGenerator(vertexCount, hasNormals, hasTangents);
            }
            m_Handles = new NativeArray<JobHandle>(morphTargetCount * subMeshCount, VertexBufferGeneratorBase.defaultAllocator);
        }

        public bool AddMorphTarget(
            int offset,
            int subMesh,
            int morphTargetIndex,
            MorphTarget morphTarget,
            ICodeLogger logger
            )
        {
            var morphTargetGenerator = m_Contexts[morphTargetIndex];
            var jobHandle = morphTargetGenerator.ScheduleMorphTargetJobs(
                morphTarget,
                offset,
                m_Buffers,
                logger
                );
            if (jobHandle.HasValue)
            {
                m_Handles[subMesh * m_Contexts.Length + morphTargetIndex] = jobHandle.Value;
            }
            else
            {
                return false;
            }
            return true;
        }

        public JobHandle GetJobHandle()
        {
            var handle = m_Handles.Length > 1 ? JobHandle.CombineDependencies(m_Handles) : m_Handles[0];
            m_Handles.Dispose();
            return handle;
        }

        public async Task ApplyOnMeshAndDisposeAsync(Mesh mesh)
        {
            for (var index = 0; index < m_Contexts.Length; index++)
            {
                var context = m_Contexts[index];
                context.AddToMesh(mesh, m_MorphTargetNames?[index] ?? index.ToString());
                context.Dispose();
                await m_DeferAgent.BreakPointAsync();
            }
            m_Contexts = null;
        }
    }

    sealed class MorphTargetGenerator : IDisposable
    {
        Vector3[] m_Positions;
        Vector3[] m_Normals;
        Vector3[] m_Tangents;

        GCHandle m_PositionsHandle;
        GCHandle m_NormalsHandle;
        GCHandle m_TangentsHandle;

        public MorphTargetGenerator(int vertexCount, bool hasNormals, bool hasTangents)
        {
            m_Positions = new Vector3[vertexCount];
            m_PositionsHandle = GCHandle.Alloc(m_Positions, GCHandleType.Pinned);

            if (hasNormals)
            {
                m_Normals = new Vector3[vertexCount];
                m_NormalsHandle = GCHandle.Alloc(m_Normals, GCHandleType.Pinned);
            }

            if (hasTangents)
            {
                m_Tangents = new Vector3[vertexCount];
                m_TangentsHandle = GCHandle.Alloc(m_Tangents, GCHandleType.Pinned);
            }
        }

        public unsafe JobHandle? ScheduleMorphTargetJobs(
            MorphTarget morphTarget,
            int offset,
            BufferStore buffers,
            ICodeLogger logger
        )
        {
            Profiler.BeginSample("ScheduleMorphTargetJobs");

            buffers.GetAccessorAndData(
                morphTarget.Position.Value,
                out var posAcc,
                out var posData,
                out _
                );

            var jobCount = 1;
            if (posAcc.IsSparse && posAcc.BufferView.HasValue)
                jobCount++;

            Accessor nrmAcc = null;
            void* nrmInput = null;
            int? nrmInputByteStride = null;

            if (morphTarget.Normal.HasValue)
            {
                buffers.GetAccessorAndData(morphTarget.Normal.Value, out nrmAcc, out nrmInput, out nrmInputByteStride);
                jobCount += nrmAcc.IsSparse && nrmAcc.BufferView.HasValue ? 2 : 1;
            }

            Accessor tanAcc = null;
            void* tanInput = null;
            int? tanInputByteStride = null;

            if (morphTarget.Tangent.HasValue)
            {
                buffers.GetAccessorAndData(morphTarget.Tangent.Value, out tanAcc, out tanInput, out tanInputByteStride);
                jobCount += tanAcc.IsSparse && tanAcc.BufferView.HasValue ? 2 : 1;
            }

            var handles = new NativeArray<JobHandle>(jobCount, VertexBufferGeneratorBase.defaultAllocator);
            var handleIndex = 0;

            if (!SchedulePositionsJobs(
                    offset, buffers, posData, morphTarget.Position.Value,
                    posAcc, handles, ref handleIndex, logger)
               )
            {
                return null;
            }

            if (nrmAcc != null
                && !ScheduleNormalsJobs(
                    offset,
                    buffers,
                    morphTarget.Normal.Value,
                    nrmAcc,
                    nrmInput,
                    nrmInputByteStride,
                    handles,
                    ref handleIndex,
                    logger)
                )
            {
                return null;
            }

            if (tanAcc != null
                && !ScheduleTangentsJobs(
                    offset, buffers, morphTarget.Tangent.Value, tanAcc, tanInput, tanInputByteStride,
                    handles, handleIndex, logger)
                )
            {
                return null;
            }

            var handle = jobCount > 1 ? JobHandle.CombineDependencies(handles) : handles[0];
            handles.Dispose();
            Profiler.EndSample();
            return handle;
        }

        unsafe bool SchedulePositionsJobs(
            int offset,
            BufferStore buffers,
            void* posData,
            int accessorIndex,
            Accessor posAcc,
            NativeArray<JobHandle> handles,
            ref int handleIndex,
            ICodeLogger logger
            )
        {
            fixed (void* dest = &m_Positions[offset])
            {
                JobHandle? h = null;
                if (posData != null)
                {
                    h = VertexBufferGeneratorBase.GetVector3Job(
                        buffers,
                        accessorIndex,
                        posAcc,
                        (float3*)dest,
                        12,
                        logger,
                        posAcc.Normalized,
                        false // positional data never needs to be normalized
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
                }
                if (posAcc.IsSparse)
                {
                    buffers.GetAccessorSparseIndices(posAcc.Sparse.Indices, out var posIndexData);
                    buffers.GetAccessorSparseValues(posAcc.Sparse.Values, out var posValueData);
                    var sparseJobHandle = VertexBufferGeneratorBase.GetVector3SparseJob(
                        posIndexData,
                        posValueData,
                        posAcc.Sparse.Count,
                        posAcc.Sparse.Indices.ComponentType,
                        posAcc.ComponentType,
                        (float3*)dest,
                        12,
                        dependsOn: ref h,
                        posAcc.Normalized
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
            }

            return true;
        }

        unsafe bool ScheduleNormalsJobs(
            int offset,
            BufferStore buffers,
            int normalsIndex,
            Accessor nrmAcc,
            void* nrmInput,
            int? nrmInputByteStride,
            NativeArray<JobHandle> handles,
            ref int handleIndex,
            ICodeLogger logger
            )
        {
            fixed (void* dest = &(m_Normals[offset]))
            {
                JobHandle? h = null;
                if (nrmAcc.BufferView.HasValue)
                {
                    h = VertexBufferGeneratorBase.GetVector3Job(
                        buffers,
                        normalsIndex,
                        nrmAcc,
                        (float3*)dest,
                        12,
                        logger,
                        nrmAcc.Normalized,
                        false // morph target normals are deltas -> don't normalize
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
                }
                if (nrmAcc.IsSparse)
                {
                    buffers.GetAccessorSparseIndices(nrmAcc.Sparse.Indices, out var indexData);
                    buffers.GetAccessorSparseValues(nrmAcc.Sparse.Values, out var valueData);
                    var sparseJobHandle = VertexBufferGeneratorBase.GetVector3SparseJob(
                        indexData,
                        valueData,
                        nrmAcc.Sparse.Count,
                        nrmAcc.Sparse.Indices.ComponentType,
                        nrmAcc.ComponentType,
                        (float3*)dest,
                        12,
                        dependsOn: ref h,
                        nrmAcc.Normalized
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
            }

            return true;
        }

        unsafe bool ScheduleTangentsJobs(
            int offset,
            BufferStore buffers,
            int tangentsIndex,
            Accessor tanAcc,
            void* tanInput,
            int? tanInputByteStride,
            NativeArray<JobHandle> handles,
            int handleIndex,
            ICodeLogger logger
            )
        {
            fixed (void* dest = &(m_Tangents[offset]))
            {
                JobHandle? h = null;
                if (tanAcc.BufferView.HasValue)
                {
                    h = VertexBufferGeneratorBase.GetVector3Job(
                        buffers,
                        tangentsIndex,
                        tanAcc,
                        (float3*)dest,
                        12,
                        logger,
                        tanAcc.Normalized,
                        false // morph target tangents are deltas -> don't normalize
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
                }
                if (tanAcc.IsSparse)
                {
                    buffers.GetAccessorSparseIndices(tanAcc.Sparse.Indices, out var indexData);
                    buffers.GetAccessorSparseValues(tanAcc.Sparse.Values, out var valueData);
                    var sparseJobHandle = VertexBufferGeneratorBase.GetVector3SparseJob(
                        indexData,
                        valueData,
                        tanAcc.Sparse.Count,
                        tanAcc.Sparse.Indices.ComponentType,
                        tanAcc.ComponentType,
                        (float3*)dest,
                        12,
                        dependsOn: ref h,
                        tanAcc.Normalized
                    );
                    if (sparseJobHandle.HasValue)
                    {
                        handles[handleIndex] = sparseJobHandle.Value;
                    }
                    else
                    {
                        Profiler.EndSample();
                        return false;
                    }
                }
            }

            return true;
        }

        public void AddToMesh(Mesh mesh, string name)
        {
            Profiler.BeginSample("AddBlendShapeFrame");
            mesh.AddBlendShapeFrame(name, 1f, m_Positions, m_Normals, m_Tangents);
            Profiler.EndSample();
        }

        public void Dispose()
        {
            m_PositionsHandle.Free();
            m_Positions = null;
            if (m_Normals != null)
            {
                m_NormalsHandle.Free();
                m_Normals = null;
            }
            if (m_Tangents != null)
            {
                m_TangentsHandle.Free();
                m_Tangents = null;
            }
        }
    }
}
