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

using System;
using GLTFast.Vertex;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast
{
#if BURST
    using Unity.Mathematics;
#endif
    using Logging;

    class VertexBufferConfig<VType> :
        VertexBufferConfigBase
        where VType : struct
    {
        NativeArray<VType> m_Data;

        bool m_HasNormals;
        bool m_HasTangents;
        bool m_HasColors;
        bool m_HasBones;

        VertexBufferTexCoordsBase m_TexCoords;
        VertexBufferColors m_Colors;
        VertexBufferBones m_Bones;

        public override int VertexCount
        {
            get
            {
                if (m_Data.IsCreated)
                {
                    return m_Data.Length;
                }
                return 0;
            }
        }

        public VertexBufferConfig(ICodeLogger logger) : base(logger) { }

        public override unsafe JobHandle? ScheduleVertexJobs(
            IGltfBuffers buffers,
            int positionAccessorIndex,
            int normalAccessorIndex,
            int tangentAccessorIndex,
            int[] uvAccessorIndices,
            int colorAccessorIndex,
            int weightsAccessorIndex,
            int jointsAccessorIndex
        )
        {
            buffers.GetAccessor(positionAccessorIndex, out var posAcc, out var posData, out var posByteStride);

            Profiler.BeginSample("ScheduleVertexJobs");
            Profiler.BeginSample("AllocateNativeArray");
            m_Data = new NativeArray<VType>(posAcc.count, defaultAllocator);
            var vDataPtr = (byte*)m_Data.GetUnsafeReadOnlyPtr();
            Profiler.EndSample();

            Bounds = posAcc.TryGetBounds();

            int jobCount = 1;
            int outputByteStride = 12; // sizeof Vector3
            if (posAcc.IsSparse && posAcc.bufferView >= 0)
            {
                jobCount++;
            }
            if (normalAccessorIndex >= 0)
            {
                jobCount++;
                m_HasNormals = true;
            }
            m_HasNormals |= calculateNormals;
            if (m_HasNormals)
            {
                outputByteStride += 12;
            }

            if (tangentAccessorIndex >= 0)
            {
                jobCount++;
                m_HasTangents = true;
            }
            m_HasTangents |= calculateTangents;
            if (m_HasTangents)
            {
                outputByteStride += 16;
            }

            if (uvAccessorIndices != null && uvAccessorIndices.Length > 0)
            {

                // More than two UV sets are not supported yet
                Assert.IsTrue(uvAccessorIndices.Length < 9);

                jobCount += uvAccessorIndices.Length;
                switch (uvAccessorIndices.Length)
                {
                    case 1:
                        m_TexCoords = new VertexBufferTexCoords<VTexCoord1>(m_Logger);
                        break;
                    case 2:
                        m_TexCoords = new VertexBufferTexCoords<VTexCoord2>(m_Logger);
                        break;
                    case 3:
                        m_TexCoords = new VertexBufferTexCoords<VTexCoord3>(m_Logger);
                        break;
                    case 4:
                        m_TexCoords = new VertexBufferTexCoords<VTexCoord4>(m_Logger);
                        break;
                    case 5:
                        m_TexCoords = new VertexBufferTexCoords<VTexCoord5>(m_Logger);
                        break;
                    case 6:
                        m_TexCoords = new VertexBufferTexCoords<VTexCoord6>(m_Logger);
                        break;
                    case 7:
                        m_TexCoords = new VertexBufferTexCoords<VTexCoord7>(m_Logger);
                        break;
                    default:
                        m_TexCoords = new VertexBufferTexCoords<VTexCoord8>(m_Logger);
                        break;
                }
            }

            m_HasColors = colorAccessorIndex >= 0;
            if (m_HasColors)
            {
                jobCount++;
                m_Colors = new VertexBufferColors();
            }

            m_HasBones = weightsAccessorIndex >= 0 && jointsAccessorIndex >= 0;
            if (m_HasBones)
            {
                jobCount++;
                m_Bones = new VertexBufferBones(m_Logger);
            }

            NativeArray<JobHandle> handles = new NativeArray<JobHandle>(jobCount, defaultAllocator);
            int handleIndex = 0;

            {
                JobHandle? h = null;
                if (posAcc.bufferView >= 0)
                {
                    h = GetVector3Job(
                        posData,
                        posAcc.count,
                        posAcc.componentType,
                        posByteStride,
                        (float3*)vDataPtr,
                        outputByteStride,
                        posAcc.normalized,
                        false // positional data never needs to be normalized
                    );
                }
                if (posAcc.IsSparse)
                {
                    buffers.GetAccessorSparseIndices(posAcc.sparse.indices, out var posIndexData);
                    buffers.GetAccessorSparseValues(posAcc.sparse.values, out var posValueData);
                    var sparseJobHandle = GetVector3SparseJob(
                        posIndexData,
                        posValueData,
                        posAcc.sparse.count,
                        posAcc.sparse.indices.componentType,
                        posAcc.componentType,
                        (float3*)vDataPtr,
                        outputByteStride,
                        dependsOn: ref h,
                        posAcc.normalized
                    );
                    if (sparseJobHandle.HasValue)
                    {
                        handles[handleIndex] = sparseJobHandle.Value;
                        handleIndex++;
                    }
                    else
                    {
                        Profiler.EndSample();
                        return null;
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
                    return null;
                }
            }

            if (normalAccessorIndex >= 0)
            {
                buffers.GetAccessor(normalAccessorIndex, out var nrmAcc, out var input, out var inputByteStride);
                if (nrmAcc.IsSparse)
                {
                    m_Logger.Error(LogCode.SparseAccessor, "normals");
                }
                var h = GetVector3Job(
                    input,
                    nrmAcc.count,
                    nrmAcc.componentType,
                    inputByteStride,
                    (float3*)(vDataPtr + 12),
                    outputByteStride,
                    nrmAcc.normalized
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
                    return null;
                }
            }

            if (tangentAccessorIndex >= 0)
            {
                buffers.GetAccessor(tangentAccessorIndex, out var tanAcc, out var input, out var inputByteStride);
                if (tanAcc.IsSparse)
                {
                    m_Logger.Error(LogCode.SparseAccessor, "tangents");
                }
                var h = GetTangentsJob(
                    input,
                    tanAcc.count,
                    tanAcc.componentType,
                    inputByteStride,
                    (float4*)(vDataPtr + 24),
                    outputByteStride,
                    tanAcc.normalized
                );
                if (h.HasValue)
                {
                    handles[handleIndex] = h.Value;
                    handleIndex++;
                }
                else
                {
                    Profiler.EndSample();
                    return null;
                }
            }

            if (m_TexCoords != null)
            {
                m_TexCoords.ScheduleVertexUVJobs(
                    buffers,
                    uvAccessorIndices,
                    posAcc.count,
                    new NativeSlice<JobHandle>(
                        handles,
                        handleIndex,
                        uvAccessorIndices.Length
                        )
                    );
                handleIndex += uvAccessorIndices.Length;
            }

            if (m_HasColors)
            {
                m_Colors.ScheduleVertexColorJob(
                    buffers,
                    colorAccessorIndex,
                    new NativeSlice<JobHandle>(
                        handles,
                        handleIndex,
                        1
                        )
                    );
                handleIndex++;
            }

            if (m_HasBones)
            {
                var h = m_Bones.ScheduleVertexBonesJob(
                    buffers,
                    weightsAccessorIndex,
                    jointsAccessorIndex
                );
                if (h.HasValue)
                {
                    handles[handleIndex] = h.Value;
                }
                else
                {
                    Profiler.EndSample();
                    return null;
                }
            }

            var handle = (jobCount > 1) ? JobHandle.CombineDependencies(handles) : handles[0];
            handles.Dispose();
            Profiler.EndSample();
            return handle;
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

        public override void ApplyOnMesh(Mesh msh, MeshUpdateFlags flags = PrimitiveCreateContextBase.defaultMeshUpdateFlags)
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

        public override void Dispose()
        {
            if (m_Data.IsCreated)
            {
                m_Data.Dispose();
            }

            if (m_Colors != null)
            {
                m_Colors.Dispose();
            }

            if (m_TexCoords != null)
            {
                m_TexCoords.Dispose();
            }

            if (m_Bones != null)
            {
                m_Bones.Dispose();
            }
        }
    }
}
