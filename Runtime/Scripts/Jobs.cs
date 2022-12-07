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

using System.Runtime.CompilerServices;
using System;
using AOT;
using GLTFast.Vertex;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;

[assembly: InternalsVisibleTo("glTF-test-framework.Tests")]

namespace GLTFast.Jobs
{

    using Schema;

    [BurstCompile]
    static unsafe class CachedFunction
    {

        public delegate int GetIndexDelegate(void* baseAddress, int index);
        public delegate void GetFloat3Delegate(float3* destination, void* src);

        // Cached function pointers
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueInt8Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueUInt8Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueInt16Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueUInt16Method;
        static FunctionPointer<GetIndexDelegate> s_GetIndexValueUInt32Method;


        static FunctionPointer<GetFloat3Delegate> s_GetFloat3FloatMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int8Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt8Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int16Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt16Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt32Method;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int8NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt8NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3Int16NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt16NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> s_GetFloat3UInt32NormalizedMethod;

        /// <summary>
        /// Returns Burst compatible function that retrieves an index value
        /// </summary>
        /// <param name="format">Data type of index</param>
        /// <returns>Burst Function Pointer to correct conversion function</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static FunctionPointer<GetIndexDelegate> GetIndexConverter(GltfComponentType format)
        {
            switch (format)
            {
                case GltfComponentType.UnsignedByte:
                    if (!s_GetIndexValueUInt8Method.IsCreated)
                    {
                        s_GetIndexValueUInt8Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt8);
                    }
                    return s_GetIndexValueUInt8Method;
                case GltfComponentType.Byte:
                    if (!s_GetIndexValueInt8Method.IsCreated)
                    {
                        s_GetIndexValueInt8Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueInt8);
                    }
                    return s_GetIndexValueInt8Method;
                case GltfComponentType.UnsignedShort:
                    if (!s_GetIndexValueUInt16Method.IsCreated)
                    {
                        s_GetIndexValueUInt16Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt16);
                    }
                    return s_GetIndexValueUInt16Method;
                case GltfComponentType.Short:
                    if (!s_GetIndexValueInt16Method.IsCreated)
                    {
                        s_GetIndexValueInt16Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueInt16);
                    }
                    return s_GetIndexValueInt16Method;
                case GltfComponentType.UnsignedInt:
                    if (!s_GetIndexValueUInt32Method.IsCreated)
                    {
                        s_GetIndexValueUInt32Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt32);
                    }
                    return s_GetIndexValueUInt32Method;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        public static FunctionPointer<GetFloat3Delegate> GetPositionConverter(
            GltfComponentType format,
            bool normalized
            )
        {
            if (normalized)
            {
                switch (format)
                {
                    case GltfComponentType.Float:
                        // Floats cannot be normalized.
                        // Fall back to non-normalized below
                        break;
                    case GltfComponentType.Byte:
                        if (!s_GetFloat3Int8NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3Int8NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int8Normalized);
                        }
                        return s_GetFloat3Int8NormalizedMethod;
                    case GltfComponentType.UnsignedByte:
                        if (!s_GetFloat3UInt8NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3UInt8NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt8Normalized);
                        }
                        return s_GetFloat3UInt8NormalizedMethod;
                    case GltfComponentType.Short:
                        if (!s_GetFloat3Int16NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3Int16NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int16Normalized);
                        }
                        return s_GetFloat3Int16NormalizedMethod;
                    case GltfComponentType.UnsignedShort:
                        if (!s_GetFloat3UInt16NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3UInt16NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt16Normalized);
                        }
                        return s_GetFloat3UInt16NormalizedMethod;
                    case GltfComponentType.UnsignedInt:
                        if (!s_GetFloat3UInt32NormalizedMethod.IsCreated)
                        {
                            s_GetFloat3UInt32NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt32Normalized);
                        }
                        return s_GetFloat3UInt32NormalizedMethod;
                }
            }
            switch (format)
            {
                case GltfComponentType.Float:
                    if (!s_GetFloat3FloatMethod.IsCreated)
                    {
                        s_GetFloat3FloatMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Float);
                    }
                    return s_GetFloat3FloatMethod;
                case GltfComponentType.Byte:
                    if (!s_GetFloat3Int8Method.IsCreated)
                    {
                        s_GetFloat3Int8Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int8);
                    }
                    return s_GetFloat3Int8Method;
                case GltfComponentType.UnsignedByte:
                    if (!s_GetFloat3UInt8Method.IsCreated)
                    {
                        s_GetFloat3UInt8Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt8);
                    }
                    return s_GetFloat3UInt8Method;
                case GltfComponentType.Short:
                    if (!s_GetFloat3Int16Method.IsCreated)
                    {
                        s_GetFloat3Int16Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int16);
                    }
                    return s_GetFloat3Int16Method;
                case GltfComponentType.UnsignedShort:
                    if (!s_GetFloat3UInt16Method.IsCreated)
                    {
                        s_GetFloat3UInt16Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt16);
                    }
                    return s_GetFloat3UInt16Method;
                case GltfComponentType.UnsignedInt:
                    if (!s_GetFloat3UInt32Method.IsCreated)
                    {
                        s_GetFloat3UInt32Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt32);
                    }
                    return s_GetFloat3UInt32Method;
            }
            throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt8(void* baseAddress, int index)
        {
            return *((byte*)baseAddress + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueInt8(void* baseAddress, int index)
        {
            return *(((sbyte*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt16(void* baseAddress, int index)
        {
            return *(((ushort*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueInt16(void* baseAddress, int index)
        {
            return *(((short*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt32(void* baseAddress, int index)
        {
            return (int)*(((uint*)baseAddress) + index);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Float(float3* destination, void* src)
        {
            destination->x = -*(float*)src;
            destination->y = *((float*)src + 1);
            destination->z = *((float*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int8(float3* destination, void* src)
        {
            destination->x = -*(sbyte*)src;
            destination->y = *((sbyte*)src + 1);
            destination->z = *((sbyte*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt8(float3* destination, void* src)
        {
            destination->x = -*(byte*)src;
            destination->y = *((byte*)src + 1);
            destination->z = *((byte*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int16(float3* destination, void* src)
        {
            destination->x = -*(short*)src;
            destination->y = *((short*)src + 1);
            destination->z = *((short*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt16(float3* destination, void* src)
        {
            destination->x = -*(ushort*)src;
            destination->y = *((ushort*)src + 1);
            destination->z = *((ushort*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt32(float3* destination, void* src)
        {
            destination->x = -*(uint*)src;
            destination->y = *((uint*)src + 1);
            destination->z = *((uint*)src + 2);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int8Normalized(float3* destination, void* src)
        {
            destination->x = -max(*(sbyte*)src / 127f, -1);
            destination->y = max(*((sbyte*)src + 1) / 127f, -1);
            destination->z = max(*((sbyte*)src + 2) / 127f, -1);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt8Normalized(float3* destination, void* src)
        {
            destination->x = -*(byte*)src / 255f;
            destination->y = *((byte*)src + 1) / 255f;
            destination->z = *((byte*)src + 2) / 255f;
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int16Normalized(float3* destination, void* src)
        {
            destination->x = -max(*(short*)src / (float)short.MaxValue, -1f);
            destination->y = max(*((short*)src + 1) / (float)short.MaxValue, -1f);
            destination->z = max(*((short*)src + 2) / (float)short.MaxValue, -1f);
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt16Normalized(float3* destination, void* src)
        {
            destination->x = -*(ushort*)src / (float)ushort.MaxValue;
            destination->y = *((ushort*)src + 1) / (float)ushort.MaxValue;
            destination->z = *((ushort*)src + 2) / (float)ushort.MaxValue;
        }

        [BurstCompile, MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt32Normalized(float3* destination, void* src)
        {
            destination->x = -*(uint*)src / (float)uint.MaxValue;
            destination->y = *((uint*)src + 1) / (float)uint.MaxValue;
            destination->z = *((uint*)src + 2) / (float)uint.MaxValue;
        }
    }

    [BurstCompile]
    unsafe struct CreateIndicesInt32Job : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i] = i;
        }
    }

    [BurstCompile]
    unsafe struct CreateIndicesInt32FlippedJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i] = i - 2 * (i % 3 - 1);
        }
    }

    [BurstCompile]
    unsafe struct ConvertIndicesUInt8ToInt32Job : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i] = input[i];
        }
    }

    [BurstCompile]
    unsafe struct ConvertIndicesUInt8ToInt32FlippedJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int3* result;

        public void Execute(int i)
        {
            result[i] = new int3(
                input[i * 3],
                input[i * 3 + 2],
                input[i * 3 + 1]
                );
        }
    }

    [BurstCompile]
    unsafe struct ConvertIndicesUInt16ToInt32FlippedJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ushort* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int3* result;

        public void Execute(int i)
        {
            result[i] = new int3(
                input[i * 3],
                input[i * 3 + 2],
                input[i * 3 + 1]
            );
        }
    }

    [BurstCompile]
    unsafe struct ConvertIndicesUInt16ToInt32Job : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ushort* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i] = input[i];
        }
    }

    [BurstCompile]
    unsafe struct ConvertIndicesUInt32ToInt32Job : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public uint* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i] = (int)input[i];
        }
    }

    [BurstCompile]
    unsafe struct ConvertIndicesUInt32ToInt32FlippedJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public uint* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int3* result;

        public void Execute(int i)
        {
            result[i] = new int3(
                (int)input[i * 3],
                (int)input[i * 3 + 2],
                (int)input[i * 3 + 1]
            );
        }
    }

    // /// <summary>
    // /// Unused at the moment in favor of interleaved variant
    // /// TODO: Investigate if this would be faster
    // /// when input is not interleaved
    // /// </summary>
    // [BurstCompile]
    // unsafe struct ConvertUVsUInt8ToFloatJob : IJobParallelFor  {
    //
    //     [ReadOnly]
    //     [NativeDisableUnsafePtrRestriction]
    //     public byte* input;
    //
    //     [ReadOnly]
    //     [NativeDisableUnsafePtrRestriction]
    //     public float2* result;
    //
    //     public void Execute(int i)
    //     {
    //         result[i].x = input[i*2];
    //         result[i].y = 1 - input[i*2+1];
    //     }
    // }

    // /// <summary>
    // /// Unused at the moment in favor of interleaved variant
    // /// TODO: Investigate if this would be faster
    // /// when input is not interleaved
    // /// </summary>
    // [BurstCompile]
    // unsafe struct ConvertUVsUInt8ToFloatNormalizedJob : IJobParallelFor  {
    //
    //     [ReadOnly]
    //     [NativeDisableUnsafePtrRestriction]
    //     public byte* input;
    //
    //     [ReadOnly]
    //     [NativeDisableUnsafePtrRestriction]
    //     public float2* result;
    //
    //     public void Execute(int i)
    //     {
    //         result[i].x = input[i*2] / 255f;
    //         result[i].y = 1 - input[i*2+1] / 255f;
    //     }
    // }

    // /// <summary>
    // /// Unused at the moment in favor of interleaved variant
    // /// TODO: Investigate if this would be faster
    // /// when input is not interleaved
    // /// </summary>
    // [BurstCompile]
    // unsafe struct ConvertUVsUInt16ToFloatNormalizedJob : IJobParallelFor  {
    //
    //     [ReadOnly]
    //     [NativeDisableUnsafePtrRestriction]
    //     public ushort* input;
    //
    //     [ReadOnly]
    //     [NativeDisableUnsafePtrRestriction]
    //     public float2* result;
    //
    //     public void Execute(int i)
    //     {
    //         result[i].x = input[i*2] / (float) ushort.MaxValue;
    //         result[i].y = 1 - input[i*2+1] / (float) ushort.MaxValue;
    //     }
    // }

    // /// <summary>
    // /// Unused at the moment in favor of interleaved variant
    // /// TODO: Investigate if this would be faster
    // /// when input is not interleaved
    // /// </summary>
    // [BurstCompile]
    // unsafe struct ConvertUVsUInt16ToFloatJob : IJobParallelFor  {
    //
    //     [ReadOnly]
    //     [NativeDisableUnsafePtrRestriction]
    //     public ushort* input;
    //
    //     [ReadOnly]
    //     [NativeDisableUnsafePtrRestriction]
    //     public float2* result;
    //
    //     public void Execute(int i)
    //     {
    //         result[i].x = input[i*2];
    //         result[i].y = 1 - input[i*2+1];
    //     }
    // }

    // /// <summary>
    // /// Unused at the moment in favor of interleaved variant
    // /// TODO: Investigate if this would be faster
    // /// when input is not interleaved
    // /// </summary>
    // [BurstCompile]
    // unsafe struct ConvertUVsFloatToFloatJob : IJobParallelFor {
    //     [ReadOnly]
    //     [NativeDisableUnsafePtrRestriction]
    //     public float* input;
    //
    //     [ReadOnly]
    //     [NativeDisableUnsafePtrRestriction]
    //     public float2* result;
    //
    //     public void Execute(int i) {
    //         result[i].x = ((float*)input)[i*2];
    //         result[i].y = 1-((float*)input)[i*2+1];
    //     }
    // }

    [BurstCompile]
    unsafe struct ConvertUVsUInt8ToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int index, int count) {
            var resultV = (float2*) ((byte*)result + index*outputByteStride);
            var off = input + (index*inputByteStride);

            for (var x = 0; x < count; x++) {
                *resultV = new float2(off[0], 1 - off[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float2*)(((byte*)result) + (i * outputByteStride));
            var off = input + inputByteStride * i;
            *resultV = new float2(off[0], 1 - off[1]);
        }
#endif
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

        public void Execute(int i)
        {
            var resultV = (float2*)(((byte*)result) + (i * outputByteStride));
            var off = input + inputByteStride * i;
            var tmp = new float2(
                off[0],
                off[1]
                ) / 255f;
            tmp.y = 1 - tmp.y;
            *resultV = tmp;
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsUInt16ToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float2*) ((byte*)result + i*outputByteStride);
            var uv = (ushort*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                *resultV = new float2 (uv[0], 1 - uv[1] );
                resultV = (float2*)((byte*)resultV + outputByteStride);
                uv = (ushort*)((byte*)uv + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float2*)(((byte*)result) + (i * outputByteStride));
            var uv = (ushort*)(input + inputByteStride * i);
            *resultV = new float2(uv[0], 1 - uv[1]);
        }
#endif
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

        public void Execute(int i)
        {
            var resultV = (float2*)(((byte*)result) + (i * outputByteStride));
            var uv = (ushort*)(input + inputByteStride * i);
            var tmp = new float2(
                uv[0],
                uv[1]
            ) / ushort.MaxValue;
            tmp.y = 1 - tmp.y;
            *resultV = tmp;
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt16ToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float2*) ((byte*)result + i*outputByteStride);
            var uv = (short*) ((byte*)input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                *resultV = new float2(uv[0],1 - uv[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                uv = (short*)((byte*)uv + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float2*)(((byte*)result) + (i * outputByteStride));
            var uv = (short*)((byte*)input + inputByteStride * i);
            *resultV = new float2(uv[0], 1 - uv[1]);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt16ToFloatInterleavedNormalizedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float2*) ((byte*)result + i*outputByteStride);
            var uv = (short*) ((byte*)input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                var tmp = new float2(uv[0], uv[1]) / short.MaxValue;
                var tmp2 = max(tmp, -1f);
                tmp2.y = 1 - tmp2.y;
                *resultV = tmp2;

                resultV = (float2*)((byte*)resultV + outputByteStride);
                uv = (short*)((byte*)uv + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float2*)(((byte*)result) + (i * outputByteStride));
            var uv = (short*)((byte*)input + inputByteStride * i);

            var tmp = new float2(uv[0], uv[1]) / short.MaxValue;
            var tmp2 = max(tmp, -1f);
            tmp2.y = 1 - tmp2.y;
            *resultV = tmp2;
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt8ToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float2*) ((byte*)result + i*outputByteStride);
            var off = input + i*inputByteStride;

            for (var x = 0; x < count; x++) {
                *resultV = new float2(off[0],1 - off[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float2*)(((byte*)result) + (i * outputByteStride));
            var off = input + inputByteStride * i;
            *resultV = new float2(off[0], 1 - off[1]);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertUVsInt8ToFloatInterleavedNormalizedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float2*) ((byte*)result + i*outputByteStride);
            var off = input + i*inputByteStride;

            for (var x = 0; x < count; x++) {
                var tmp = new float2(off[0],off[1]) / 127f;
                var tmp2 = max(tmp, -1f);
                tmp2.y = 1-tmp2.y;
                *resultV = tmp2;

                resultV = (float2*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float2*)(((byte*)result) + (i * outputByteStride));
            var off = input + inputByteStride * i;
            var tmp = new float2(off[0], off[1]) / 127f;
            var tmp2 = max(tmp, -1f);
            tmp2.y = 1 - tmp2.y;
            *resultV = tmp2;
        }
#endif
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
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

        public void Execute(int i)
        {
            var src = (float3*)(input + (i * inputByteStride));
            result[i] = new float4(*src, 1f);
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

        public void Execute(int i)
        {
            var src = input + (i * inputByteStride);
            result[i] = new float4(
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

        public void Execute(int i)
        {
            var src = (ushort*)(((byte*)input) + (i * inputByteStride));
            result[i] = new float4(
                new float3(src[0], src[1], src[2]) / ushort.MaxValue,
                1f
            );
        }
    }

    [BurstCompile]
    unsafe struct ConvertColorsRgbaUInt16ToRGBAFloatJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ushort* input;

        [WriteOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var src = (ushort*)((byte*)input + i*inputByteStride);
            var endIndex = i + count;
            for (var x = i; x < endIndex; x++) {
                result[x] = new float4 (
                    src[0] / (float)ushort.MaxValue,
                    src[1] / (float)ushort.MaxValue,
                    src[2] / (float)ushort.MaxValue,
                    src[3] / (float)ushort.MaxValue
                );
                src = (ushort*)((byte*)src + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            ushort* src = (ushort*)(((byte*)input) + (i * inputByteStride));
            result[i] = new float4(
                src[0] / (float)ushort.MaxValue,
                src[1] / (float)ushort.MaxValue,
                src[2] / (float)ushort.MaxValue,
                src[3] / (float)ushort.MaxValue
            );
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertColorsRGBAFloatToRGBAFloatJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var src = (float4*)(input + i*inputByteStride);
            var endIndex = i + count;
            for (var x = i; x < endIndex; x++) {
                result[x] = *src;
                src = (float4*)((byte*)src + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var src = (float4*)(input + (i * inputByteStride));
            result[i] = *src;
        }
#endif
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

        public void Execute(int i)
        {
            var src = input + (i * inputByteStride);
            result[i] = new float4(
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
    unsafe struct ConvertVector3FloatToFloatJob : IJobParallelFor
    {
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int i)
        {
            var tmp = input[i];
            tmp.x *= -1;
            result[i] = tmp;
        }
    }

    [BurstCompile]
    unsafe struct ConvertRotationsFloatToFloatJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

        public void Execute(int i)
        {
            var tmp = input[i];
            tmp.y *= -1;
            tmp.z *= -1;
            result[i] = tmp;
        }
    }

    [BurstCompile]
    unsafe struct ConvertRotationsInt16ToFloatJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public short* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* result;

        public void Execute(int i)
        {
            result[i * 4] = Mathf.Max(input[i * 4] / (float)short.MaxValue, -1f);
            result[i * 4 + 1] = -Mathf.Max(input[i * 4 + 1] / (float)short.MaxValue, -1f);
            result[i * 4 + 2] = -Mathf.Max(input[i * 4 + 2] / (float)short.MaxValue, -1f);
            result[i * 4 + 3] = Mathf.Max(input[i * 4 + 3] / (float)short.MaxValue, -1f);
        }
    }

    /// <summary>
    /// Converts an array of glTF space quaternions (normalized, signed bytes) to
    /// Quaternions in Unity space (floats).
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertRotationsInt8ToFloatJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* result;

        public void Execute(int i)
        {
            result[i * 4] = Mathf.Max(input[i * 4] / 127f, -1f);
            result[i * 4 + 1] = -Mathf.Max(input[i * 4 + 1] / 127f, -1f);
            result[i * 4 + 2] = -Mathf.Max(input[i * 4 + 2] / 127f, -1f);
            result[i * 4 + 3] = Mathf.Max(input[i * 4 + 3] / 127f, -1f);
        }
    }

    [BurstCompile]
    unsafe struct ConvertUVsFloatToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float2*) ((byte*)result + i*outputByteStride);
            var off = (float2*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                var tmp = *off;
                tmp.y = 1-tmp.y;
                *resultV = tmp;

                resultV = (float2*)((byte*)resultV + outputByteStride);
                off = (float2*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float2*)(((byte*)result) + (i * outputByteStride));
            var off = (float2*)(input + (i * inputByteStride));
            var tmp = *off;
            tmp.y = 1 - tmp.y;
            *resultV = tmp;
        }
#endif
    }

    /// <summary>
    /// General purpose vector 3 (position or normal) conversion
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3FloatToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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
        public float3* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = (float3*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                var tmp = *off;
                tmp.x *= -1;
                *resultV = tmp;

                resultV = (float3*)((byte*)resultV + outputByteStride);
                off = (float3*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            var off = (float3*)(input + i * inputByteStride);
            var tmp = *off;
            tmp.x *= -1;
            *resultV = tmp;
        }
#endif
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

        public void Execute(int i)
        {
            var index = indexConverter.Invoke(indexBuffer, i);
            var resultV = (float3*)(((byte*)result) + (index * outputByteStride));
            valueConverter.Invoke(resultV, (byte*)input + i * inputByteStride);
        }
    }

    [BurstCompile]
    unsafe struct ConvertTangentsFloatToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float4*) ((byte*)result + i*outputByteStride);
            var off = (float4*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                var tmp = *off;
                tmp.z *= -1;
                *resultV = tmp;

                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (float4*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            float4* resultV = (float4*)(((byte*)result) + (i * outputByteStride));
            byte* off = input + (i * inputByteStride);
            var tmp = *((float4*)off);
            tmp.z *= -1;
            *resultV = tmp;
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertBoneWeightsFloatToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float4*) ((byte*)result + i*outputByteStride);
            var off = (float4*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                *resultV = *off;
                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (float4*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float4*)(((byte*)result) + (i * outputByteStride));
            var off = input + (i * inputByteStride);
            *resultV = *((float4*)off);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertBoneWeightsUInt8ToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float4*) ((byte*)result + i*outputByteStride);
            var off = input + i*inputByteStride;

            for (var x = 0; x < count; x++) {
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
#else
        public void Execute(int i)
        {
            var resultV = (float4*)(((byte*)result) + (i * outputByteStride));
            var off = input + (i * inputByteStride);
            *resultV = new float4(
                off[0] / 255f,
                off[1] / 255f,
                off[2] / 255f,
                off[3] / 255f
                );
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertBoneWeightsUInt16ToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float4*) ((byte*)result + i*outputByteStride);
            var off = (ushort*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                *resultV = new float4(
                    off[0] / (float) ushort.MaxValue,
                    off[1] / (float) ushort.MaxValue,
                    off[2] / (float) ushort.MaxValue,
                    off[3] / (float) ushort.MaxValue
                    );
                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (ushort*) ((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float4*)(((byte*)result) + (i * outputByteStride));
            var off = (ushort*)(input + i * inputByteStride);
            *resultV = new float4(
                off[0] / (float)ushort.MaxValue,
                off[1] / (float)ushort.MaxValue,
                off[2] / (float)ushort.MaxValue,
                off[3] / (float)ushort.MaxValue
                );
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertTangentsInt16ToFloatInterleavedNormalizedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float4*) ((byte*)result + i*outputByteStride);
            var off = (short*) ((byte*)input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                var tmp = new float4(off[0],off[1],off[2],off[3]) / short.MaxValue;
                var tmp2 = max(tmp, -1f);
                tmp2.z *= -1;
                *resultV = normalize(tmp2);

                resultV = (float4*)((byte*)resultV + outputByteStride);
                off = (short*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float4*)(((byte*)result) + (i * outputByteStride));
            var off = (short*)(((byte*)input) + (i * inputByteStride));
            var tmp = new float4(off[0], off[1], off[2], off[3]) / short.MaxValue;
            var tmp2 = max(tmp, -1f);
            tmp2.z *= -1;
            *resultV = normalize(tmp2);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertTangentsInt8ToFloatInterleavedNormalizedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float4*) ((byte*)result + i*outputByteStride);
            var off = input + i*inputByteStride;

            for (var x = 0; x < count; x++) {
                var tmp = new float4(off[0],off[1],off[2],off[3]) / 127f;
                var tmp2 = max(tmp, -1f);
                tmp2.z *= -1;
                *resultV = normalize(tmp2);

                resultV = (float4*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float4*)(((byte*)result) + (i * outputByteStride));
            var off = input + (i * inputByteStride);
            var tmp = new float4(off[0], off[1], off[2], off[3]) / 127f;
            var tmp2 = max(tmp, -1f);
            tmp2.z *= -1;
            *resultV = normalize(tmp2);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt16ToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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
        public float3* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = (ushort*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                *resultV = new float3(-(float)off[0], off[1], off[2]);
                resultV = (float3*)((byte*)resultV + outputByteStride);
                off = (ushort*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            var off = (ushort*)(input + (inputByteStride * i));
            *resultV = new float3(-(float)off[0], off[1], off[2]);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt16ToFloatInterleavedNormalizedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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
        public float3* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = (ushort*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                var tmp = new float3(
                    -(off[0] / (float) ushort.MaxValue),
                    off[1] / (float) ushort.MaxValue,
                    off[2] / (float) ushort.MaxValue
                );
                *resultV = tmp;
                resultV = (float3*)((byte*)resultV + outputByteStride);
                off = (ushort*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            var off = (ushort*)(input + (inputByteStride * i));
            *resultV = new float3(
                -(off[0] / (float)ushort.MaxValue),
                off[1] / (float)ushort.MaxValue,
                off[2] / (float)ushort.MaxValue
            );
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsInt16ToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
    {

        [ReadOnly]
        public int inputByteStride;


        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        [ReadOnly]
        public int outputByteStride;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = (short*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                *resultV = new float3(-off[0],off[1],off[2]);
                resultV = (float3*)((byte*)resultV + outputByteStride);
                off = (short*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            var off = (short*)(input + (i * inputByteStride));
            *resultV = new float3(-(float)off[0], off[1], off[2]);
        }
#endif
    }

    /// <summary>
    /// General purpose (position / morph target delta normal)
    /// Result is not normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3Int16ToFloatInterleavedNormalizedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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
        public float3* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = (short*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                var tmp = new float3(off[0], off[1], off[2]) / short.MaxValue;
                var tmp2 = max(tmp, -1f);
                tmp2.x *= -1;
                *resultV = tmp2;

                resultV = (float3*)((byte*)resultV + outputByteStride);
                off = (short*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            var off = (short*)(input + (i * inputByteStride));

            var tmp = new float3(off[0], off[1], off[2]) / short.MaxValue;
            var tmp2 = max(tmp, -1f);
            tmp2.x *= -1;
            *resultV = tmp2;
        }
#endif
    }

    /// <summary>
    /// Normal conversion
    /// Result is normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertNormalsInt16ToFloatInterleavedNormalizedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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
        public float3* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = (short*) (input + i*inputByteStride);

            for (var x = 0; x < count; x++) {
                var tmp = new float3(off[0], off[1], off[2]) / short.MaxValue;
                var tmp2 = max(tmp, -1f);
                tmp2.x *= -1;
                *resultV = normalize(tmp2);

                resultV = (float3*)((byte*)resultV + outputByteStride);
                off = (short*)((byte*)off + inputByteStride);
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            var off = (short*)(input + (i * inputByteStride));

            var tmp = new float3(off[0], off[1], off[2]) / short.MaxValue;
            var tmp2 = max(tmp, -1f);
            tmp2.x *= -1;
            *resultV = normalize(tmp2);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsInt8ToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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
        public float3* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = input + i*inputByteStride;

            for (var x = 0; x < count; x++) {
                *resultV = new float3(-off[0], off[1], off[2]);
                resultV = (float3*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            var off = input + (inputByteStride * i);
            *resultV = new float3(-(float)off[0], off[1], off[2]);
        }
#endif
    }

    /// <summary>
    /// General purpose conversion (positions or morph target delta normals)
    /// Result is not normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3Int8ToFloatInterleavedNormalizedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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
        public float3* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = input + i*inputByteStride;

            for (var x = 0; x < count; x++) {
                var tmp = new float3(off[0], off[1], off[2]) / 127f;
                var tmp2 = max(tmp, -1);
                tmp2.x *= -1;
                *resultV = tmp2;

                resultV = (float3*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            var off = input + (inputByteStride * i);

            var tmp = new float3(off[0], off[1], off[2]) / 127f;
            var tmp2 = max(tmp, -1);
            tmp2.x *= -1;
            *resultV = tmp2;
        }
#endif
    }

    /// <summary>
    /// Normal conversion
    /// Result is normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertNormalsInt8ToFloatInterleavedNormalizedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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
        public float3* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = input + i*inputByteStride;

            for (var x = 0; x < count; x++) {
                var tmp = new float3(off[0], off[1], off[2]) / 127f;
                var tmp2 = max(tmp, -1);
                tmp2.x *= -1;
                *resultV = normalize(tmp2);

                resultV = (float3*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            var off = input + (inputByteStride * i);

            var tmp = new float3(off[0], off[1], off[2]) / 127f;
            var tmp2 = max(tmp, -1);
            tmp2.x *= -1;
            *resultV = normalize(tmp2);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt8ToFloatInterleavedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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
        public float3* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = input + i*inputByteStride;

            for (var x = 0; x < count; x++) {
                *resultV = new float3(-(float)off[0],off[1],off[2]);
                resultV = (float3*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int i)
        {
            var off = input + (i * inputByteStride);
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            *resultV = new float3(-(float)off[0], off[1], off[2]);
        }
#endif
    }

    [BurstCompile]
    unsafe struct ConvertPositionsUInt8ToFloatInterleavedNormalizedJob :
#if UNITY_JOBS
        IJobParallelForBatch
#else
        IJobParallelFor
#endif
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
        public float3* result;

#if UNITY_JOBS
        public void Execute(int i, int count) {
            var resultV = (float3*) ((byte*)result + i*outputByteStride);
            var off = input + i*inputByteStride;

            for (var x = 0; x < count; x++) {
                *resultV = new float3(
                    -(off[0] / 255f),
                    off[1] / 255f,
                    off[2] / 255f
                );
                resultV = (float3*)((byte*)resultV + outputByteStride);
                off += inputByteStride;
            }
        }
#else
        public void Execute(int i)
        {
            var resultV = (float3*)(((byte*)result) + (i * outputByteStride));
            var off = input + (i * inputByteStride);
            *resultV = new float3(
                -(off[0] / 255f),
                off[1] / 255f,
                off[2] / 255f
            );
        }
#endif
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

        public void Execute(int i)
        {
            var resultV = (uint4*)(((byte*)result) + (i * outputByteStride));
            var off = input + (i * inputByteStride);
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

        public void Execute(int i)
        {
            var resultV = (uint4*)(((byte*)result) + (i * outputByteStride));
            var off = (ushort*)(input + (i * inputByteStride));
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
    unsafe struct ConvertMatricesJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4x4* input;

        [WriteOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4x4* result;

        public void Execute(int i)
        {
            var tmp = input[i].c0;
            tmp.y *= -1;
            tmp.z *= -1;
            result[i].c0 = tmp;

            tmp = input[i].c1;
            tmp.x *= -1;
            result[i].c1 = tmp;

            tmp = input[i].c2;
            tmp.x *= -1;
            result[i].c2 = tmp;

            tmp = input[i].c3;
            tmp.x *= -1;
            result[i].c3 = tmp;
        }
    }

    [BurstCompile]
    unsafe struct ConvertScalarInt8ToFloatNormalizedJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int i)
        {
            result[i] = max(input[i] / 127f, -1.0f);
        }
    }

    [BurstCompile]
    unsafe struct ConvertScalarUInt8ToFloatNormalizedJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int i)
        {
            result[i] = input[i] / 255f;
        }
    }

    [BurstCompile]
    unsafe struct ConvertScalarInt16ToFloatNormalizedJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public short* input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int i)
        {
            result[i] = max(input[i] / (float)short.MaxValue, -1.0f);
        }
    }

    [BurstCompile]
    unsafe struct ConvertScalarUInt16ToFloatNormalizedJob : IJobParallelFor
    {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ushort* input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int i)
        {
            result[i] = input[i] / (float)ushort.MaxValue;
        }
    }
}
