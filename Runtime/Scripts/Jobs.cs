// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Burst;
using Unity.Cloud.Gltfast.Vertex;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Unity.Cloud.Gltfast.Jobs
{
    [BurstCompile]
    struct CreateIndicesUInt16Job : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<ushort> result;

        public void Execute(int index)
        {
            result[index] = (ushort)index;
        }
    }

    [BurstCompile]
    struct CreateIndicesUInt32Job : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<uint> result;

        public void Execute(int index)
        {
            result[index] = (uint)index;
        }
    }

    [BurstCompile]
    struct CreateIndicesUInt16FlippedJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<ushort> result;

        public void Execute(int index)
        {
            result[index] = (ushort)(index - 2 * (index % 3 - 1));
        }
    }

    [BurstCompile]
    struct CreateIndicesUInt32FlippedJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<uint> result;

        public void Execute(int index)
        {
            result[index] = (uint)(index - 2 * (index % 3 - 1));
        }
    }

    [BurstCompile]
    struct CreateIndicesForTriangleStripUInt16Job : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<ushort> result;

        public void Execute(int index)
        {
            // Source https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html
            // Triangle Strips
            // One triangle primitive is defined by each vertex and the two vertices that follow it, according to the equation:
            // pi = { vi, vi + (1 + i % 2), vi + (2 - i % 2)}
            // We swap the second and third indices for Unity

            var triangleIndex = index / 3;
            result[index] = (index % 3) switch
            {
                0 => (ushort)(triangleIndex),
                1 => (ushort)(triangleIndex + (2 - triangleIndex % 2)),
                2 => (ushort)(triangleIndex + (1 + triangleIndex % 2)),
                _ => result[index]
            };
        }
    }

    [BurstCompile]
    struct CreateIndicesForTriangleStripUInt32Job : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<uint> result;

        public void Execute(int index)
        {
            // Source https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html
            // Triangle Strips
            // One triangle primitive is defined by each vertex and the two vertices that follow it, according to the equation:
            // pi = { vi, vi + (1 + i % 2), vi + (2 - i % 2)}
            // We change first and second indices for Unity

            var triangleIndex = index / 3;
            result[index] = (index % 3) switch
            {
                0 => (uint)triangleIndex,
                1 => (uint)(triangleIndex + 2 - triangleIndex % 2),
                2 => (uint)(triangleIndex + 1 + triangleIndex % 2),
                _ => result[index]
            };
        }
    }

    [BurstCompile]
    struct CreateIndicesForTriangleFanUInt16Job : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<ushort> result;

        public void Execute(int index)
        {
            // Source https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html
            // Triangle Fans
            // Triangle primitives are defined around a shared common vertex, according to the equation:
            // pi = {vi+1, vi+2, v0}
            // We change first and second indices for Unity

            var triangleIndex = index / 3;
            result[index] = (index % 3) switch
            {
                0 => (ushort)(triangleIndex + 2),
                1 => (ushort)(triangleIndex + 1),
                _ => 0
            };
        }
    }

    [BurstCompile]
    struct CreateIndicesForTriangleFanUInt32Job : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<uint> result;

        public void Execute(int index)
        {
            // Source https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html
            // Triangle Fans
            // Triangle primitives are defined around a shared common vertex, according to the equation:
            // pi = {vi+1, vi+2, v0}
            // We change first and second indices for Unity

            var triangleIndex = index / 3;
            result[index] = (index % 3) switch
            {
                0 => (uint)(triangleIndex + 2),
                1 => (uint)(triangleIndex + 1),
                _ => 0
            };
        }
    }

    [BurstCompile]
    struct RecalculateIndicesForTriangleStripInPlaceJob<T> : IJob where T : struct
    {
        public NativeArray<T> indices;

        public void Execute()
        {
            var lastTriangleIndex = indices.Length / 3 - 1;
            for (var i = lastTriangleIndex; i > 0; i--)
            {
                var mod = i % 2;
                indices[i * 3 + 1 + mod] = indices[2 + i];
                indices[i * 3 + 2 - mod] = indices[1 + i];
                indices[i * 3] = indices[i];
            }
            (indices[2], indices[1]) = (indices[1], indices[2]);
        }
    }

    [BurstCompile]
    struct RecalculateIndicesForTriangleFanInPlaceJob<T> : IJob where T : struct
    {
        public NativeArray<T> indices;

        public void Execute()
        {
            var triangleCount = indices.Length / 3;
            var pivot = indices[0];
            for (var i = triangleCount - 1; i > 0; i--)
            {
                var triangleIndex = i * 3;
                indices[triangleIndex + 2] = pivot;
                indices[triangleIndex + 1] = indices[i + 1];
                indices[triangleIndex] = indices[i + 2];
            }
            (indices[2], indices[0]) = (indices[0], indices[2]);
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt8ToUInt16Job : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte>.ReadOnly input;

        [WriteOnly]
        public NativeArray<ushort> result;

        public void Execute(int index)
        {
            result[index] = input[index];
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt8ToUInt32Job : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte>.ReadOnly input;

        [WriteOnly]
        public NativeArray<uint> result;

        public void Execute(int index)
        {
            result[index] = input[index];
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt8ToUInt16FlippedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte3>.ReadOnly input;

        [WriteOnly]
        public NativeArray<ushort3> result;

        public void Execute(int index)
        {
            result[index] = input[index].GltfToUnityTriangleIndicesUInt16();
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt8ToUInt32FlippedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte3>.ReadOnly input;

        [WriteOnly]
        public NativeArray<uint3> result;

        public void Execute(int index)
        {
            result[index] = input[index].GltfToUnityTriangleIndices();
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt16ToUInt16FlippedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<ushort3>.ReadOnly input;

        [WriteOnly]
        public NativeArray<ushort3> result;

        public void Execute(int index)
        {
            result[index] = input[index].GltfToUnityTriangleIndicesUInt16();
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt16ToUInt32FlippedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<ushort3>.ReadOnly input;

        [WriteOnly]
        public NativeArray<uint3> result;

        public void Execute(int index)
        {
            result[index] = input[index].GltfToUnityTriangleIndices();
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt16ToUInt32Job : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<ushort>.ReadOnly input;

        [WriteOnly]
        public NativeArray<uint> result;

        public void Execute(int index)
        {
            result[index] = input[index];
        }
    }

    [BurstCompile]
    struct ConvertIndicesUInt32ToUInt32FlippedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<uint3>.ReadOnly input;

        [WriteOnly]
        public NativeArray<uint3> result;

        public void Execute(int index)
        {
            var idx = input[index];
            result[index] = new uint3(idx.x, idx.z, idx.y);
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsUInt8ToFloatInterleavedJob : IJobParallelForBatch
    {
        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var off = input + (startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                *resultV = new float2(off[0], 1 - off[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsUInt8ToFloatInterleavedNormalizedJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var off = input + inputByteStride * index;
            var tmp = new float2(
                off[0],
                off[1]
                ) / 255f;
            tmp.y = 1 - tmp.y;
            *resultV = tmp;
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsUInt16ToFloatInterleavedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var uv = (ushort*)(input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                *resultV = new float2(uv[0], 1 - uv[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                uv = (ushort*)((byte*)uv + inputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsUInt16ToFloatInterleavedNormalizedJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int index)
        {
            var resultV = (float2*)(((byte*)result) + (index * outputByteStride));
            var uv = (ushort*)(input + inputByteStride * index);
            var tmp = new float2(
                uv[0],
                uv[1]
            ) / ushort.MaxValue;
            tmp.y = 1 - tmp.y;
            *resultV = tmp;
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt16ToFloatInterleavedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public short* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var uv = (short*)((byte*)input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                *resultV = new float2(uv[0], 1 - uv[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                uv = (short*)((byte*)uv + inputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt16ToFloatInterleavedNormalizedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public short* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var uv = (short*)((byte*)input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                var tmp = new float2(uv[0], uv[1]) / short.MaxValue;
                var tmp2 = max(tmp, -1f);
                tmp2.y = 1 - tmp2.y;
                *resultV = tmp2;

                resultV = (float2*)((byte*)resultV + outputByteStride);
                uv = (short*)((byte*)uv + inputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt8ToFloatInterleavedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var off = input + startIndex * inputByteStride;

            for (var index = 0; index < count; index++)
            {
                *resultV = new float2(off[0], 1 - off[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt8ToFloatInterleavedNormalizedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var off = input + startIndex * inputByteStride;

            for (var index = 0; index < count; index++)
            {
                var tmp = new float2(off[0], off[1]) / 127f;
                var tmp2 = max(tmp, -1f);
                tmp2.y = 1 - tmp2.y;
                *resultV = tmp2;

                resultV = (float2*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertColorsRGBFloatToRGBAFloatJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int index)
        {
            var src = (float3*)(input + (index * inputByteStride));
            result[index] = new float4(*src, 1f);
        }
    }

    [BurstCompile]
    unsafe struct ConvertColorsRgbUInt8ToRGBAFloatJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int index)
        {
            var src = input + (index * inputByteStride);
            result[index] = new float4(
                new float3(src[0], src[1], src[2]) / byte.MaxValue,
                1f
            );
        }
    }

    [BurstCompile]
    unsafe struct ConvertColorsRgbUInt16ToRGBAFloatJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ushort* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int index)
        {
            var src = (ushort*)(((byte*)input) + (index * inputByteStride));
            result[index] = new float4(
                new float3(src[0], src[1], src[2]) / ushort.MaxValue,
                1f
            );
        }
    }

    [BurstCompile]
    unsafe struct ConvertColorsRgbaUInt16ToRGBAFloatJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ushort* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int startIndex, int count)
        {
            var src = (ushort*)((byte*)input + startIndex * inputByteStride);
            var endIndex = startIndex + count;
            for (var index = startIndex; index < endIndex; index++)
            {
                result[index] = new float4(
                    src[0] / (float)ushort.MaxValue,
                    src[1] / (float)ushort.MaxValue,
                    src[2] / (float)ushort.MaxValue,
                    src[3] / (float)ushort.MaxValue
                );
                src = (ushort*)((byte*)src + inputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertColorsRGBAFloatToRGBAFloatJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int startIndex, int count)
        {
            var src = (float4*)(input + startIndex * inputByteStride);
            var endIndex = startIndex + count;
            for (var index = startIndex; index < endIndex; index++)
            {
                result[index] = *src;
                src = (float4*)((byte*)src + inputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertColorsRgbaUInt8ToRGBAFloatJob : IJobParallelFor
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int index)
        {
            var src = input + (index * inputByteStride);
            result[index] = new float4(
                src[0] / (float)byte.MaxValue,
                src[1] / (float)byte.MaxValue,
                src[2] / (float)byte.MaxValue,
                src[3] / (float)byte.MaxValue
            );
        }
    }

    [BurstCompile]
    unsafe struct MemCopyJob : IJob
    {

        [ReadOnly]
        public long bufferSize;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* result;

        public void Execute()
        {
            UnsafeUtility.MemCpy(result, input, bufferSize);
        }
    }

    /// <summary>
    /// General purpose vector 3 (position or normal) conversion
    /// </summary>
    [BurstCompile]
    struct ConvertVector3FloatToFloatJob : IJobParallelFor
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<float3> input;

        [WriteOnly]
        public NativeArray<float3> result;

        public void Execute(int index)
        {
            var tmp = input[index];
            tmp.x *= -1;
            result[index] = tmp;
        }
    }

    [BurstCompile]
    struct ConvertRotationsFloatToFloatJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float4>.ReadOnly input;

        [WriteOnly]
        public NativeArray<quaternion> result;

        public void Execute(int index)
        {
            var tmp = input[index];
            tmp.y *= -1;
            tmp.z *= -1;
            result[index] = tmp;
        }
    }

    [BurstCompile]
    struct ConvertRotationsInt16ToFloatJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<short4>.ReadOnly input;

        [WriteOnly]
        public NativeArray<quaternion> result;

        public void Execute(int index)
        {
            result[index] = input[index].GltfToUnityRotation();
        }
    }

    /// <summary>
    /// Converts an array of glTF space quaternions (normalized, signed bytes) to
    /// Quaternions in Unity space (floats).
    /// </summary>
    [BurstCompile]
    struct ConvertRotationsInt8ToFloatJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<sbyte4>.ReadOnly input;

        [WriteOnly]
        public NativeArray<quaternion> result;

        public void Execute(int index)
        {
            result[index] = input[index].GltfToUnityRotation();
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsFloatToFloatInterleavedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float2* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float2*)((byte*)result + startIndex * outputByteStride);
            var off = (float2*)(input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                var tmp = *off;
                tmp.y = 1 - tmp.y;
                *resultV = tmp;

                resultV = (float2*)((byte*)resultV + outputByteStride);
                off = (float2*)((byte*)off + inputByteStride);
            }
        }
    }

    /// <summary>
    /// General purpose vector 3 (position or normal) conversion
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3FloatToFloatInterleavedJob : IJobParallelForBatch
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<float3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);

            var end = startIndex + count;
            for (var index = startIndex; index < end; index++)
            {
                var tmp = input[index];
                tmp.x *= -1;
                *resultV = tmp;

                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    /// <summary>
    /// General purpose sparse vector 3 (position or normal) conversion
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3SparseJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* indexBuffer;

        public FunctionPointer<CachedFunction.GetIndexDelegate> indexConverter;

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* input;

        public FunctionPointer<CachedFunction.GetFloat3Delegate> valueConverter;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int index)
        {
            var resultIndex = indexConverter.Invoke(indexBuffer, index);
            var resultV = (float3*)(((byte*)result) + (resultIndex * outputByteStride));
            valueConverter.Invoke(resultV, (byte*)input + index * inputByteStride);
        }
    }

    [BurstCompile]
    unsafe struct ConvertTangentsFloatToFloatInterleavedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = (float4*)(input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                var tmp = *off;
                tmp.z *= -1;
                *resultV = tmp;

                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (float4*)((byte*)off + inputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertBoneWeightsFloatToFloatInterleavedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = (float4*)(input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                *resultV = *off;
                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (float4*)((byte*)off + inputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertBoneWeightsUInt8ToFloatInterleavedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = input + startIndex * inputByteStride;

            for (var index = 0; index < count; index++)
            {
                *resultV = new float4(
                    off[0] / 255f,
                    off[1] / 255f,
                    off[2] / 255f,
                    off[3] / 255f
                    );
                resultV = (float4*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertBoneWeightsUInt16ToFloatInterleavedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = (ushort*)(input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                *resultV = new float4(
                    off[0] / (float)ushort.MaxValue,
                    off[1] / (float)ushort.MaxValue,
                    off[2] / (float)ushort.MaxValue,
                    off[3] / (float)ushort.MaxValue
                    );
                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (ushort*)((byte*)off + inputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertTangentsInt16ToFloatInterleavedNormalizedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public short* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = (short*)((byte*)input + startIndex * inputByteStride);

            for (var index = 0; index < count; index++)
            {
                var tmp = new float4(off[0], off[1], off[2], off[3]) / short.MaxValue;
                var tmp2 = max(tmp, -1f);
                tmp2.z *= -1;
                *resultV = normalize(tmp2);

                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (short*)((byte*)off + inputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertTangentsInt8ToFloatInterleavedNormalizedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float4*)((byte*)result + startIndex * outputByteStride);
            var off = input + startIndex * inputByteStride;

            for (var index = 0; index < count; index++)
            {
                var tmp = new float4(off[0], off[1], off[2], off[3]) / 127f;
                var tmp2 = max(tmp, -1f);
                tmp2.z *= -1;
                *resultV = normalize(tmp2);

                resultV = (float4*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt16ToFloatInterleavedJob : IJobParallelForBatch
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<ushort3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt16ToFloatInterleavedNormalizedJob : IJobParallelForBatch
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<ushort3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityNormalizedFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertPositionsInt16ToFloatInterleavedJob : IJobParallelForBatch
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<short3> input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        [ReadOnly]
        public int outputByteStride;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    /// <summary>
    /// General purpose (position / morph target delta normal)
    /// Result is not normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3Int16ToFloatInterleavedNormalizedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public ReadOnlyNativeStridedArray<short3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityNormalizedFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    /// <summary>
    /// Normal conversion
    /// Result is normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertNormalsInt16ToFloatInterleavedNormalizedJob : IJobParallelForBatch
    {

        [ReadOnly]
        public ReadOnlyNativeStridedArray<short3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfNormalToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertPositionsInt8ToFloatInterleavedJob : IJobParallelForBatch
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<sbyte3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    /// <summary>
    /// General purpose conversion (positions or morph target delta normals)
    /// Result is not normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3Int8ToFloatInterleavedNormalizedJob : IJobParallelForBatch
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<sbyte3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityNormalizedFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    /// <summary>
    /// Normal conversion
    /// Result is normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertNormalsInt8ToFloatInterleavedNormalizedJob : IJobParallelForBatch
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<sbyte3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfNormalToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt8ToFloatInterleavedJob : IJobParallelForBatch
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<byte3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt8ToFloatInterleavedNormalizedJob : IJobParallelForBatch
    {
        [ReadOnly]
        public ReadOnlyNativeStridedArray<byte3> input;

        [ReadOnly]
        public int outputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int startIndex, int count)
        {
            var resultV = (float3*)((byte*)result + startIndex * outputByteStride);
            var end = startIndex + count;

            for (var index = startIndex; index < end; index++)
            {
                *resultV = input[index].GltfToUnityNormalizedFloat3();
                resultV = (float3*)((byte*)resultV + outputByteStride);
            }
        }
    }

    [BurstCompile]
    unsafe struct ConvertBoneJointsUInt8ToUInt32Job : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int inputByteStride;

        [WriteOnly]
        [NativeDisableUnsafePtrRestriction]
        public uint4* result;

        [ReadOnly]
        public int outputByteStride;

        public void Execute(int index)
        {
            var resultV = (uint4*)(((byte*)result) + (index * outputByteStride));
            var off = input + (index * inputByteStride);
            *resultV = new uint4(off[0], off[1], off[2], off[3]);
        }
    }

    [BurstCompile]
    unsafe struct ConvertBoneJointsUInt16ToUInt32Job : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        public int inputByteStride;

        [WriteOnly]
        [NativeDisableUnsafePtrRestriction]
        public uint4* result;

        [ReadOnly]
        public int outputByteStride;

        public void Execute(int index)
        {
            var resultV = (uint4*)(((byte*)result) + (index * outputByteStride));
            var off = (ushort*)(input + (index * inputByteStride));
            *resultV = new uint4(off[0], off[1], off[2], off[3]);
        }
    }

    [BurstCompile]
    struct SortAndNormalizeBoneWeightsJob : IJobParallelFor
    {

        public NativeArray<VBones> bones;

        /// <summary>
        /// Number of skin weights that are taken into account (project quality setting)
        /// </summary>
        public int skinWeights;


        public unsafe void Execute(int index)
        {
            var v = bones[index];

            // Most joints/weights are already sorted by weight
            // Detect and early return if true
            var sortedAndNormalized = true;
            for (var i = 0; i < 3; i++)
            {
                var a = v.weights[i];
                var b = v.weights[i + 1];
                if (a < b)
                {
                    sortedAndNormalized = false;
                    break;
                }
            }

            // Sort otherwise
            if (!sortedAndNormalized)
            {
                for (var i = 0; i < skinWeights; i++)
                {
                    var max = v.weights[i];
                    var maxI = i;

                    for (var j = i + 1; j < 4; j++)
                    {
                        var value = v.weights[j];
                        if (v.weights[j] > max)
                        {
                            max = value;
                            maxI = j;
                        }
                    }

                    if (maxI > i)
                    {
                        Swap(ref v, maxI, i);
                    }
                }
            }

            // Calculate the sum of weights
            var weightSum = 0f;
            for (var i = 0; i < skinWeights; i++)
            {
                weightSum += v.weights[i];
            }
            if (abs(weightSum - 1.0f) > 2e-7f && weightSum > 0)
            {
                sortedAndNormalized = false;
                // Re-normalize the weight sum
                for (var i = 0; i < skinWeights; i++)
                {
                    v.weights[i] /= weightSum;
                }
            }

            if (!sortedAndNormalized)
            {
                bones[index] = v;
            }
        }

        static unsafe void Swap(ref VBones v, int a, int b)
        {
            (v.weights[a], v.weights[b]) = (v.weights[b], v.weights[a]);
            (v.joints[a], v.joints[b]) = (v.joints[b], v.joints[a]);
        }
    }

#if GLTFAST_SAFE
    [BurstCompile]
    struct RenormalizeBoneWeightsJob : IJobParallelFor {

        public NativeArray<VBones> bones;

        public unsafe void Execute(int index) {
            var v = bones[index];

            // Calculate the sum of weights
            var weightSum = v.weights[0] + v.weights[1] + v.weights[2] + v.weights[3];
            if (abs(weightSum - 1.0f) > 2e-7f && weightSum > 0) {
                // Re-normalize the weight sum
                for (var i = 0; i < 4; i++) {
                    v.weights[i] /= weightSum;
                }
            }

            bones[index] = v;
        }
    }
#endif

    [BurstCompile]
    struct ConvertMatricesJob : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<float4x4>.ReadOnly input;

        [WriteOnly]
        public NativeArray<float4x4> result;

        public void Execute(int index)
        {
            var tmp = input[index];
            result[index] = new float4x4(
                tmp.c0.x, -tmp.c1.x, -tmp.c2.x, -tmp.c3.x,
                -tmp.c0.y, tmp.c1.y, tmp.c2.y, tmp.c3.y,
                -tmp.c0.z, tmp.c1.z, tmp.c2.z, tmp.c3.z,
                tmp.c0.w, tmp.c1.w, tmp.c2.w, tmp.c3.w
                );
        }
    }

    [BurstCompile]
    struct ConvertScalarInt8ToFloatNormalizedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<sbyte>.ReadOnly input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int index)
        {
            result[index] = max(input[index] / 127f, -1.0f);
        }
    }

    [BurstCompile]
    struct ConvertScalarUInt8ToFloatNormalizedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<byte>.ReadOnly input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int index)
        {
            result[index] = input[index] / 255f;
        }
    }

    [BurstCompile]
    struct ConvertScalarInt16ToFloatNormalizedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<short>.ReadOnly input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int index)
        {
            result[index] = max(input[index] / (float)short.MaxValue, -1.0f);
        }
    }

    [BurstCompile]
    struct ConvertScalarUInt16ToFloatNormalizedJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<ushort>.ReadOnly input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int index)
        {
            result[index] = input[index] / (float)ushort.MaxValue;
        }
    }
}
