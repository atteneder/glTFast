// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Jobs;
using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Objects;
using Unity.Cloud.Gltfast.Vertex;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Unity.Cloud.Gltfast
{
    sealed class VertexBufferBones : IDisposable
    {
        readonly ICodeLogger m_Logger;

        NativeArray<VBones> m_Data;

        public VertexBufferBones(int vertexCount, ICodeLogger logger)
        {
            m_Logger = logger;
            Profiler.BeginSample("AllocateNativeArray");
            m_Data = new NativeArray<VBones>(vertexCount, VertexBufferGeneratorBase.defaultAllocator);
            Profiler.EndSample();
        }

        public unsafe JobHandle? ScheduleVertexBonesJob(
            int weightsAccessorIndex,
            int jointsAccessorIndex,
            int offset,
            BufferStore buffers
        )
        {
            Profiler.BeginSample("ScheduleVertexBonesJob");

            buffers.GetAccessorAndData(weightsAccessorIndex, out var weightsAcc, out var weightsData, out var weightsByteStride);
            if (weightsAcc == null)
            {
                m_Logger?.Error(LogCode.AccessorAccessFailed, weightsAccessorIndex.ToString());
                Profiler.EndSample();
                return null;
            }
            if (weightsAcc.IsSparse)
            {
                m_Logger?.Error(LogCode.SparseAccessor, "bone weights");
            }
            // Ignoring NativeArray safety here, so that multiple jobs (one per sub-mesh) can write to the same array.
            var vDataPtr = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(m_Data);

            JobHandle weightsHandle;
            JobHandle jointsHandle;

            {
                var h = GetWeightsJob(
                    weightsData,
                    weightsAcc.Count,
                    weightsAcc.ComponentType,
                    weightsByteStride,
                    (float4*)(vDataPtr + offset * sizeof(VBones)),
                    32
                );
                if (h.HasValue)
                {
                    weightsHandle = h.Value;
                }
                else
                {
                    Profiler.EndSample();
                    return null;
                }
            }

            {
                buffers.GetAccessorAndData(jointsAccessorIndex, out var jointsAcc, out var jointsData, out var jointsByteStride);
                if (jointsAcc == null)
                {
                    m_Logger?.Error(LogCode.AccessorAccessFailed, jointsAccessorIndex.ToString());
                    Profiler.EndSample();
                    return null;
                }
                if (jointsAcc.IsSparse)
                {
                    m_Logger?.Error(LogCode.SparseAccessor, "bone joints");
                }
                var h = GetJointsJob(
                    jointsData,
                    jointsAcc.Count,
                    jointsAcc.ComponentType,
                    jointsByteStride,
                    (uint4*)(vDataPtr + offset * sizeof(VBones) + sizeof(float4)),
                    32,
                    m_Logger
                );
                if (h.HasValue)
                {
                    jointsHandle = h.Value;
                }
                else
                {
                    Profiler.EndSample();
                    return null;
                }
            }

            Profiler.EndSample();
            return JobHandle.CombineDependencies(weightsHandle, jointsHandle);
        }

        public JobHandle ScheduleSortAndNormalizeBoneWeightsJob(JobHandle dependsOn)
        {
            var skinWeights = (int)QualitySettings.skinWeights;

#if UNITY_EDITOR
            // If this is design-time import, fix and import all weights.
            if (!UnityEditor.EditorApplication.isPlaying || skinWeights < 4)
            {
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    skinWeights = 4;
                }
#else
            if (skinWeights < 4)
            {
#endif
                return new SortAndNormalizeBoneWeightsJob
                {
                    bones = m_Data,
                    skinWeights = math.max(1, skinWeights)
                }.Schedule(m_Data.Length, GltfImport.DefaultBatchCount, dependsOn);
            }
#if GLTFAST_SAFE
            // Re-normalizing alone is sufficient
            return new RenormalizeBoneWeightsJob {
                bones = m_Data,
            }.Schedule(m_Data.Length, GltfImport.DefaultBatchCount, dependsOn);
#else
            return dependsOn;
#endif
        }

        public void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream)
        {
            dst[offset] = new VertexAttributeDescriptor(VertexAttribute.BlendWeight, VertexAttributeFormat.Float32, 4, stream);
            dst[offset + 1] = new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt32, 4, stream);
        }

        public void ApplyOnMesh(
            UnityEngine.Mesh msh,
            int stream,
            MeshUpdateFlags flags = MeshGeneratorBase.defaultMeshUpdateFlags
            )
        {
            Profiler.BeginSample("ApplyBones");
            msh.SetVertexBufferData(m_Data, 0, 0, m_Data.Length, stream, flags);
            Profiler.EndSample();
        }

        public void Dispose()
        {
            if (m_Data.IsCreated)
            {
                m_Data.Dispose();
            }
        }

        unsafe JobHandle? GetWeightsJob(
            void* input,
            int count,
            AccessorDataType inputType,
            int? inputByteStride,
            float4* output,
            int outputByteStride
            )
        {
            Profiler.BeginSample("GetWeightsJob");
            JobHandle? jobHandle;
            switch (inputType)
            {
                case AccessorDataType.Float:
                    var jobTangentI = new ConvertBoneWeightsFloatToFloatInterleavedJob
                    {
                        inputByteStride = inputByteStride ?? sizeof(float4),
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobTangentI.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    break;
                case AccessorDataType.UnsignedShort:
                {
                    var job = new ConvertBoneWeightsUInt16ToFloatInterleavedJob
                    {
                        inputByteStride = inputByteStride ?? 4 * sizeof(ushort),
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = job.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    break;
                }
                case AccessorDataType.UnsignedByte:
                {
                    var job = new ConvertBoneWeightsUInt8ToFloatInterleavedJob
                    {
                        inputByteStride = inputByteStride ?? 4 * sizeof(byte),
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = job.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    break;
                }
                default:
                    m_Logger?.Error(LogCode.TypeUnsupported, "Weights", inputType.ToString());
                    jobHandle = null;
                    break;
            }

            Profiler.EndSample();
            return jobHandle;
        }

        static unsafe JobHandle? GetJointsJob(
            void* input,
            int count,
            AccessorDataType inputType,
            int? inputByteStride,
            uint4* output,
            int outputByteStride,
            ICodeLogger logger
        )
        {
            Profiler.BeginSample("GetJointsJob");
            JobHandle? jobHandle;
            switch (inputType)
            {
                case AccessorDataType.UnsignedByte:
                    var jointsUInt8Job = new ConvertBoneJointsUInt8ToUInt32Job
                    {
                        inputByteStride = inputByteStride ?? 4 * sizeof(byte),
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jointsUInt8Job.Schedule(count, GltfImport.DefaultBatchCount);
                    break;
                case AccessorDataType.UnsignedShort:
                    var jointsUInt16Job = new ConvertBoneJointsUInt16ToUInt32Job
                    {
                        inputByteStride = inputByteStride ?? 4 * sizeof(ushort),
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jointsUInt16Job.Schedule(count, GltfImport.DefaultBatchCount);
                    break;
                default:
                    logger?.Error(LogCode.TypeUnsupported, "Joints", inputType.ToString());
                    jobHandle = null;
                    break;
            }

            Profiler.EndSample();
            return jobHandle;
        }
    }
}
