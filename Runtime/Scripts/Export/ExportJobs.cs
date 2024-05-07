// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using GLTFast.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Export
{

    [BurstCompile]
    static class ExportJobs
    {

        [BurstCompile]
        public struct ConvertIndicesFlippedJob<T> : IJobParallelFor where T : struct
        {

            [ReadOnly]
            public NativeArray<T> input;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<T> result;

            public void Execute(int i)
            {
                result[i * 3 + 0] = input[i * 3 + 0];
                result[i * 3 + 1] = input[i * 3 + 2];
                result[i * 3 + 2] = input[i * 3 + 1];
            }
        }

        [BurstCompile]
        public struct ConvertIndicesQuadFlippedJob<T> : IJobParallelFor where T : struct
        {

            [ReadOnly]
            public NativeArray<T> input;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<T> result;

            public void Execute(int i)
            {
                result[i * 6 + 0] = input[i * 4 + 0];
                result[i * 6 + 1] = input[i * 4 + 2];
                result[i * 6 + 2] = input[i * 4 + 1];
                result[i * 6 + 3] = input[i * 4 + 2];
                result[i * 6 + 4] = input[i * 4 + 0];
                result[i * 6 + 5] = input[i * 4 + 3];
            }
        }

        [BurstCompile]
        public unsafe struct ConvertPositionFloatJob : IJobParallelFor
        {

            public uint inputByteStride;
            public uint outputByteStride;

            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;

            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i)
            {
                var inPtr = (float3*)(input + i * inputByteStride);
                var outPtr = (float3*)(output + i * outputByteStride);

                var tmp = *inPtr;
                tmp.x *= -1;
                *outPtr = tmp;
            }
        }

        [BurstCompile]
        public unsafe struct ConvertTangentFloatJob : IJobParallelFor
        {

            public uint inputByteStride;
            public uint outputByteStride;

            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;

            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i)
            {
                var inPtr = (float4*)(input + i * inputByteStride);
                var outPtr = (float4*)(output + i * outputByteStride);

                var tmp = *inPtr;
                tmp.z *= -1;
                *outPtr = tmp;
            }
        }

        [BurstCompile]
        public unsafe struct ConvertTexCoordFloatJob : IJobParallelFor
        {
            public uint inputByteStride;
            public uint outputByteStride;

            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;

            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i)
            {
                var inPtr = (float2*)(input + i * inputByteStride);
                var outPtr = (float2*)(output + i * outputByteStride);

                var tmp = *inPtr;
                tmp.y = 1 - tmp.y;
                *outPtr = tmp;
            }
        }

        [BurstCompile]
        public unsafe struct ConvertGenericJob : IJobParallelFor
        {
            public uint inputByteStride;
            public uint outputByteStride;

            public uint byteLength;

            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;

            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i)
            {
                var inPtr = input + i * inputByteStride;
                var outPtr = output + i * outputByteStride;
                UnsafeUtility.MemCpy(outPtr, inPtr, byteLength);
            }
        }
    }
}
