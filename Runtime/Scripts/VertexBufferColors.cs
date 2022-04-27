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

    abstract class VertexBufferColorsBase {
        public abstract bool ScheduleVertexColorJob(IGltfBuffers buffers, int colorAccessorIndex, NativeSlice<JobHandle> handles);
        public abstract void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream);
        public abstract void ApplyOnMesh(UnityEngine.Mesh msh, int stream, MeshUpdateFlags flags = PrimitiveCreateContextBase.defaultMeshUpdateFlags);
        public abstract void Dispose();

        protected ICodeLogger logger;
    }

    class VertexBufferColors : VertexBufferColorsBase {
        NativeArray<float4> vData;

        public override unsafe bool ScheduleVertexColorJob(IGltfBuffers buffers, int colorAccessorIndex, NativeSlice<JobHandle> handles) {
            Profiler.BeginSample("ScheduleVertexColorJob");
            Profiler.BeginSample("AllocateNativeArray");
            buffers.GetAccessor(colorAccessorIndex, out var colorAcc, out var data, out var byteStride);
            if (colorAcc.isSparse) {
                logger.Error(LogCode.SparseAccessor,"color");
            }
            vData = new NativeArray<float4>(colorAcc.count, VertexBufferConfigBase.defaultAllocator);
            var vDataPtr = (byte*) NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(vData);
            Profiler.EndSample();
            
            var h = GetColors32Job(
                data,
                colorAcc.componentType,
                colorAcc.typeEnum,
                byteStride,
                vData
            );
            if (h.HasValue) {
                handles[0] = h.Value;
            } else {
                Profiler.EndSample();
                return false;
            }
            Profiler.EndSample();
            return true;
        }

        public override void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream) {
            dst[offset] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, stream);
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
        
        unsafe JobHandle? GetColors32Job(
            void* input,
            GLTFComponentType inputType,
            GLTFAccessorAttributeType attributeType,
            int inputByteStride,
            NativeArray<float4> output
            )
        {
            Profiler.BeginSample("PrepareColors32");
            JobHandle? jobHandle = null;

            if (attributeType == GLTFAccessorAttributeType.VEC3)
            {
                switch (inputType)
                {
                    case GLTFComponentType.UnsignedByte:
                        {
                            var job = new Jobs.ConvertColorsRGBUInt8ToRGBAFloatJob {
                                input = (byte*) input,
                                inputByteStride = inputByteStride>0 ? inputByteStride : 3,
                                result = output
                            };
                            jobHandle = job.Schedule(output.Length,GltfImport.DefaultBatchCount);
                        }
                        break;
                    case GLTFComponentType.Float:
                        {
                            var job = new Jobs.ConvertColorsRGBFloatToRGBAFloatJob {
                                input = (byte*) input,
                                inputByteStride = inputByteStride>0 ? inputByteStride : 12,
                                result = (float4*)output.GetUnsafePtr()
                            };
                            jobHandle = job.Schedule(output.Length,GltfImport.DefaultBatchCount);
                        }
                        break;
                    case GLTFComponentType.UnsignedShort:
                        {
                            var job = new Jobs.ConvertColorsRGBUInt16ToRGBAFloatJob {
                                input = (System.UInt16*)input,
                                inputByteStride = inputByteStride>0 ? inputByteStride : 6,
                                result = output
                            };
                            jobHandle = job.Schedule(output.Length,GltfImport.DefaultBatchCount);
                        }
                        break;
                    default:
                        logger?.Error(LogCode.ColorFormatUnsupported,attributeType.ToString());
                        break;
                }
            }
            else if (attributeType == GLTFAccessorAttributeType.VEC4)
            {
                switch (inputType)
                {
                    case GLTFComponentType.UnsignedByte:
                        {
                            var job = new Jobs.ConvertColorsRGBAUInt8ToRGBAFloatJob {
                                input = (byte*) input,
                                inputByteStride = inputByteStride > 0 ? inputByteStride : 4,
                                result = output
                            };
                            jobHandle = job.Schedule(output.Length,GltfImport.DefaultBatchCount);
                        }
                        break;
                    case GLTFComponentType.Float:
                        {
                            if (inputByteStride == 16 || inputByteStride <= 0)
                            {
                                var job = new Jobs.MemCopyJob {
                                    bufferSize = output.Length*16,
                                    input = input,
                                    result = output.GetUnsafeReadOnlyPtr()
                                };
                                jobHandle = job.Schedule();
                            } else {
                                var job = new Jobs.ConvertColorsRGBAFloatToRGBAFloatJob {
                                    input = (byte*) input,
                                    inputByteStride = inputByteStride,
                                    result = (float4*)output.GetUnsafePtr()
                                };
#if UNITY_JOBS
                                jobHandle = job.ScheduleBatch(output.Length,GltfImport.DefaultBatchCount);
#else
                                jobHandle = job.Schedule(output.Length,GltfImport.DefaultBatchCount);
#endif
                            }
                        }
                        break;
                    case GLTFComponentType.UnsignedShort:
                        {
                            var job = new Jobs.ConvertColorsRGBAUInt16ToRGBAFloatJob {
                                input = (System.UInt16*) input,
                                inputByteStride = inputByteStride>0 ? inputByteStride : 8,
                                result = (float4*)output.GetUnsafePtr()
                            };
#if UNITY_JOBS
                            jobHandle = job.ScheduleBatch(output.Length,GltfImport.DefaultBatchCount);
#else
                            jobHandle = job.Schedule(output.Length,GltfImport.DefaultBatchCount);
#endif
                        }
                        break;
                    default:
                        logger?.Error(LogCode.ColorFormatUnsupported, attributeType.ToString());
                        break;
                }
            } else {
                logger?.Error(LogCode.TypeUnsupported, "color accessor", inputType.ToString());
            }
            Profiler.EndSample();
            return jobHandle;
        }
    }
}
