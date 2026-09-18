// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Unity.Cloud.Gltfast.Export
{

    [BurstCompile]
    static class ExportJobs
    {

        [BurstCompile]
        public struct ConvertIndicesFlippedJobUInt16 : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<ushort> input;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<ushort> result;

            public int indexStart;
            public ushort baseVertexOffset;

            public void Execute(int i)
            {
                result[i * 3 + 0] = (ushort)(input[i * 3 + 0] + baseVertexOffset);
                result[i * 3 + 1] = (ushort)(input[i * 3 + 2] + baseVertexOffset);
                result[i * 3 + 2] = (ushort)(input[i * 3 + 1] + baseVertexOffset);
            }
        }


        [BurstCompile]
        public struct ConvertIndicesFlippedJobUInt32 : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<uint> input;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<uint> result;

            public uint baseVertexOffset;

            public void Execute(int index)
            {
                result[index * 3 + 0] = input[index * 3 + 0] + baseVertexOffset;
                result[index * 3 + 1] = input[index * 3 + 2] + baseVertexOffset;
                result[index * 3 + 2] = input[index * 3 + 1] + baseVertexOffset;
            }
        }

        [BurstCompile]
        public struct ConvertIndicesQuadFlippedJobUInt16 : IJobParallelFor
        {

            [ReadOnly]
            public NativeArray<ushort> input;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<ushort> result;

            public ushort baseVertexOffset;

            public void Execute(int i)
            {
                result[i * 6 + 0] = (ushort)(input[i * 4 + 0] + baseVertexOffset);
                result[i * 6 + 1] = (ushort)(input[i * 4 + 2] + baseVertexOffset);
                result[i * 6 + 2] = (ushort)(input[i * 4 + 1] + baseVertexOffset);
                result[i * 6 + 3] = (ushort)(input[i * 4 + 2] + baseVertexOffset);
                result[i * 6 + 4] = (ushort)(input[i * 4 + 0] + baseVertexOffset);
                result[i * 6 + 5] = (ushort)(input[i * 4 + 3] + baseVertexOffset);
            }
        }

        [BurstCompile]
        public struct ConvertIndicesQuadFlippedJobUInt32 : IJobParallelFor
        {

            [ReadOnly]
            public NativeArray<uint> input;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<uint> result;

            public uint baseVertexOffset;

            public void Execute(int index)
            {
                result[index * 6 + 0] = input[index * 4 + 0] + baseVertexOffset;
                result[index * 6 + 1] = input[index * 4 + 2] + baseVertexOffset;
                result[index * 6 + 2] = input[index * 4 + 1] + baseVertexOffset;
                result[index * 6 + 3] = input[index * 4 + 2] + baseVertexOffset;
                result[index * 6 + 4] = input[index * 4 + 0] + baseVertexOffset;
                result[index * 6 + 5] = input[index * 4 + 3] + baseVertexOffset;
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
        public unsafe struct ConvertPositionHalfJob : IJobParallelFor
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
                var inPtr = (half3*)(input + i * inputByteStride);
                var outPtr = (float3*)(output + i * outputByteStride);

                var tmp = (float3)(*inPtr);
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
        public unsafe struct ConvertTangentHalfJob : IJobParallelFor
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
                var inPtr = (half4*)(input + i * inputByteStride);
                var outPtr = (float4*)(output + i * outputByteStride);

                var tmp = (float4)(*inPtr);
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
        public unsafe struct ConvertTexCoordHalfJob : IJobParallelFor
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
                var inPtr = (half2*)(input + i * inputByteStride);
                var outPtr = (float2*)(output + i * outputByteStride);

                var tmp = (float2)(*inPtr);
                tmp.y = 1 - tmp.y;
                *outPtr = tmp;
            }
        }


        [BurstCompile]
        public unsafe struct ConvertSkinWeightsJob : IJobParallelFor
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

                *outPtr = *inPtr;
            }
        }

        [BurstCompile]
        public struct ConvertMatrixJob : IJobParallelFor
        {
            public NativeArray<float4x4> matrices;

            public void Execute(int i)
            {

                var tmp = matrices[i];
                tmp.c0.y *= -1;
                tmp.c0.z *= -1;
                tmp.c1.x *= -1;
                tmp.c2.x *= -1;
                tmp.c3.x *= -1;
                matrices[i] = tmp;
            }
        }

        [BurstCompile]
        public unsafe struct ConvertSkinIndicesJob : IJobParallelFor
        {

            struct ushort4
            {
                public ushort4(uint x, uint y, uint z, uint w)
                {
                    m_X = (ushort)x;
                    m_Y = (ushort)y;
                    m_Z = (ushort)z;
                    m_W = (ushort)w;
                }

                ushort m_X;
                ushort m_Y;
                ushort m_Z;
                ushort m_W;
            }

            public uint inputByteStride;
            public int indicesOffset;
            public uint outputByteStride;

            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;

            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i)
            {
                var inputIndexPtr = (uint4*)(indicesOffset + input + i * inputByteStride);
                var outIndexPtr = (ushort4*)(indicesOffset + output + i * outputByteStride);

                // Set the correct values for the indices
                var tmpIndex = *inputIndexPtr;
                var tmpOut = new ushort4(tmpIndex[0], tmpIndex[1], tmpIndex[2], tmpIndex[3]);
                *outIndexPtr = tmpOut;
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
