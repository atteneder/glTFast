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

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast {

    using Logging;
    using Schema;

    abstract class VertexBufferTexCoordsBase {
        
        protected ICodeLogger logger;

        public VertexBufferTexCoordsBase(ICodeLogger logger) {
            this.logger = logger;
        }
        
        public int uvSetCount { get; protected set; }
        public abstract bool ScheduleVertexUVJobs(IGltfBuffers buffers, int[] uvAccessorIndices, int vertexCount, NativeSlice<JobHandle> handles);
        public abstract void AddDescriptors(VertexAttributeDescriptor[] dst, ref int offset, int stream);
        public abstract void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = PrimitiveCreateContextBase.defaultMeshUpdateFlags);
        public abstract void Dispose();
    }

    class VertexBufferTexCoords<T> : VertexBufferTexCoordsBase where T : struct {
        NativeArray<T> vData;

        public VertexBufferTexCoords(ICodeLogger logger) : base(logger) {}
        
        public override unsafe bool ScheduleVertexUVJobs(IGltfBuffers buffers, int[] uvAccessorIndices, int vertexCount, NativeSlice<JobHandle> handles) {
            Profiler.BeginSample("ScheduleVertexUVJobs");
            Profiler.BeginSample("AllocateNativeArray");
            vData = new NativeArray<T>( vertexCount, VertexBufferConfigBase.defaultAllocator);
            var vDataPtr = (byte*) NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(vData);
            Profiler.EndSample();
            uvSetCount = uvAccessorIndices.Length;
            int outputByteStride = uvAccessorIndices.Length * 8;

            for (int i=0; i<uvAccessorIndices.Length; i++) {
                var accIndex = uvAccessorIndices[i];
                buffers.GetAccessor(accIndex, out var uvAcc, out var data, out var byteStride);
                if (uvAcc.isSparse) {
                    logger.Error(LogCode.SparseAccessor,"UVs");
                }
                var h = GetUvsJob(
                    data,
                    uvAcc.count,
                    uvAcc.componentType,
                    byteStride,
                    (float2*) (vDataPtr+(i*8)),
                    outputByteStride,
                    uvAcc.normalized
                );
                if (h.HasValue) {
                    handles[i] = h.Value;
                } else {
                    Profiler.EndSample();
                    return false;
                }
            }
            Profiler.EndSample();
            return true;
        }

        public override void AddDescriptors(VertexAttributeDescriptor[] dst, ref int offset, int stream) {
            for (int i = 0; i < uvSetCount; i++) {
                var vertexAttribute = (VertexAttribute)((int)VertexAttribute.TexCoord0+i);
                dst[offset] = new VertexAttributeDescriptor(vertexAttribute, VertexAttributeFormat.Float32, 2, stream);
                offset++;
            }
        }

        public override void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = PrimitiveCreateContextBase.defaultMeshUpdateFlags) {
            Profiler.BeginSample("ApplyUVs");
            msh.SetVertexBufferData(vData,0,0,vData.Length,stream,flags);
            Profiler.EndSample();
        }

        public override void Dispose() {
            if (vData.IsCreated) {
                vData.Dispose();
            }
        }

        unsafe JobHandle? GetUvsJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            float2* output,
            int outputByteStride,
            bool normalized = false
            )
        {
            Profiler.BeginSample("PrepareUVs");
            JobHandle? jobHandle = null;
            
            switch( inputType ) { 
            case GLTFComponentType.Float:
                {
                    var jobUv = new Jobs.ConvertUVsFloatToFloatInterleavedJob {
                        inputByteStride = (inputByteStride>0) ? inputByteStride : 8,
                        input = (byte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = jobUv.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = jobUv.Schedule(count,GltfImport.DefaultBatchCount);
#endif
                }
                break;
            case GLTFComponentType.UnsignedByte:
                if (normalized) {
                    var jobUv = new Jobs.ConvertUVsUInt8ToFloatInterleavedNormalizedJob {
                        inputByteStride = (inputByteStride>0) ? inputByteStride : 2,
                        input = (byte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobUv.Schedule(count,GltfImport.DefaultBatchCount);
                } else {
                    var jobUv = new Jobs.ConvertUVsUInt8ToFloatInterleavedJob {
                        inputByteStride = (inputByteStride>0) ? inputByteStride : 2,
                        input = (byte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = jobUv.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = jobUv.Schedule(count,GltfImport.DefaultBatchCount);
#endif
                }
                break;
            case GLTFComponentType.UnsignedShort:
                if (normalized) {
                    var jobUv = new Jobs.ConvertUVsUInt16ToFloatInterleavedNormalizedJob {
                        inputByteStride = (inputByteStride>0) ? inputByteStride : 4,
                        input = (byte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobUv.Schedule(count,GltfImport.DefaultBatchCount);
                } else {
                    var jobUv = new Jobs.ConvertUVsUInt16ToFloatInterleavedJob {
                        inputByteStride = (inputByteStride>0) ? inputByteStride : 4,
                        input = (byte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = jobUv.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = jobUv.Schedule(count,GltfImport.DefaultBatchCount);
#endif
                }
                break;
            case GLTFComponentType.Short:
                if (normalized) {
                    var job = new Jobs.ConvertUVsInt16ToFloatInterleavedNormalizedJob {
                        inputByteStride = inputByteStride > 0 ? inputByteStride : 4,
                        input = (System.Int16*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
#endif
                } else {
                    var job = new Jobs.ConvertUVsInt16ToFloatInterleavedJob {
                        inputByteStride = inputByteStride > 0 ? inputByteStride : 4,
                        input = (System.Int16*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
#endif
                }
                break;
            case GLTFComponentType.Byte:
                var byteStride = inputByteStride>0 ? inputByteStride : 2;
                if (normalized) {
                    var jobInt8 = new Jobs.ConvertUVsInt8ToFloatInterleavedNormalizedJob {
                        inputByteStride = inputByteStride > 0 ? inputByteStride : 2,
                        input = (sbyte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = jobInt8.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = jobInt8.Schedule(count,GltfImport.DefaultBatchCount);
#endif
                } else {
                    var jobInt8 = new Jobs.ConvertUVsInt8ToFloatInterleavedJob {
                        inputByteStride = inputByteStride > 0 ? inputByteStride : 2,
                        input = (sbyte*) input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = jobInt8.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = jobInt8.Schedule(count,GltfImport.DefaultBatchCount);
#endif
                }
                break;
            default:
                logger?.Error(LogCode.TypeUnsupported, "UV", inputType.ToString());
                break;
            }
            Profiler.EndSample();
            return jobHandle;
        }
    }
}
