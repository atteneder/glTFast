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

#if DEBUG
using System.Collections.Generic;
#endif

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace GLTFast
{
#if BURST
    using Unity.Mathematics;
#endif
    using Jobs;
    using Schema;

    [System.Flags]
    enum MainBufferType {
        None = 0x0,
        Position = 0x1,
        Normal = 0x2,
        Tangent = 0x4,
        
        PosNorm = 0x3,
        PosNormTan = 0x7,
    }

    abstract class VertexBufferConfigBase {

        public const Allocator defaultAllocator = Allocator.Persistent;

        public bool calculateNormals = false;
        public bool calculateTangents = false;

        protected VertexAttributeDescriptor[] vad;
        protected ICodeLogger logger;

        public Bounds? bounds { get; protected set; }

        public VertexBufferConfigBase(ICodeLogger logger) {
            this.logger = logger;
        }
        
        public abstract unsafe JobHandle? ScheduleVertexJobs(
            IGltfBuffers buffers,
            int positionAccessorIndex,
            int normalAccessorIndex,
            int tangentAccessorIndex,
            int[] uvAccessorIndices,
            int colorAccessorIndex,
            int weightsAccessorIndex,
            int jointsAccessorIndex
            );
        public abstract void ApplyOnMesh(UnityEngine.Mesh msh, MeshUpdateFlags flags = PrimitiveCreateContextBase.defaultMeshUpdateFlags);
        public abstract int vertexCount { get; }
        public abstract void Dispose();

        public static unsafe JobHandle? GetVector3sJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            Vector3* output,
            int outputByteStride,
            bool normalized = false
        ) {
            JobHandle? jobHandle;

            Profiler.BeginSample("GetVector3sJob");
            if(inputType == GLTFComponentType.Float) {
                var job = new Jobs.ConvertPositionsFloatToFloatInterleavedJob();
                job.inputByteStride = (inputByteStride>0) ? inputByteStride : 12;
                job.input = (byte*)input;
                job.outputByteStride = outputByteStride;
                job.result = output;
                jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
            } else
            if(inputType == GLTFComponentType.UnsignedShort) {
                if (normalized) {
                    var job = new Jobs.ConvertPositionsUInt16ToFloatInterleavedNormalizedJob();
                    job.inputByteStride = (inputByteStride>0) ? inputByteStride : 6;
                    job.input = (byte*)input;
                    job.outputByteStride = outputByteStride;
                    job.result = output;
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
                } else {
                    var job = new Jobs.ConvertPositionsUInt16ToFloatInterleavedJob();
                    job.inputByteStride = (inputByteStride>0) ? inputByteStride : 6;
                    job.input = (byte*)input;
                    job.outputByteStride = outputByteStride;
                    job.result = output;
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
                }
            } else
            if(inputType == GLTFComponentType.Short) {
                // TODO: test. did not have test files
                if (normalized) {
                    var job = new Jobs.ConvertPositionsInt16ToFloatInterleavedNormalizedJob();
                    job.inputByteStride = (inputByteStride>0) ? inputByteStride : 6;
                    job.input = (byte*)input;
                    job.outputByteStride = outputByteStride;
                    job.result = output;
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
                } else {
                    var job = new Jobs.ConvertPositionsInt16ToFloatInterleavedJob();
                    job.inputByteStride = (inputByteStride>0) ? inputByteStride : 6;
                    job.input = (byte*)input;
                    job.outputByteStride = outputByteStride;
                    job.result = output;
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
                }
            } else
            if(inputType == GLTFComponentType.Byte) {
                // TODO: test positions. did not have test files
                if (normalized) {
                    var job = new Jobs.ConvertPositionsSByteToFloatInterleavedNormalizedJob();
                    job.Setup((inputByteStride>0) ? inputByteStride : 3, (sbyte*)input,outputByteStride,output);
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
                } else {
                    var job = new Jobs.ConvertPositionsSByteToFloatInterleavedJob();
                    job.Setup((inputByteStride>0) ? inputByteStride : 3,(sbyte*)input,outputByteStride,output);
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
                }
            } else
            if(inputType == GLTFComponentType.UnsignedByte) {
                // TODO: test. did not have test files
                if (normalized) {
                    var job = new Jobs.ConvertPositionsByteToFloatInterleavedNormalizedJob {
                        input = (byte*)input,
                        inputByteStride = (inputByteStride > 0) ? inputByteStride : 3,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
                } else {
                    var job = new Jobs.ConvertPositionsByteToFloatInterleavedJob {
                        input = (byte*)input,
                        inputByteStride = (inputByteStride > 0) ? inputByteStride : 3,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = job.Schedule(count,GltfImport.DefaultBatchCount);
                }
            } else {
                Debug.LogError("Unknown componentType");
                jobHandle = null;
            }
            Profiler.EndSample();
            return jobHandle;
        }
        
        protected unsafe JobHandle? GetTangentsJob(
            void* input,
            int count,
            GLTFComponentType inputType,
            int inputByteStride,
            Vector4* output,
            int outputByteStride,
            bool normalized = false
            )
        {
            Profiler.BeginSample("GetTangentsJob");
            JobHandle? jobHandle;
            switch(inputType) {
                case GLTFComponentType.Float:
                    var jobTangentI = new Jobs.ConvertTangentsFloatToFloatInterleavedJob();
                    jobTangentI.inputByteStride = inputByteStride>0 ? inputByteStride : 16;
                    jobTangentI.input = (byte*)input;
                    jobTangentI.outputByteStride = outputByteStride;
                    jobTangentI.result = output;
                    jobHandle = jobTangentI.Schedule(count,GltfImport.DefaultBatchCount);
                    break;
                case GLTFComponentType.Short:
                    var jobTangent = new Jobs.ConvertTangentsInt16ToFloatInterleavedNormalizedJob();
                    jobTangent.inputByteStride = inputByteStride>0 ? inputByteStride : 8;;
                    Assert.IsTrue(normalized);
                    jobTangent.input = (System.Int16*)input;
                    jobTangent.outputByteStride = outputByteStride;
                    jobTangent.result = output;
                    jobHandle = jobTangent.Schedule(count,GltfImport.DefaultBatchCount);
                    break;
                case GLTFComponentType.Byte:
                    var jobTangentByte = new Jobs.ConvertTangentsInt8ToFloatInterleavedNormalizedJob();
                    jobTangentByte.inputByteStride = inputByteStride>0 ? inputByteStride : 4;
                    Assert.IsTrue(normalized);
                    jobTangentByte.input = (sbyte*)input;
                    jobTangentByte.outputByteStride = outputByteStride;
                    jobTangentByte.result = output;
                    jobHandle = jobTangentByte.Schedule(count,GltfImport.DefaultBatchCount);
                    break;
                default:
                    logger?.Error(LogCode.TypeUnsupported, "Tangent", inputType.ToString());
                    jobHandle = null;
                    break;
            }

            Profiler.EndSample();
            return jobHandle;
        }

        public static unsafe JobHandle? GetVector3sSparseJob(
            void* indexBuffer,
            void* valueBuffer,
            int sparseCount,
            GLTFComponentType indexType,
            GLTFComponentType valueType,
            Vector3* output,
            int outputByteStride,
            ref JobHandle? dependsOn,
            bool normalized = false
        ) {
            JobHandle? jobHandle;

            Profiler.BeginSample("GetVector3sSparseJob");
            var job = new ConvertPositionsSparseJob {
                indexBuffer = (ushort*)indexBuffer,
                indexConverter = CachedFunction.GetIndexConverter(indexType),
                inputByteStride = 3*Accessor.GetComponentTypeSize(valueType),
                input = valueBuffer,
                valueConverter = CachedFunction.GetPositionConverter(valueType,normalized),
                outputByteStride = outputByteStride,
                result = output,
            };
            
            jobHandle = job.Schedule(
                sparseCount,
                GltfImport.DefaultBatchCount,
                dependsOn: dependsOn.HasValue ? dependsOn.Value : default(JobHandle)
                );
            Profiler.EndSample();
            return jobHandle;
        }
    }
}
