// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast
{

    using Logging;
    using Schema;
    using Vertex;

    abstract class VertexBufferBonesBase
    {

        protected ICodeLogger m_Logger;

        protected VertexBufferBonesBase(ICodeLogger logger)
        {
            m_Logger = logger;
        }

        public abstract JobHandle? ScheduleVertexBonesJob(
            IGltfBuffers buffers,
            int weightsAccessorIndex,
            int jointsAccessorIndex
        );
        public abstract void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream);
        public abstract void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = PrimitiveCreateContextBase.defaultMeshUpdateFlags);
        public abstract void Dispose();
    }

    class VertexBufferBones : VertexBufferBonesBase
    {
        NativeArray<VBones> m_Data;

        public VertexBufferBones(ICodeLogger logger) : base(logger) { }

        public override unsafe JobHandle? ScheduleVertexBonesJob(
            IGltfBuffers buffers,
            int weightsAccessorIndex,
            int jointsAccessorIndex
        )
        {
            Profiler.BeginSample("ScheduleVertexBonesJob");
            Profiler.BeginSample("AllocateNativeArray");

            buffers.GetAccessor(weightsAccessorIndex, out var weightsAcc, out var weightsData, out var weightsByteStride);
            if (weightsAcc.IsSparse)
            {
                m_Logger?.Error(LogCode.SparseAccessor, "bone weights");
            }
            m_Data = new NativeArray<VBones>(weightsAcc.count, VertexBufferConfigBase.defaultAllocator);
            var vDataPtr = (byte*)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(m_Data);
            Profiler.EndSample();

            JobHandle weightsHandle;
            JobHandle jointsHandle;

            {
                var h = GetWeightsJob(
                    weightsData,
                    weightsAcc.count,
                    weightsAcc.componentType,
                    weightsByteStride,
                    (float4*)vDataPtr,
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
                buffers.GetAccessor(jointsAccessorIndex, out var jointsAcc, out var jointsData, out var jointsByteStride);
                if (jointsAcc.IsSparse)
                {
                    m_Logger?.Error(LogCode.SparseAccessor, "bone joints");
                }
                var h = GetJointsJob(
                    jointsData,
                    jointsAcc.count,
                    jointsAcc.componentType,
                    jointsByteStride,
                    (uint4*)(vDataPtr + 16),
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

            var jobHandle = JobHandle.CombineDependencies(weightsHandle, jointsHandle);

            var skinWeights = (int)QualitySettings.skinWeights;

#if UNITY_EDITOR
            // If this is design-time import, fix and import all weights.
            if(!UnityEditor.EditorApplication.isPlaying || skinWeights < 4) {
                if (!UnityEditor.EditorApplication.isPlaying) {
                    skinWeights = 4;
                }
#else
            if (skinWeights < 4)
            {
#endif
                var job = new SortAndNormalizeBoneWeightsJob
                {
                    bones = m_Data,
                    skinWeights = math.max(1, skinWeights)
                };
                jobHandle = job.Schedule(m_Data.Length, GltfImport.DefaultBatchCount, jobHandle);
            }
#if GLTFAST_SAFE
            else {
                // Re-normalizing alone is sufficient
                var job = new RenormalizeBoneWeightsJob {
                    bones = m_Data,
                };
                jobHandle = job.Schedule(m_Data.Length, GltfImport.DefaultBatchCount, jobHandle);
            }
#endif

            Profiler.EndSample();
            return jobHandle;
        }

        public override void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream)
        {
            dst[offset] = new VertexAttributeDescriptor(VertexAttribute.BlendWeight, VertexAttributeFormat.Float32, 4, stream);
            dst[offset + 1] = new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt32, 4, stream);
        }

        public override void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = PrimitiveCreateContextBase.defaultMeshUpdateFlags)
        {
            Profiler.BeginSample("ApplyBones");
            msh.SetVertexBufferData(m_Data, 0, 0, m_Data.Length, stream, flags);
            Profiler.EndSample();
        }

        public override void Dispose()
        {
            if (m_Data.IsCreated)
            {
                m_Data.Dispose();
            }
        }

        unsafe JobHandle? GetWeightsJob(
            void* input,
            int count,
            GltfComponentType inputType,
            int inputByteStride,
            float4* output,
            int outputByteStride
            )
        {
            Profiler.BeginSample("GetWeightsJob");
            JobHandle? jobHandle;
            switch (inputType)
            {
                case GltfComponentType.Float:
                    var jobTangentI = new ConvertBoneWeightsFloatToFloatInterleavedJob();
                    jobTangentI.inputByteStride = inputByteStride > 0 ? inputByteStride : 16;
                    jobTangentI.input = (byte*)input;
                    jobTangentI.outputByteStride = outputByteStride;
                    jobTangentI.result = output;
#if UNITY_JOBS
                    jobHandle = jobTangentI.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = jobTangentI.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                    break;
                case GltfComponentType.UnsignedShort:
                    {
                        var job = new ConvertBoneWeightsUInt16ToFloatInterleavedJob
                        {
                            inputByteStride = inputByteStride > 0 ? inputByteStride : 8,
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                        jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                        break;
                    }
                case GltfComponentType.UnsignedByte:
                    {
                        var job = new ConvertBoneWeightsUInt8ToFloatInterleavedJob
                        {
                            inputByteStride = inputByteStride > 0 ? inputByteStride : 4,
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                        jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
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
            GltfComponentType inputType,
            int inputByteStride,
            uint4* output,
            int outputByteStride,
            ICodeLogger logger
        )
        {
            Profiler.BeginSample("GetJointsJob");
            JobHandle? jobHandle;
            switch (inputType)
            {
                case GltfComponentType.UnsignedByte:
                    var jointsUInt8Job = new ConvertBoneJointsUInt8ToUInt32Job();
                    jointsUInt8Job.inputByteStride = inputByteStride > 0 ? inputByteStride : 4;
                    jointsUInt8Job.input = (byte*)input;
                    jointsUInt8Job.outputByteStride = outputByteStride;
                    jointsUInt8Job.result = output;
                    jobHandle = jointsUInt8Job.Schedule(count, GltfImport.DefaultBatchCount);
                    break;
                case GltfComponentType.UnsignedShort:
                    var jointsUInt16Job = new ConvertBoneJointsUInt16ToUInt32Job();
                    jointsUInt16Job.inputByteStride = inputByteStride > 0 ? inputByteStride : 8;
                    jointsUInt16Job.input = (byte*)input;
                    jointsUInt16Job.outputByteStride = outputByteStride;
                    jointsUInt16Job.result = output;
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
