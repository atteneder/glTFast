// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_2020_2_OR_NEWER
#define GLTFAST_MESH_DATA
#endif


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

#if GLTFAST_MESH_DATA

        [BurstCompile]
        public struct ConvertIndicesFlippedJob<T> : IJobParallelFor where T : struct {

            [ReadOnly]
            public NativeArray<T> input;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<T> result;

            public void Execute(int i) {
                result[i*3+0] = input[i*3+0];
                result[i*3+1] = input[i*3+2];
                result[i*3+2] = input[i*3+1];
            }
        }

        [BurstCompile]
        public struct ConvertIndicesQuadFlippedJob<T> : IJobParallelFor where T : struct {

            [ReadOnly]
            public NativeArray<T> input;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<T> result;

            public void Execute(int i) {
                result[i*6+0] = input[i*4+0];
                result[i*6+1] = input[i*4+2];
                result[i*6+2] = input[i*4+1];
                result[i*6+3] = input[i*4+2];
                result[i*6+4] = input[i*4+0];
                result[i*6+5] = input[i*4+3];
            }
        }

#endif // GLTFAST_MESH_DATA

        [BurstCompile]
        public unsafe struct ConvertPositionFloatJob : IJobParallelFor
        {

            public uint byteStride;

            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;

            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i)
            {
                var inPtr = (float3*)(input + i * byteStride);
                var outPtr = (float3*)(output + i * byteStride);

                var tmp = *inPtr;
                tmp.x *= -1;
                *outPtr = tmp;
            }
        }

        [BurstCompile]
        public unsafe struct ConvertTangentFloatJob : IJobParallelFor
        {

            public uint byteStride;

            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;

            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i)
            {
                var inPtr = (float4*)(input + i * byteStride);
                var outPtr = (float4*)(output + i * byteStride);

                var tmp = *inPtr;
                tmp.z *= -1;
                *outPtr = tmp;
            }
        }

        [BurstCompile]
        public unsafe struct ConvertTexCoordFloatJob : IJobParallelFor
        {
            public uint byteStride;

            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;

            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i)
            {
                var inPtr = (float2*)(input + i * byteStride);
                var outPtr = (float2*)(output + i * byteStride);

                var tmp = *inPtr;
                tmp.y = 1 - tmp.y;
                *outPtr = tmp;
            }
        }
    }
}
