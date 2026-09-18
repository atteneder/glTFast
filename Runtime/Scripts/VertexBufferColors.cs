// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
using System.Collections.Generic;
#endif
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

    sealed class VertexBufferColors : IDisposable
    {
        readonly ICodeLogger m_Logger;

        NativeArray<float4> m_Data;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        List<AtomicSafetyHandle> m_SafetyHandles;
#endif

        public VertexBufferColors(int vertexCount, ICodeLogger logger)
        {
            m_Logger = logger;
            Profiler.BeginSample("VertexBufferColors.Allocate");
            m_Data = new NativeArray<float4>(vertexCount, VertexBufferGeneratorBase.defaultAllocator);
            Profiler.EndSample();
        }

        public unsafe bool ScheduleVertexColorJob(
            int colorAccessorIndex,
            int offset,
            NativeArray<JobHandle> handles,
            BufferStore buffers
            )
        {
            Profiler.BeginSample("VertexBufferColors.Schedule");
            buffers.GetAccessorAndData(colorAccessorIndex, out var colorAcc, out var data, out var byteStride);
            if (colorAcc.IsSparse)
            {
                m_Logger?.Error(LogCode.SparseAccessor, "color");
            }

            var colorDestination = m_Data.GetSubArray(offset, colorAcc.Count);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (offset > 0)
            {
                // On multi-primitive meshes color jobs may only write into the same destination if we set dedicated
                // safety handles for each sub-array following the first one (offset==0).
                var safetyHandle = AtomicSafetyHandle.Create();
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref colorDestination, safetyHandle);
                m_SafetyHandles ??= new List<AtomicSafetyHandle>();
                m_SafetyHandles.Add(safetyHandle);
            }
#endif

            var h = GetColors32Job(
                data,
                colorAcc.ComponentType,
                colorAcc.Type.Value,
                byteStride,
                colorDestination
            );
            if (h.HasValue)
            {
                handles[0] = h.Value;
            }
            else
            {
                Profiler.EndSample();
                return false;
            }
            Profiler.EndSample();
            return true;
        }

        public void AddDescriptors(VertexAttributeDescriptor[] dst, int offset, int stream)
        {
            dst[offset] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, stream);
        }

        public void ApplyOnMesh(
            UnityEngine.Mesh msh,
            int stream,
            MeshUpdateFlags flags = MeshGeneratorBase.defaultMeshUpdateFlags
            )
        {
            Profiler.BeginSample("ApplyUVs");
            msh.SetVertexBufferData(m_Data, 0, 0, m_Data.Length, stream, flags);
            Profiler.EndSample();
        }

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (m_SafetyHandles != null)
            {
                foreach (var handle in m_SafetyHandles)
                {
                    AtomicSafetyHandle.CheckDeallocateAndThrow(handle);
                    AtomicSafetyHandle.Release(handle);
                }
                m_SafetyHandles = null;
            }
#endif
            if (m_Data.IsCreated)
            {
                m_Data.Dispose();
            }
        }

        unsafe JobHandle? GetColors32Job(
            void* input,
            AccessorDataType inputType,
            AccessorType type,
            int? inputByteStride,
            NativeArray<float4> output
            )
        {
            Profiler.BeginSample("PrepareColors32");
            JobHandle? jobHandle = null;

            if (type == AccessorType.Vector3)
            {
                switch (inputType)
                {
                    case AccessorDataType.UnsignedByte:
                    {
                        var job = new Jobs.ConvertColorsRgbUInt8ToRGBAFloatJob
                        {
                            input = (byte*)input,
                            inputByteStride = inputByteStride ?? 3 * sizeof(byte),
                            result = output
                        };
                        jobHandle = job.Schedule(output.Length, GltfImport.DefaultBatchCount);
                    }
                    break;
                    case AccessorDataType.Float:
                    {
                        var job = new Jobs.ConvertColorsRGBFloatToRGBAFloatJob
                        {
                            input = (byte*)input,
                            inputByteStride = inputByteStride ?? sizeof(float3),
                            result = output
                        };
                        jobHandle = job.Schedule(output.Length, GltfImport.DefaultBatchCount);
                    }
                    break;
                    case AccessorDataType.UnsignedShort:
                    {
                        var job = new Jobs.ConvertColorsRgbUInt16ToRGBAFloatJob
                        {
                            input = (ushort*)input,
                            inputByteStride = inputByteStride ?? 3 * sizeof(ushort),
                            result = output
                        };
                        jobHandle = job.Schedule(output.Length, GltfImport.DefaultBatchCount);
                    }
                    break;
                    default:
                        m_Logger?.Error(LogCode.ColorFormatUnsupported, type.ToString());
                        break;
                }
            }
            else if (type == AccessorType.Vector4)
            {
                switch (inputType)
                {
                    case AccessorDataType.UnsignedByte:
                    {
                        var job = new Jobs.ConvertColorsRgbaUInt8ToRGBAFloatJob
                        {
                            input = (byte*)input,
                            inputByteStride = inputByteStride ?? 4 * sizeof(byte),
                            result = output
                        };
                        jobHandle = job.Schedule(output.Length, GltfImport.DefaultBatchCount);
                    }
                    break;
                    case AccessorDataType.Float:
                    {
                        if (inputByteStride == 16 || inputByteStride <= 0)
                        {
                            var job = new Jobs.MemCopyJob
                            {
                                bufferSize = output.Length * 16,
                                input = input,
                                result = output.GetUnsafeReadOnlyPtr()
                            };
                            jobHandle = job.Schedule();
                        }
                        else
                        {
                            var job = new Jobs.ConvertColorsRGBAFloatToRGBAFloatJob
                            {
                                input = (byte*)input,
                                inputByteStride = inputByteStride ?? sizeof(float4),
                                result = output
                            };
                            jobHandle = job.ScheduleBatch(output.Length, GltfImport.DefaultBatchCount);
                        }
                    }
                    break;
                    case AccessorDataType.UnsignedShort:
                    {
                        var job = new Jobs.ConvertColorsRgbaUInt16ToRGBAFloatJob
                        {
                            input = (ushort*)input,
                            inputByteStride = inputByteStride ?? 4 * sizeof(ushort),
                            result = output
                        };
                        jobHandle = job.ScheduleBatch(output.Length, GltfImport.DefaultBatchCount);
                    }
                    break;
                    default:
                        m_Logger?.Error(LogCode.ColorFormatUnsupported, type.ToString());
                        break;
                }
            }
            else
            {
                m_Logger?.Error(LogCode.TypeUnsupported, "color accessor", inputType.ToString());
            }
            Profiler.EndSample();
            return jobHandle;
        }
    }
}
