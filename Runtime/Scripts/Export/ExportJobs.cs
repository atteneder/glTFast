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

using System.Security.Permissions;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Export {
    
    [BurstCompile]
    static class ExportJobs {

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

        [BurstCompile]
        public unsafe struct ConvertPositionFloatJob : IJobParallelFor {

            public uint byteStride;
            
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;
            
            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i) {
                var inPtr = (float3*)(input + i * byteStride);
                var outPtr = (float3*)(output + i * byteStride);

                var tmp = *inPtr;
                tmp.x *= -1;
                *outPtr = tmp;
            }
        }
        
        [BurstCompile]
        public unsafe struct ConvertMatrixJob : IJobParallelFor {

            public uint byteStride;
            
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public NativeArray<float4x4> input;
            
            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public NativeArray<float4x4> output;

            public void Execute(int i) {
                
                var tmp = input[i];
                tmp.c0.y *= -1;
                tmp.c0.z *= -1;
                
                tmp.c1.x *= -1;
            
                tmp.c2.x *= -1;
            
                tmp.c3.x *= -1;

                output[i] = tmp;
            }
        }
        
        [BurstCompile]
        public unsafe struct ConvertSkinningJob : IJobParallelFor {

            struct ushort4
            {
                public ushort4(uint x, uint y, uint z, uint w)
                {
                    this.x = (ushort)x;
                    this.y = (ushort)y;
                    this.z = (ushort)z;
                    this.w = (ushort)w;
                }
                
                ushort x;
                ushort y;
                ushort z;
                ushort w;
            }
            
            public uint weightsByteStride;
            public uint indicesByteStride;
            public int weightsOffset;
            public int indicesOffset;
            
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;
            
            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i)
            {
                const int gltfSkinByteStride = 16 + 8; // 16 bytes for the weights (float4) plus 8 for the indices (ushort4)
                var inputWeightPtr = (float4*)(weightsOffset + input + i * weightsByteStride);
                var inputIndexPtr = (uint4*)(indicesOffset + input + i * indicesByteStride);
                var outWeightPtr = (float4*)(weightsOffset + output + i * gltfSkinByteStride);
                var outIndexPtr = (ushort4*)(indicesOffset + output + i * gltfSkinByteStride);

                var tmpIndex = *inputIndexPtr;
                ushort4 tmpOut = new ushort4(tmpIndex[0], tmpIndex[1], tmpIndex[2], tmpIndex[3]);
                *outIndexPtr = tmpOut;
                *outWeightPtr = *inputWeightPtr;
            }
        }
        
        [BurstCompile]
        public unsafe struct ConvertTangentFloatJob : IJobParallelFor {

            public uint byteStride;
            
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* input;
            
            [WriteOnly]
            [NativeDisableUnsafePtrRestriction]
            public byte* output;

            public void Execute(int i) {
                var inPtr = (float4*)(input + i * byteStride);
                var outPtr = (float4*)(output + i * byteStride);

                var tmp = *inPtr;
                tmp.z *= -1;
                *outPtr = tmp;
            }
        }
    }
}