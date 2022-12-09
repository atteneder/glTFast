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
    using Jobs;
    using Logging;
    using Schema;

    [System.Flags]
    enum MainBufferType
    {
        None = 0x0,
        Position = 0x1,
        Normal = 0x2,
        Tangent = 0x4,

        PosNorm = 0x3,
        PosNormTan = 0x7,
    }

    abstract class VertexBufferConfigBase
    {

        public const Allocator defaultAllocator = Allocator.Persistent;

        public bool calculateNormals = false;
        public bool calculateTangents = false;

        protected VertexAttributeDescriptor[] m_Descriptors;
        protected ICodeLogger m_Logger;

        public Bounds? Bounds { get; protected set; }

        protected VertexBufferConfigBase(ICodeLogger logger)
        {
            m_Logger = logger;
        }

        public abstract JobHandle? ScheduleVertexJobs(
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
        public abstract int VertexCount { get; }
        public abstract void Dispose();

        /// <summary>
        /// Schedules a job that converts input data into float3 arrays.
        /// </summary>
        /// <param name="input">Points at the input data in memory</param>
        /// <param name="count">Attribute quantity</param>
        /// <param name="inputType">Input data type</param>
        /// <param name="inputByteStride">Input byte stride</param>
        /// <param name="output">Points at the destination buffer in memory</param>
        /// <param name="outputByteStride">Output byte stride</param>
        /// <param name="normalized">If true, integer values have to be normalized</param>
        /// <param name="ensureUnitLength">If true, normalized values will be scaled to have unit length again (only if <see cref="normalized"/>is true)</param>
        /// <returns></returns>
        public static unsafe JobHandle? GetVector3Job(
            void* input,
            int count,
            GltfComponentType inputType,
            int inputByteStride,
            float3* output,
            int outputByteStride,
            bool normalized = false,
            bool ensureUnitLength = true
        )
        {
            JobHandle? jobHandle;

            Profiler.BeginSample("GetVector3Job");
            if (inputType == GltfComponentType.Float)
            {
                var job = new ConvertVector3FloatToFloatInterleavedJob
                {
                    inputByteStride = (inputByteStride > 0) ? inputByteStride : 12,
                    input = (byte*)input,
                    outputByteStride = outputByteStride,
                    result = output
                };
#if UNITY_JOBS
                jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
            }
            else
            if (inputType == GltfComponentType.UnsignedShort)
            {
                if (normalized)
                {
                    var job = new ConvertPositionsUInt16ToFloatInterleavedNormalizedJob
                    {
                        inputByteStride = (inputByteStride > 0) ? inputByteStride : 6,
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                }
                else
                {
                    var job = new ConvertPositionsUInt16ToFloatInterleavedJob
                    {
                        inputByteStride = (inputByteStride > 0) ? inputByteStride : 6,
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                }
            }
            else
            if (inputType == GltfComponentType.Short)
            {
                if (normalized)
                {
                    if (ensureUnitLength)
                    {
                        // TODO: test. did not have test files
                        var job = new ConvertNormalsInt16ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = (inputByteStride > 0) ? inputByteStride : 6,
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
#if UNITY_JOBS
                        jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                        jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                    }
                    else
                    {
                        var job = new ConvertVector3Int16ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = (inputByteStride > 0) ? inputByteStride : 6,
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
#if UNITY_JOBS
                        jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                        jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                    }
                }
                else
                {
                    var job = new ConvertPositionsInt16ToFloatInterleavedJob
                    {
                        inputByteStride = (inputByteStride > 0) ? inputByteStride : 6,
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                }
            }
            else
            if (inputType == GltfComponentType.Byte)
            {
                if (normalized)
                {
                    if (ensureUnitLength)
                    {
                        var job = new ConvertNormalsInt8ToFloatInterleavedNormalizedJob
                        {
                            input = (sbyte*)input,
                            inputByteStride = (inputByteStride > 0) ? inputByteStride : 3,
                            outputByteStride = outputByteStride,
                            result = output
                        };
#if UNITY_JOBS
                        jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                        jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                    }
                    else
                    {
                        var job = new ConvertVector3Int8ToFloatInterleavedNormalizedJob()
                        {
                            input = (sbyte*)input,
                            inputByteStride = (inputByteStride > 0) ? inputByteStride : 3,
                            outputByteStride = outputByteStride,
                            result = output
                        };
#if UNITY_JOBS
                        jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                        jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                    }
                }
                else
                {
                    // TODO: test positions. did not have test files
                    var job = new ConvertPositionsInt8ToFloatInterleavedJob
                    {
                        inputByteStride = inputByteStride > 0 ? inputByteStride : 3,
                        input = (sbyte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                }
            }
            else
            if (inputType == GltfComponentType.UnsignedByte)
            {
                // TODO: test. did not have test files
                if (normalized)
                {
                    var job = new ConvertPositionsUInt8ToFloatInterleavedNormalizedJob
                    {
                        input = (byte*)input,
                        inputByteStride = (inputByteStride > 0) ? inputByteStride : 3,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                }
                else
                {
                    var job = new ConvertPositionsUInt8ToFloatInterleavedJob
                    {
                        input = (byte*)input,
                        inputByteStride = (inputByteStride > 0) ? inputByteStride : 3,
                        outputByteStride = outputByteStride,
                        result = output
                    };
#if UNITY_JOBS
                    jobHandle = job.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                    jobHandle = job.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                }
            }
            else
            {
                Debug.LogError("Unknown componentType");
                jobHandle = null;
            }
            Profiler.EndSample();
            return jobHandle;
        }

        protected unsafe JobHandle? GetTangentsJob(
            void* input,
            int count,
            GltfComponentType inputType,
            int inputByteStride,
            float4* output,
            int outputByteStride,
            bool normalized = false
            )
        {
            Profiler.BeginSample("GetTangentsJob");
            JobHandle? jobHandle;
            switch (inputType)
            {
                case GltfComponentType.Float:
                    {
                        var jobTangent = new ConvertTangentsFloatToFloatInterleavedJob
                        {
                            inputByteStride = inputByteStride > 0 ? inputByteStride : 16,
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
#if UNITY_JOBS
                    jobHandle = jobTangent.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                        jobHandle = jobTangent.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                        break;
                    }
                case GltfComponentType.Short:
                    {
                        Assert.IsTrue(normalized);
                        var jobTangent = new ConvertTangentsInt16ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = inputByteStride > 0 ? inputByteStride : 8,
                            input = (short*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
#if UNITY_JOBS
                    jobHandle = jobTangent.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                        jobHandle = jobTangent.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                        break;
                    }
                case GltfComponentType.Byte:
                    {
                        Assert.IsTrue(normalized);
                        var jobTangent = new ConvertTangentsInt8ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = inputByteStride > 0 ? inputByteStride : 4,
                            input = (sbyte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
#if UNITY_JOBS
                    jobHandle = jobTangent.ScheduleBatch(count,GltfImport.DefaultBatchCount);
#else
                        jobHandle = jobTangent.Schedule(count, GltfImport.DefaultBatchCount);
#endif
                        break;
                    }
                default:
                    m_Logger?.Error(LogCode.TypeUnsupported, "Tangent", inputType.ToString());
                    jobHandle = null;
                    break;
            }

            Profiler.EndSample();
            return jobHandle;
        }

        public static unsafe JobHandle? GetVector3SparseJob(
            void* indexBuffer,
            void* valueBuffer,
            int sparseCount,
            GltfComponentType indexType,
            GltfComponentType valueType,
            float3* output,
            int outputByteStride,
            ref JobHandle? dependsOn,
            bool normalized = false
        )
        {
            Profiler.BeginSample("GetVector3SparseJob");
            var job = new ConvertVector3SparseJob
            {
                indexBuffer = (ushort*)indexBuffer,
                indexConverter = CachedFunction.GetIndexConverter(indexType),
                inputByteStride = 3 * Accessor.GetComponentTypeSize(valueType),
                input = valueBuffer,
                valueConverter = CachedFunction.GetPositionConverter(valueType, normalized),
                outputByteStride = outputByteStride,
                result = output,
            };

            JobHandle? jobHandle = job.Schedule(
                sparseCount,
                GltfImport.DefaultBatchCount,
                dependsOn: dependsOn ?? default(JobHandle)
            );
            Profiler.EndSample();
            return jobHandle;
        }
    }
}
