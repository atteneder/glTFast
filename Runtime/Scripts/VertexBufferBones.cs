// Copyright 2020-2021 Andreas Atteneder
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

using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast {

    using Vertex;
    using Schema;

    abstract class VertexBufferBonesBase {
        
        protected ICodeLogger logger;

        public VertexBufferBonesBase(ICodeLogger logger) {
            this.logger = logger;
        }

        public abstract bool ScheduleVertexBonesJob(
            IGltfBuffers buffers,
            int weightsAccessorIndex,
            int jointsAccessorIndex,
            NativeSlice<JobHandle> handles
            );
        public abstract void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream);
        public abstract void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = PrimitiveCreateContextBase.defaultMeshUpdateFlags);
        public abstract void Dispose();
    }

    class VertexBufferBones : VertexBufferBonesBase {
        NativeArray<VBones> vData;

        public VertexBufferBones(ICodeLogger logger) : base(logger) {}
        
        public override unsafe bool ScheduleVertexBonesJob(
            IGltfBuffers buffers,
            int weightsAccessorIndex,
            int jointsAccessorIndex,
            NativeSlice<JobHandle> handles
            )
        {
            Profiler.BeginSample("ScheduleVertexBonesJob");
            Profiler.BeginSample("AllocateNativeArray");
            
            buffers.GetAccessor(weightsAccessorIndex, out var weightsAcc, out var weightsData, out var weightsByteStride);
            if (weightsAcc.isSparse) {
                logger.Error(LogCode.SparseAccessor,"bone weights");
            }
            vData = new NativeArray<VBones>(weightsAcc.count, VertexBufferConfigBase.defaultAllocator);
            var vDataPtr = (byte*) NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(vData);
            Profiler.EndSample();
            
            {
                var h = GetWeightsJob(
                    weightsData,
                    weightsAcc.count,
                    weightsAcc.componentType,
                    weightsByteStride,
                    (float4*)vDataPtr,
                    32,
                    weightsAcc.normalized
                );
                if (h.HasValue) {
                    handles[0] = h.Value;
                } else {
                    Profiler.EndSample();
                    return false;
                }
            }

            {
                buffers.GetAccessor(jointsAccessorIndex, out var jointsAcc, out var jointsData, out var jointsByteStride);
                if (jointsAcc.isSparse) {
                    logger.Error(LogCode.SparseAccessor,"bone joints");
                }
                var h = GetJointsJob(
                    jointsData,
                    jointsAcc.count,
                    jointsAcc.componentType,
                    jointsByteStride,
                    (uint4*)(vDataPtr+16),
                    32,
                    logger
                );
                if (h.HasValue) {
                    handles[1] = h.Value;
                } else {
                    Profiler.EndSample();
                    return false;
                }
            }
            Profiler.EndSample();
            return true;
        }

        public override void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream) {
            dst[offset] = new VertexAttributeDescriptor(VertexAttribute.BlendWeight, VertexAttributeFormat.Float32, 4, stream);
            dst[offset+1] = new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt32, 4, stream);
        }

        public override void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = PrimitiveCreateContextBase.defaultMeshUpdateFlags) {
            Profiler.BeginSample("ApplyBones");
            msh.SetVertexBufferData(vData,0,0,vData.Length,stream,flags);
            Profiler.EndSample();
        }

        public override void Dispose() {
            if (vData.IsCreated) {
                vData.Dispose();
            }
        }

        protected unsafe JobHandle? GetWeightsJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            float4* output,
            int outputByteStride,
            bool normalized = false
            )
        {
            Profiler.BeginSample("GetWeightsJob");
            JobHandle? jobHandle;
            switch(inputType) {
                case GLTFComponentType.Float:
                    var jobTangentI = new Jobs.ConvertBoneWeightsFloatToFloatInterleavedJob();
                    jobTangentI.inputByteStride = inputByteStride>0 ? inputByteStride : 16;
                    jobTangentI.input = (byte*)input;
                    jobTangentI.outputByteStride = outputByteStride;
                    jobTangentI.result = output;
#if UNITY_JOBS
                    jobHandle = jobTangentI.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = jobTangentI.Schedule(count,GltfImport.DefaultBatchCount);
#endif
                    break;
                case GLTFComponentType.UnsignedShort: {
                    var job = new Jobs.ConvertBoneWeightsUInt16ToFloatInterleavedJob {
                        inputByteStride = inputByteStride>0 ? inputByteStride : 8,
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
#endif
                    break;
                }
                case GLTFComponentType.UnsignedByte: {
                    var job = new Jobs.ConvertBoneWeightsUInt8ToFloatInterleavedJob {
                        inputByteStride = inputByteStride>0 ? inputByteStride : 4,
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
#endif
                    break;
                }
                default:
                    logger?.Error(LogCode.TypeUnsupported,"Weights",inputType.ToString());
                    jobHandle = null;
                    break;
            }

            Profiler.EndSample();
            return jobHandle;
        }

        static unsafe JobHandle? GetJointsJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            uint4* output,
            int outputByteStride,
            ICodeLogger logger
        )
        {
            Profiler.BeginSample("GetJointsJob");
            JobHandle? jobHandle;
            switch(inputType) {
                case GLTFComponentType.UnsignedByte:
                    var jointsUInt8Job = new Jobs.ConvertBoneJointsUInt8ToUInt32Job();
                    jointsUInt8Job.inputByteStride = inputByteStride>0 ? inputByteStride : 4;
                    jointsUInt8Job.input = (byte*)input;
                    jointsUInt8Job.outputByteStride = outputByteStride;
                    jointsUInt8Job.result = output;
                    jobHandle = jointsUInt8Job.Schedule(count,GltfImport.DefaultBatchCount);
                    break;
                case GLTFComponentType.UnsignedShort:
                    var jointsUInt16Job = new Jobs.ConvertBoneJointsUInt16ToUInt32Job();
                    jointsUInt16Job.inputByteStride = inputByteStride>0 ? inputByteStride : 8;
                    jointsUInt16Job.input = (byte*)input;
                    jointsUInt16Job.outputByteStride = outputByteStride;
                    jointsUInt16Job.result = output;
                    jobHandle = jointsUInt16Job.Schedule(count,GltfImport.DefaultBatchCount);
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
