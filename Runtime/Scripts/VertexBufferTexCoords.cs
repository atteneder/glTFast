// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Unity.Cloud.Gltfast
{

    using Logging;
    using Objects;

    abstract class VertexBufferTexCoordsBase : IDisposable
    {

        protected ICodeLogger m_Logger;

        protected VertexBufferTexCoordsBase(ICodeLogger logger)
        {
            m_Logger = logger;
        }

        public int UVSetCount { get; protected set; }
        public abstract bool ScheduleVertexUVJobs(
            int offset,
            int[] uvAccessorIndices,
            NativeArray<JobHandle> handles,
            BufferStore buffers
            );
        public abstract void AddDescriptors(VertexAttributeDescriptor[] dst, ref int offset, int stream);
        public abstract void ApplyOnMesh(
            UnityEngine.Mesh msh,
            int stream,
            MeshUpdateFlags flags = MeshGeneratorBase.defaultMeshUpdateFlags
            );

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }

    class VertexBufferTexCoords<T> : VertexBufferTexCoordsBase where T : unmanaged
    {
        NativeArray<T> m_Data;

        public VertexBufferTexCoords(int uvSetCount, int vertexCount, ICodeLogger logger)
            : base(logger)
        {
            UVSetCount = uvSetCount;
            m_Data = new NativeArray<T>(
                vertexCount,
                VertexBufferGeneratorBase.defaultAllocator
            );
        }

        public override unsafe bool ScheduleVertexUVJobs(
            int offset,
            int[] uvAccessorIndices,
            NativeArray<JobHandle> handles,
            BufferStore buffers
            )
        {
            Profiler.BeginSample("ScheduleVertexUVJobs");
            Profiler.BeginSample("AllocateNativeArray");

            var vDataPtr = (byte*)m_Data.GetUnsafeReadOnlyPtr();
            Profiler.EndSample();
            var outputByteStride = UVSetCount * sizeof(float2);

            for (var uvSet = 0; uvSet < UVSetCount; uvSet++)
            {
                var accIndex = uvAccessorIndices[uvSet];
                buffers.GetAccessorAndData(accIndex, out var uvAcc, out var data, out var byteStride);
                if (uvAcc.IsSparse)
                {
                    m_Logger?.Error(LogCode.SparseAccessor, "UVs");
                    Profiler.EndSample();
                    return false;
                }
                var h = GetUvsJob(
                    data,
                    uvAcc.Count,
                    uvAcc.ComponentType,
                    byteStride,
                    (float2*)(vDataPtr + outputByteStride * offset + uvSet * sizeof(float2)),
                    outputByteStride,
                    uvAcc.Normalized
                );
                if (h.HasValue)
                {
                    handles[uvSet] = h.Value;
                }
                else
                {
                    Profiler.EndSample();
                    return false;
                }
            }
            Profiler.EndSample();
            return true;
        }

        public override void AddDescriptors(VertexAttributeDescriptor[] dst, ref int offset, int stream)
        {
            for (int i = 0; i < UVSetCount; i++)
            {
                var vertexAttribute = (VertexAttribute)((int)VertexAttribute.TexCoord0 + i);
                dst[offset] = new VertexAttributeDescriptor(vertexAttribute, VertexAttributeFormat.Float32, 2, stream);
                offset++;
            }
        }

        public override void ApplyOnMesh(
            UnityEngine.Mesh msh,
            int stream,
            MeshUpdateFlags flags = MeshGeneratorBase.defaultMeshUpdateFlags
            )
        {
            Profiler.BeginSample("ApplyUVs");
            msh.SetVertexBufferData(m_Data, 0, 0, m_Data.Length, stream, flags);
            Profiler.EndSample();
        }

        protected override void Dispose(bool disposing)
        {
            if (m_Data.IsCreated)
            {
                m_Data.Dispose();
            }
        }

        unsafe JobHandle? GetUvsJob(
            void* input,
            int count,
            AccessorDataType inputType,
            int? inputByteStride,
            float2* output,
            int outputByteStride,
            bool normalized = false
            )
        {
            Profiler.BeginSample("PrepareUVs");
            JobHandle? jobHandle = null;

            switch (inputType)
            {
                case AccessorDataType.Float:
                {
                    var jobUv = new Jobs.ConvertUVsFloatToFloatInterleavedJob
                    {
                        inputByteStride = inputByteStride ?? sizeof(float2),
                        input = (byte*)input,
                        outputByteStride = outputByteStride,
                        result = output
                    };
                    jobHandle = jobUv.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                }
                break;
                case AccessorDataType.UnsignedByte:
                    if (normalized)
                    {
                        var jobUv = new Jobs.ConvertUVsUInt8ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = inputByteStride ?? 2 * sizeof(byte),
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobUv.Schedule(count, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var jobUv = new Jobs.ConvertUVsUInt8ToFloatInterleavedJob
                        {
                            inputByteStride = inputByteStride ?? 2 * sizeof(byte),
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobUv.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    break;
                case AccessorDataType.UnsignedShort:
                    if (normalized)
                    {
                        var jobUv = new Jobs.ConvertUVsUInt16ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = inputByteStride ?? 2 * sizeof(ushort),
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobUv.Schedule(count, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var jobUv = new Jobs.ConvertUVsUInt16ToFloatInterleavedJob
                        {
                            inputByteStride = inputByteStride ?? 2 * sizeof(ushort),
                            input = (byte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobUv.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    break;
                case AccessorDataType.Short:
                    if (normalized)
                    {
                        var job = new Jobs.ConvertUVsInt16ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = inputByteStride ?? 2 * sizeof(short),
                            input = (short*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = job.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var job = new Jobs.ConvertUVsInt16ToFloatInterleavedJob
                        {
                            inputByteStride = inputByteStride ?? 2 * sizeof(short),
                            input = (short*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = job.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    break;
                case AccessorDataType.Byte:
                    if (normalized)
                    {
                        var jobInt8 = new Jobs.ConvertUVsInt8ToFloatInterleavedNormalizedJob
                        {
                            inputByteStride = inputByteStride ?? 2 * sizeof(sbyte),
                            input = (sbyte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobInt8.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    else
                    {
                        var jobInt8 = new Jobs.ConvertUVsInt8ToFloatInterleavedJob
                        {
                            inputByteStride = inputByteStride ?? 2 * sizeof(sbyte),
                            input = (sbyte*)input,
                            outputByteStride = outputByteStride,
                            result = output
                        };
                        jobHandle = jobInt8.ScheduleBatch(count, GltfImport.DefaultBatchCount);
                    }
                    break;
                default:
                    m_Logger?.Error(LogCode.TypeUnsupported, "UV", inputType.ToString());
                    break;
            }
            Profiler.EndSample();
            return jobHandle;
        }
    }
}
