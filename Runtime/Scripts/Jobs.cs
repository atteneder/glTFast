﻿// Copyright 2020-2022 Andreas Atteneder
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

#define USE_MATH_EXT
#define USE_MATHEMATICS

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
using GLTFast.Maths;

[assembly: InternalsVisibleTo("glTF-test-framework.Tests")]

namespace GLTFast.Jobs {
    
    using Schema;

    [BurstCompile]
    static unsafe class CachedFunction {
    
        public delegate int GetIndexDelegate(void* baseAddress, int index);
        public delegate void GetFloat3Delegate(float3* destination, void* src);
        
        // Cached function pointers
        static FunctionPointer<GetIndexDelegate> GetIndexValueInt8Method;
        static FunctionPointer<GetIndexDelegate> GetIndexValueUInt8Method;
        static FunctionPointer<GetIndexDelegate> GetIndexValueInt16Method;
        static FunctionPointer<GetIndexDelegate> GetIndexValueUInt16Method;
        static FunctionPointer<GetIndexDelegate> GetIndexValueUInt32Method;
        
        
        static FunctionPointer<GetFloat3Delegate> GetFloat3FloatMethod;
        static FunctionPointer<GetFloat3Delegate> GetFloat3Int8Method;
        static FunctionPointer<GetFloat3Delegate> GetFloat3UInt8Method;
        static FunctionPointer<GetFloat3Delegate> GetFloat3Int16Method;
        static FunctionPointer<GetFloat3Delegate> GetFloat3UInt16Method;
        static FunctionPointer<GetFloat3Delegate> GetFloat3UInt32Method;
        static FunctionPointer<GetFloat3Delegate> GetFloat3Int8NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> GetFloat3UInt8NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> GetFloat3Int16NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> GetFloat3UInt16NormalizedMethod;
        static FunctionPointer<GetFloat3Delegate> GetFloat3UInt32NormalizedMethod;
        
        /// <summary>
        /// Returns Burst compatible function that retrieves an index value
        /// </summary>
        /// <param name="format">Data type of index</param>
        /// <returns>Burst Function Pointer to correct conversion function</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static FunctionPointer<GetIndexDelegate> GetIndexConverter(GLTFComponentType format) {
            switch (format) {
                case GLTFComponentType.UnsignedByte:
                    if (!GetIndexValueUInt8Method.IsCreated) {
                        GetIndexValueUInt8Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt8);
                    }
                    return GetIndexValueUInt8Method;
                case GLTFComponentType.Byte:
                    if (!GetIndexValueInt8Method.IsCreated) {
                        GetIndexValueInt8Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueInt8);
                    }
                    return GetIndexValueInt8Method;
                case GLTFComponentType.UnsignedShort:
                    if (!GetIndexValueUInt16Method.IsCreated) {
                        GetIndexValueUInt16Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt16);
                    }
                    return GetIndexValueUInt16Method;
                case GLTFComponentType.Short:
                    if (!GetIndexValueInt16Method.IsCreated) {
                        GetIndexValueInt16Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueInt16);
                    }
                    return GetIndexValueInt16Method;
                case GLTFComponentType.UnsignedInt:
                    if (!GetIndexValueUInt32Method.IsCreated) {
                        GetIndexValueUInt32Method = BurstCompiler.CompileFunctionPointer<GetIndexDelegate>(GetIndexValueUInt32);
                    }
                    return GetIndexValueUInt32Method;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
        
        public static FunctionPointer<GetFloat3Delegate> GetPositionConverter(
            GLTFComponentType format,
            bool normalized
            )
        {
            if (normalized) {
                switch (format) {
                    case GLTFComponentType.Float:
                        // Floats cannot be normalized.
                        // Fall back to non-normalized below
                        break;
                    case GLTFComponentType.Byte:
                        if (!GetFloat3Int8NormalizedMethod.IsCreated) {
                            GetFloat3Int8NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int8Normalized);
                        }
                        return GetFloat3Int8NormalizedMethod;
                    case GLTFComponentType.UnsignedByte:
                        if (!GetFloat3UInt8NormalizedMethod.IsCreated) {
                            GetFloat3UInt8NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt8Normalized);
                        }
                        return GetFloat3UInt8NormalizedMethod;
                    case GLTFComponentType.Short:
                        if (!GetFloat3Int16NormalizedMethod.IsCreated) {
                            GetFloat3Int16NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int16Normalized);
                        }
                        return GetFloat3Int16NormalizedMethod;
                    case GLTFComponentType.UnsignedShort:
                        if (!GetFloat3UInt16NormalizedMethod.IsCreated) {
                            GetFloat3UInt16NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt16Normalized);
                        }
                        return GetFloat3UInt16NormalizedMethod;
                    case GLTFComponentType.UnsignedInt:
                        if (!GetFloat3UInt32NormalizedMethod.IsCreated) {
                            GetFloat3UInt32NormalizedMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt32Normalized);
                        }
                        return GetFloat3UInt32NormalizedMethod;
                }
            }
            switch (format) {
                case GLTFComponentType.Float:
                    if (!GetFloat3FloatMethod.IsCreated) {
                        GetFloat3FloatMethod = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Float);
                    }
                    return GetFloat3FloatMethod;
                case GLTFComponentType.Byte:
                    if (!GetFloat3Int8Method.IsCreated) {
                        GetFloat3Int8Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int8);
                    }
                    return GetFloat3Int8Method;
                case GLTFComponentType.UnsignedByte:
                    if (!GetFloat3UInt8Method.IsCreated) {
                        GetFloat3UInt8Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt8);
                    }
                    return GetFloat3UInt8Method;
                case GLTFComponentType.Short:
                    if (!GetFloat3Int16Method.IsCreated) {
                        GetFloat3Int16Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3Int16);
                    }
                    return GetFloat3Int16Method;
                case GLTFComponentType.UnsignedShort:
                    if (!GetFloat3UInt16Method.IsCreated) {
                        GetFloat3UInt16Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt16);
                    }
                    return GetFloat3UInt16Method;
                case GLTFComponentType.UnsignedInt:
                    if (!GetFloat3UInt32Method.IsCreated) {
                        GetFloat3UInt32Method = BurstCompiler.CompileFunctionPointer<GetFloat3Delegate>(GetFloat3UInt32);
                    }
                    return GetFloat3UInt32Method;
            }
            throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        [BurstCompile,MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt8(void* baseAddress, int index) {
            return *((byte*)baseAddress+index);
        }
            
        [BurstCompile,MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueInt8(void* baseAddress, int index) {
            return *(((sbyte*)baseAddress)+index);
        }
            
        [BurstCompile,MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt16(void* baseAddress, int index) {
            return *(((ushort*)baseAddress)+index);
        }
            
        [BurstCompile,MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueInt16(void* baseAddress, int index) {
            return *(((short*)baseAddress)+index);
        }
            
        [BurstCompile,MonoPInvokeCallback(typeof(GetIndexDelegate))]
        static int GetIndexValueUInt32(void* baseAddress, int index) {
            return (int) *(((uint*)baseAddress)+index);
        }

        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Float(float3* destination, void* src) {
            destination->x = -*(float*)src;
            destination->y = *((float*)src+1);
            destination->z = *((float*)src+2);
        }

        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int8(float3* destination, void* src) {
            destination->x = -*(sbyte*)src;
            destination->y = *((sbyte*)src+1);
            destination->z = *((sbyte*)src+2);
        }
        
        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt8(float3* destination, void* src) {
            destination->x = -*(byte*)src;
            destination->y = *((byte*)src+1);
            destination->z = *((byte*)src+2);
        }
        
        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int16(float3* destination, void* src) {
            destination->x = -*(short*)src;
            destination->y = *((short*)src+1);
            destination->z = *((short*)src+2);
        }
        
        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt16(float3* destination, void* src) {
            destination->x = -*(ushort*)src;
            destination->y = *((ushort*)src+1);
            destination->z = *((ushort*)src+2);
        }
        
        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt32(float3* destination, void* src) {
            destination->x = -*(uint*)src;
            destination->y = *((uint*)src+1);
            destination->z = *((uint*)src+2);
        }
        
        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int8Normalized(float3* destination, void* src) {
            destination->x = -max(*(sbyte*)src/127f,-1);
            destination->y = max(*((sbyte*)src+1)/127f,-1);
            destination->z = max(*((sbyte*)src+2)/127f,-1);
        }
        
        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt8Normalized(float3* destination, void* src) {
            destination->x = -*(byte*)src / 255f;
            destination->y = *((byte*)src+1) / 255f;
            destination->z = *((byte*)src+2) / 255f;
        }
        
        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3Int16Normalized(float3* destination, void* src) {
            destination->x = -max( *(short*)src / (float) short.MaxValue, -1f);
            destination->y = max( *((short*)src+1) / (float) short.MaxValue, -1f);
            destination->z = max( *((short*)src+2) / (float) short.MaxValue, -1f);
        }
        
        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt16Normalized(float3* destination, void* src) {
            destination->x = -*(ushort*)src / (float) ushort.MaxValue;
            destination->y = *((ushort*)src+1) / (float) ushort.MaxValue;
            destination->z = *((ushort*)src+2) / (float) ushort.MaxValue;
        }
        
        [BurstCompile,MonoPInvokeCallback(typeof(GetFloat3Delegate))]
        static void GetFloat3UInt32Normalized(float3* destination, void* src) {
            destination->x = -*(uint*)src / (float) uint.MaxValue;
            destination->y = *((uint*)src+1) / (float) uint.MaxValue;
            destination->z = *((uint*)src+2) / (float) uint.MaxValue;
        }
    }
    
    [BurstCompile]
    public unsafe struct CreateIndicesInt32Job : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;
        
        public void Execute(int i)
        {
            result[i] = i;
        }
    }

    [BurstCompile]
    public unsafe struct CreateIndicesInt32FlippedJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int i)
        {
            result[i] = i - 2*(i%3-1);
        }
    }
    
    [BurstCompile]
    public unsafe struct ConvertIndicesUInt8ToInt32Job : IJobParallelFor  {

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
    public unsafe struct ConvertIndicesUInt8ToInt32FlippedJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int3* result;

        public void Execute(int i) {
            result[i] = new int3(
                input[i*3],
                input[i*3+2],
                input[i*3+1]
                );
        }
    }

    [BurstCompile]
    public unsafe struct ConvertIndicesUInt16ToInt32FlippedJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ushort* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public int3* result;

        public void Execute(int i) {
            result[i] = new int3(
                input[i * 3],
                input[i * 3 + 2],
                input[i * 3 + 1]
            );
        }
    }

    [BurstCompile]
    public unsafe struct ConvertIndicesUInt16ToInt32Job : IJobParallelFor  {

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
    public unsafe struct ConvertIndicesUInt32ToInt32Job : IJobParallelFor  {

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
    public unsafe struct ConvertIndicesUInt32ToInt32FlippedJob : IJobParallelFor  {

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
    // public unsafe struct ConvertUVsUInt8ToFloatJob : IJobParallelFor  {
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
    // public unsafe struct ConvertUVsUInt8ToFloatNormalizedJob : IJobParallelFor  {
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
    // public unsafe struct ConvertUVsUInt16ToFloatNormalizedJob : IJobParallelFor  {
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
    // public unsafe struct ConvertUVsUInt16ToFloatJob : IJobParallelFor  {
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
    // public unsafe struct ConvertUVsFloatToFloatJob : IJobParallelFor {
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
    public unsafe struct ConvertUVsUInt8ToFloatInterleavedJob : 
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
        public void Execute(int i) {
            var resultV = (float2*) (((byte*)result) + (i*outputByteStride));
            var off = input + inputByteStride*i;
            *resultV = new float2(off[0], 1 - off[1]);
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertUVsUInt8ToFloatInterleavedNormalizedJob : IJobParallelFor  {

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
            var resultV = (float2*) (((byte*)result) + (i*outputByteStride));
            var off = input + inputByteStride*i;
            var tmp = new float2(
                off[0],
                off[1] 
                ) / 255f;
            tmp.y = 1-tmp.y;
            *resultV = tmp;
        }
    }

    [BurstCompile]
    public unsafe struct ConvertUVsUInt16ToFloatInterleavedJob : 
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
            var resultV = (float2*) (((byte*)result) + (i*outputByteStride));
            var uv = (ushort*) (input + inputByteStride*i);
            *resultV = new float2 (uv[0], 1 - uv[1] );
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertUVsUInt16ToFloatInterleavedNormalizedJob : IJobParallelFor  {

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

        public void Execute(int i) {
            var resultV = (float2*) (((byte*)result) + (i*outputByteStride));
            var uv = (ushort*) (input + inputByteStride*i);
            var tmp = new float2( 
                uv[0],
                uv[1]
            ) / ushort.MaxValue;
            tmp.y = 1 - tmp.y;
            *resultV = tmp;
        }
    }

    [BurstCompile]
    public unsafe struct ConvertUVsInt16ToFloatInterleavedJob : 
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
            var uv = (short*) (input + i*inputByteStride);
            
            for (var x = 0; x < count; x++) {
                *resultV = new float2(uv[0],1 - uv[1]);
                resultV = (float2*)((byte*)resultV + outputByteStride);
                uv = (short*)((byte*)uv + inputByteStride);
            }
        }
#else
        public void Execute(int i) {
            var resultV = (float2*) (((byte*)result) + (i*outputByteStride));
            var uv = (short*) ((byte*)input + inputByteStride*i);
            *resultV = new float2(uv[0],1 - uv[1]);
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertUVsInt16ToFloatInterleavedNormalizedJob : 
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
            var uv = (short*) (input + i*inputByteStride);
            
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
        public void Execute(int i) {
            var resultV = (float2*) (((byte*)result) + (i*outputByteStride));
            var uv = (short*) ((byte*)input + inputByteStride*i);

            var tmp = new float2(uv[0], uv[1]) / short.MaxValue;
            var tmp2 = max(tmp, -1f);
            tmp2.y = 1 - tmp2.y;
            *resultV = tmp2;
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertUVsInt8ToFloatInterleavedJob : 
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
            var resultV = (float2*) (((byte*)result) + (i*outputByteStride));
            var off = input + inputByteStride*i;
            *resultV = new float2(off[0],1 - off[1]);
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertUVsInt8ToFloatInterleavedNormalizedJob : 
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
        public void Execute(int i) {
            var resultV = (float2*) (((byte*)result) + (i*outputByteStride));
            var off = input + inputByteStride*i;
            var tmp = new float2(off[0],off[1]) / 127f;
            var tmp2 = max(tmp, -1f);
            tmp2.y = 1-tmp2.y; 
            *resultV = tmp2;
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertColorsRGBFloatToRGBAFloatJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

        public void Execute(int i) {
            var src = (float3*) (input + (i * inputByteStride));
            result[i] = new float4(*src, 1f);
        }
    }

    [BurstCompile]
    public unsafe struct ConvertColorsRGBUInt8ToRGBAFloatJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int i) {
            var src = input + (i * inputByteStride);
            result[i] = new float4 (
                new float3(src[0],src[1],src[2]) / byte.MaxValue,
                1f
            );
        }
    }

    [BurstCompile]
    public unsafe struct ConvertColorsRGBUInt16ToRGBAFloatJob : IJobParallelFor {

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
                new float3(src[0],src[1],src[2]) / ushort.MaxValue,
                1f
            );
        }
    }

    [BurstCompile]
    public unsafe struct ConvertColorsRGBAUInt16ToRGBAFloatJob : 
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
            result[i] = new float4 (
                src[0] / (float)ushort.MaxValue,
                src[1] / (float)ushort.MaxValue,
                src[2] / (float)ushort.MaxValue,
                src[3] / (float)ushort.MaxValue
            );
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertColorsRGBAFloatToRGBAFloatJob : 
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
        public void Execute(int i) {
            var src = (float4*) (input + (i * inputByteStride));
            result[i] = *src;
        }
#endif
    }
    
    [BurstCompile]
    public unsafe struct ConvertColorsRGBAUInt8ToRGBAFloatJob : IJobParallelFor {

        [ReadOnly]
        public int inputByteStride;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float4> result;

        public void Execute(int i) {
            var src = input + (i * inputByteStride);
            result[i] = new float4 (
                src[0] / (float) byte.MaxValue,
                src[1] / (float) byte.MaxValue,
                src[2] / (float) byte.MaxValue,
                src[3] / (float) byte.MaxValue
            );
        }
    }

    [BurstCompile]
    public unsafe struct MemCopyJob : IJob {

        [ReadOnly]
        public long bufferSize;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public void* result;

        public void Execute() {
            UnsafeUtility.MemCpy(result,input,bufferSize);
        }
    }

    /// <summary>
    /// General purpose vector 3 (position or normal) conversion
    /// </summary>
    [BurstCompile]
    public unsafe struct ConvertVector3FloatToFloatJob : IJobParallelFor {
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float3* result;

        public void Execute(int i) 
        {
#if USE_MATH_EXT
#if USE_MATHEMATICS
            var tmp = *input;
            *result = tmp.ToUnity();

#else
            var tmp = *input;
            Vector3 gltfVec3 = tmp;
            Vector3 unityVec3 = gltfVec3.ToUnity();
            *result = unityVec3;
#endif
#else
            var tmp = input[i];
            tmp.x *= -1;
            result[i] = tmp;
#endif
        }
    }

    [BurstCompile]
    public unsafe struct ConvertRotationsFloatToFloatJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4* result;

        public void Execute(int i) {
#if USE_MATH_EXT
#if USE_MATHEMATICS
            quaternion q = *(quaternion*)input;
            q = q.ToUnity();
            result[i].x = q.value.x;
            result[i].y = q.value.y;
            result[i].z = q.value.z;
            result[i].w = q.value.w;
#else
            Quaternion q = new Quaternion(input[i].x,input[i].y,input[i].z,input[i].w);
            q = q.ToUnity();
            result[i].x = q.x;
            result[i].y = q.y;
            result[i].z = q.z;
            result[i].w = q.w;
#endif
#else
            var tmp = input[i];
            tmp.y *= -1;
            tmp.z *= -1;
            result[i] = tmp;
#endif
        }
    }
    
    [BurstCompile]
    public unsafe struct ConvertRotationsInt16ToFloatJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public short* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* result;

        public void Execute(int i) {
#if USE_MATH_EXT
#if USE_MATHEMATICS
            quaternion q = new quaternion(Mathf.Max(input[i * 4] / short.MaxValue, -1f),
                Mathf.Max(input[i * 4 + 1] / short.MaxValue, -1f),
                Mathf.Max(input[i * 4 + 2] / short.MaxValue, -1f),
                Mathf.Max(input[i * 4 + 3] / short.MaxValue, -1f));
            q = q.ToUnity();
            result[i * 4] = q.value.x;
            result[i * 4 + 1] = q.value.y;
            result[i * 4 + 2] = q.value.z;
            result[i * 4 + 3] = q.value.w;

#else
            Quaternion q = new Quaternion(Mathf.Max(input[i * 4] / short.MaxValue, -1f),
                Mathf.Max(input[i * 4 + 1] / short.MaxValue, -1f),
                Mathf.Max(input[i * 4 + 2] / short.MaxValue, -1f),
                Mathf.Max(input[i * 4 + 3] / short.MaxValue, -1f));
            q = q.ToUnity();
            result[i * 4] = q.x;
            result[i * 4 + 1] = q.y;
            result[i * 4 + 2] = q.z;
            result[i * 4 + 3] = q.w;
#endif
#else
            result[i*4] = Mathf.Max( input[i*4] / (float) short.MaxValue, -1f );
            result[i*4+1] = -Mathf.Max( input[i*4+1] / (float) short.MaxValue, -1f );
            result[i*4+2] = -Mathf.Max( input[i*4+2] / (float) short.MaxValue, -1f );
            result[i*4+3] = Mathf.Max( input[i*4+3] / (float) short.MaxValue, -1f );
#endif
        }
    }

    /// <summary>
    /// Converts an array of glTF space quaternions (normalized, signed bytes) to
    /// Quaternions in Unity space (floats). 
    /// </summary>
    [BurstCompile]
    public unsafe struct ConvertRotationsInt8ToFloatJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float* result;

        public void Execute(int i) {
#if USE_MATH_EXT
#if USE_MATHEMATICS
            quaternion q = new quaternion(Mathf.Max(input[i * 4] / 127f, -1f),
                Mathf.Max(input[i * 4 + 1] / 127f, -1f),
                Mathf.Max(input[i * 4 + 2] / 127f, -1f),
                Mathf.Max(input[i * 4 + 3] / 127f, -1f));
            q = q.ToUnity();
            result[i * 4] = q.value.x;
            result[i * 4 + 1] = q.value.y;
            result[i * 4 + 2] = q.value.z;
            result[i * 4 + 3] = q.value.w;
#else
            Quaternion q = new Quaternion(Mathf.Max(input[i * 4] / 127f, -1f),
                Mathf.Max(input[i * 4+1] / 127f, -1f),
                Mathf.Max(input[i * 4+2] / 127f, -1f),
                Mathf.Max(input[i * 4+3] / 127f, -1f));
            q = q.ToUnity();
            result[i*4] = q.x;
            result[i*4+1] = q.y;
            result[i*4+2] = q.z;
            result[i*4+3] = q.w;
#endif
#else
            result[i*4] = Mathf.Max( input[i*4] / 127f, -1f );
            result[i*4+1] = -Mathf.Max( input[i*4+1] / 127f, -1f );
            result[i*4+2] = -Mathf.Max( input[i*4+2] / 127f, -1f );
            result[i*4+3] = Mathf.Max( input[i*4+3] / 127f, -1f );
#endif
        }
    }

    [BurstCompile]
    public unsafe struct ConvertUVsFloatToFloatInterleavedJob : 
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
        public void Execute(int i) {
            var resultV = (float2*) (((byte*)result) + (i*outputByteStride));
            var off = (float2*) (input + (i*inputByteStride));
            var tmp = *off;
            tmp.y = 1 - tmp.y;
            *resultV = tmp;
        }
#endif
    }

    /// <summary>
    /// General purpose vector 3 (position or normal) conversion
    /// </summary>
    //[BurstCompile]
    public unsafe struct ConvertVector3FloatToFloatInterleavedJob :
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
        public void Execute(int i) {
            var resultV = (float3*) (((byte*)result) + (i*outputByteStride));
            var off = (float3*) (input + i*inputByteStride);

#if USE_MATH_EXT
#if USE_MATHEMATICS
            var tmp = *off;
            *resultV = tmp.ToUnity();
#else
            var tmp = *off;
            Vector3 gltfVec3 = tmp;
            Vector3 unityVec3 = gltfVec3.ToUnity();
            *resultV = unityVec3;
#endif
#else
            var tmp = *off;
            tmp.x *= -1;
            *resultV = tmp;
#endif
        }
#endif
    }

    /// <summary>
    /// General purpose sparse vector 3 (position or normal) conversion
    /// </summary>
    [BurstCompile]
    unsafe struct ConvertVector3SparseJob : IJobParallelFor {

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

        public void Execute(int i) {
            var index = indexConverter.Invoke(indexBuffer,i);
            var resultV = (float3*) (((byte*)result) + (index*outputByteStride));
            valueConverter.Invoke(resultV, (byte*)input + i*inputByteStride);
        }
    }

    [BurstCompile]
    public unsafe struct ConvertTangentsFloatToFloatInterleavedJob : 
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
        public void Execute(int i) {
            float4* resultV = (float4*) (((byte*)result) + (i*outputByteStride));
            byte* off = input + (i*inputByteStride);
            var tmp = *((float4*)off);
#if USE_MATH_EXT
#if USE_MATHEMATICS
            *resultV = tmp.ToUnityVector();
#else
            Vector4 n = tmp;
            n = n.ToUnityVector();
            *resultV = n;
#endif
#else
            tmp.z *= -1;
            *resultV = tmp;
#endif
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertBoneWeightsFloatToFloatInterleavedJob : 
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
        public void Execute(int i) {
            var resultV = (float4*) (((byte*)result) + (i*outputByteStride));
            var off = input + (i*inputByteStride);
            *resultV = *((float4*)off);
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertBoneWeightsUInt8ToFloatInterleavedJob : 
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
        public void Execute(int i) {
            var resultV = (float4*) (((byte*)result) + (i*outputByteStride));
            var off = input + (i*inputByteStride);
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
    public unsafe struct ConvertBoneWeightsUInt16ToFloatInterleavedJob : 
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
            var off = (ushort*) input + i*inputByteStride;
            
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
        public void Execute(int i) {
            var resultV = (float4*) (((byte*)result) + (i*outputByteStride));
            var off = (ushort*) input + i*inputByteStride;
            *resultV = new float4(
                off[0] / (float) ushort.MaxValue,
                off[1] / (float) ushort.MaxValue,
                off[2] / (float) ushort.MaxValue,
                off[3] / (float) ushort.MaxValue
                );
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertTangentsInt16ToFloatInterleavedNormalizedJob : 
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
            var off = (short*) (input + i*inputByteStride);
            
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
        public void Execute(int i) {
            var resultV = (float4*) (((byte*)result) + (i*outputByteStride));
            var off = (short*) (((byte*)input) + (i*inputByteStride));
            var tmp = new float4(off[0],off[1],off[2],off[3]) / short.MaxValue;
            var tmp2 = max(tmp, -1f);
#if USE_MATH_EXT
#if USE_MATHEMATICS
            *resultV = tmp2.ToUnityVector();
#else
            Vector4 n = tmp2;
            n = n.ToUnityVector();
            n.Normalize();
            *resultV = n;
#endif
#else
            tmp2.z *= -1;
            *resultV = normalize(tmp2);
#endif
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertTangentsInt8ToFloatInterleavedNormalizedJob : 
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
        public void Execute(int i) {
            var resultV = (float4*) (((byte*)result) + (i*outputByteStride));
            var off = input + (i*inputByteStride);
            var tmp = new float4(off[0],off[1],off[2],off[3]) / 127f;
            var tmp2 = max(tmp, -1f);
#if USE_MATH_EXT
#if USE_MATHEMATICS
            *resultV = tmp2.ToUnityVector();
#else
            Vector4 n = tmp2;
            n = n.ToUnityVector();
            n.Normalize();
            *resultV = n;
#endif
#else
            tmp2.z *= -1;
            *resultV = normalize(tmp2);
#endif
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertPositionsUInt16ToFloatInterleavedJob : 
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
        public void Execute(int i) {
            var resultV = (float3*) (((byte*)result) + (i*outputByteStride));
            var off = (ushort*) (input + (inputByteStride*i));
#if USE_MATH_EXT
#if USE_MATHEMATICS
            float3 p = new float3(off[0], off[1], off[2]);
            *resultV = p.ToUnity();
#else
            Vector3 p = new Vector3(off[0],off[1],off[2]);
            p = p.ToUnity();
            *resultV = p;
#endif
#else
            *resultV = new float3(-(float)off[0], off[1], off[2]);
#endif
        }
#endif
        }

    [BurstCompile]
    public unsafe struct ConvertPositionsUInt16ToFloatInterleavedNormalizedJob : 
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
        public void Execute(int i) {
            var resultV = (float3*) (((byte*)result) + (i*outputByteStride));
            var off = (ushort*) (input + (inputByteStride*i));
#if USE_MATH_EXT
#if USE_MATHEMATICS
            float3 p = new float3(off[0], off[1], off[2]);
            p = p / (float)ushort.MaxValue;
            *resultV = p.ToUnity();
#else
            Vector3 p = new Vector3(off[0],off[1],off[2]);
            p = p / (float)ushort.MaxValue;
            p = p.ToUnity();
            *resultV = p;
#endif
#else

            *resultV = new float3(
                -(off[0] / (float) ushort.MaxValue),
                off[1] / (float) ushort.MaxValue,
                off[2] / (float) ushort.MaxValue
            );
#endif
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertPositionsInt16ToFloatInterleavedJob : 
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
        public void Execute(int i) {
            var resultV = (float3*)  (((byte*)result) + (i*outputByteStride));
            var off = (short*) (input + (i*inputByteStride));
#if USE_MATH_EXT
#if USE_MATHEMATICS
            float3 p = new float3(off[0], off[1], off[2]);
            *resultV = p.ToUnity();
#else
            Vector3 p = new Vector3(off[0],off[1],off[2]);
            p = p.ToUnity();
            *resultV = p;
#endif
#else
            *resultV = new float3(-(float)off[0],off[1],off[2]);
#endif
        }
#endif
    }

    /// <summary>
    /// General purpose (position / morph target delta normal)
    /// Result is not normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    public unsafe struct ConvertVector3Int16ToFloatInterleavedNormalizedJob : 
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
        public void Execute(int i) {
            var resultV = (float3*) (((byte*)result) + (i*outputByteStride));
            var off = (short*) (input + (i*inputByteStride));

            var tmp = new float3(off[0], off[1], off[2]) / short.MaxValue;
            var tmp2 = max(tmp, -1f);
#if USE_MATH_EXT
#if USE_MATHEMATICS
            *resultV = tmp.ToUnityVector();
#else
            Vector3 n = tmp2;
            n = n.ToUnityVector();
            *resultV = n;
#endif
#else
            tmp2.x *= -1;
            *resultV = tmp2;
#endif
        }
#endif
    }

    /// <summary>
    /// Normal conversion
    /// Result is normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    public unsafe struct ConvertNormalsInt16ToFloatInterleavedNormalizedJob : 
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
        public void Execute(int i) {
            var resultV = (float3*) (((byte*)result) + (i*outputByteStride));
            var off = (short*) (input + (i*inputByteStride));

            var tmp = new float3(off[0], off[1], off[2]) / short.MaxValue;
            var tmp2 = max(tmp, -1f);
#if USE_MATH_EXT
#if USE_MATHEMATICS
            *resultV = tmp2.ToUnityVector();
#else
            Vector3 n = tmp2;
            n = n.ToUnityVector();
            n.Normalize();
            *resultV = n;
#endif
#else
            tmp2.x *= -1;
            *resultV = normalize(tmp2);
#endif
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertPositionsInt8ToFloatInterleavedJob : 
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
        public void Execute(int i) {
            var resultV = (float3*) (((byte*)result) + (i*outputByteStride));
            var off = input + (inputByteStride*i);
#if USE_MATH_EXT
#if USE_MATHEMATICS
            float3 p = new float3(off[0], off[1], off[2]);
            *resultV = p.ToUnity();
#else
            Vector3 p = new Vector3(off[0],off[1],off[2]);
            p = p.ToUnity();
            *resultV = p;
#endif
#else
            *resultV = new float3(-(float)off[0], off[1], off[2]);
#endif
        }
#endif
        }

    /// <summary>
    /// General purpose conversion (positions or morph target delta normals)
    /// Result is not normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    public unsafe struct ConvertVector3Int8ToFloatInterleavedNormalizedJob : 
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
        public void Execute(int i) {
            var resultV = (float3*) (((byte*)result) + (i*outputByteStride));
            var off = input + (inputByteStride*i);

            var tmp = new float3(off[0], off[1], off[2]) / 127f;
            var tmp2 = max(tmp, -1);
#if USE_MATH_EXT
#if USE_MATHEMATICS
            *resultV = tmp2.ToUnityVector();
#else
            Vector3 n = tmp2;
            n = n.ToUnityVector();
            n.Normalize();
            *resultV = n;
#endif
#else
            tmp2.x *= -1;
            *resultV = tmp2;
#endif
        }
#endif
    }

    /// <summary>
    /// Normal conversion
    /// Result is normalized (scaled to unit length)
    /// </summary>
    [BurstCompile]
    public unsafe struct ConvertNormalsInt8ToFloatInterleavedNormalizedJob : 
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
        public void Execute(int i) {
            var resultV = (float3*) (((byte*)result) + (i*outputByteStride));
            var off = input + (inputByteStride*i);

            var tmp = new float3(off[0], off[1], off[2]) / 127f;
            var tmp2 = max(tmp, -1);
#if USE_MATH_EXT
#if USE_MATHEMATICS
            *resultV = tmp2.ToUnityVector();
#else
            Vector3 n = tmp2;
            n = n.ToUnityVector();
            n.Normalize();
            *resultV = n;
#endif
#else
            tmp2.x *= -1;
            *resultV = normalize(tmp2);
#endif
        }
#endif
    }

    [BurstCompile]
    public unsafe struct ConvertPositionsUInt8ToFloatInterleavedJob : 
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
        public void Execute(int i) {
            var off = input + (i*inputByteStride);
            var resultV = (float3*) (((byte*) result) + (i*outputByteStride));
#if USE_MATH_EXT
#if USE_MATHEMATICS
            float3 v = new float3(off[0], off[1], off[2]);
            *resultV = v.ToUnity();
#else
            Vector3 v = new Vector3(off[0],off[1],off[2]);
            *resultV = v.ToUnity();
#endif
#else
            *resultV = new float3(-(float)off[0],off[1],off[2]);
#endif
        }
#endif
        }

    [BurstCompile]
    public unsafe struct ConvertPositionsUInt8ToFloatInterleavedNormalizedJob : 
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
        public void Execute(int i) {
            var resultV = (float3*) (((byte*)result) + (i*outputByteStride));
            var off = input + (i*inputByteStride);
            *resultV = new float3(
                -(off[0] / 255f),
                off[1] / 255f,
                off[2] / 255f
            );
        }
#endif
    }
    
    [BurstCompile]
    public unsafe struct ConvertBoneJointsUInt8ToUInt32Job : IJobParallelFor  {

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
            var resultV = (uint4*) (((byte*)result) + (i*outputByteStride));
            var off = input + (i*inputByteStride);
            *resultV = new uint4(off[0],off[1],off[2],off[3]);
        }
    }

    [BurstCompile]
    public unsafe struct ConvertBoneJointsUInt16ToUInt32Job : IJobParallelFor  {

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
            var resultV = (uint4*) (((byte*)result) + (i*outputByteStride));
            var off = (ushort*) (input + (i*inputByteStride));
            *resultV = new uint4(off[0],off[1],off[2],off[3]);
        }
    }
    
    [BurstCompile]
    struct SortJointsByWeightsJob : IJobParallelFor {

        public NativeArray<VBones> bones;
        
        /// <summary>
        /// Number of skin weights that are taken into account (project quality setting)
        /// </summary>
        public int skinWeights;


        public unsafe void Execute(int index) {
            var v = bones[index];

            // Most joints/weights are already sorted by weight
            // Detect and early return if true
            var asc = true;
            for (var i = 0; i < 3; i++) {
                var a = v.weights[i];
                var b = v.weights[i + 1];
                if (a < b) {
                    asc = false;
                    break;
                }
            }
            if (asc) return;

            // Sort otherwise
            for (var i = 0; i < skinWeights; i++) {
                var max = v.weights[i];
                var maxI = i;

                for (var j = i+1; j < 4; j++) {
                    var value = v.weights[j];
                    if (v.weights[j] > max) {
                        max = value;
                        maxI = j;
                    }
                }

                if (maxI > i) {
                    Swap(ref v, maxI, i);
                }
            }

            bones[index] = v;
        }

        static unsafe void Swap(ref VBones v, int a, int b) {
            (v.weights[a], v.weights[b]) = (v.weights[b], v.weights[a]);
            (v.joints[a], v.joints[b]) = (v.joints[b], v.joints[a]);
        }
    }

    [BurstCompile]
    public unsafe struct ConvertMatricesJob : IJobParallelFor  {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4x4* input;

        [WriteOnly]
        [NativeDisableUnsafePtrRestriction]
        public float4x4* result;

        public void Execute(int i) {
#if USE_MATH_EXT
#if USE_MATHEMATICS
            result[i] = input[i].ToUnity();
#else
            Matrix4x4 gltf = input[i];
            result[i] = gltf.ToUnity();
#endif
#else
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
#endif
        }
    }

    [BurstCompile]
    public unsafe struct ConvertScalarInt8ToFloatNormalizedJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public sbyte* input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int i) {
            result[i] = max( input[i] / 127f, -1.0f);
        }
    }

    [BurstCompile]
    public unsafe struct ConvertScalarUInt8ToFloatNormalizedJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public byte* input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int i) {
            result[i] = input[i]/255f;
        }
    }
    
    [BurstCompile]
    public unsafe struct ConvertScalarInt16ToFloatNormalizedJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public short* input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int i) {
            result[i] = max( input[i] / (float) short.MaxValue, -1.0f);
        }
    }

    [BurstCompile]
    public unsafe struct ConvertScalarUInt16ToFloatNormalizedJob : IJobParallelFor {

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ushort* input;

        [WriteOnly]
        public NativeArray<float> result;

        public void Execute(int i) {
            result[i] = input[i] / (float) ushort.MaxValue;
        }
    }
}
