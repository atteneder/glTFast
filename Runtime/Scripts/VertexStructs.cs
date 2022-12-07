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

using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace GLTFast.Vertex
{

    // Most struct members are never accessed, but keeping them public makes still sense for future use.
    // ReSharper disable MemberCanBePrivate.Global

    [StructLayout(LayoutKind.Sequential)]
    struct VPosNormTan
    {
        public float3 position;
        public float3 normal;
        public float4 tangent;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VPosNorm
    {
        public float3 position;
        public float3 normal;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VPos
    {
        public float3 position;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VTexCoord1
    {
        public float2 uv0;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VTexCoord2
    {
        public float2 uv0;
        public float2 uv1;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VTexCoord3
    {
        public float2 uv0;
        public float2 uv1;
        public float2 uv2;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VTexCoord4
    {
        public float2 uv0;
        public float2 uv1;
        public float2 uv2;
        public float2 uv3;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VTexCoord5
    {
        public float2 uv0;
        public float2 uv1;
        public float2 uv2;
        public float2 uv3;
        public float2 uv4;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VTexCoord6
    {
        public float2 uv0;
        public float2 uv1;
        public float2 uv2;
        public float2 uv3;
        public float2 uv4;
        public float2 uv5;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VTexCoord7
    {
        public float2 uv0;
        public float2 uv1;
        public float2 uv2;
        public float2 uv3;
        public float2 uv4;
        public float2 uv5;
        public float2 uv6;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VTexCoord8
    {
        public float2 uv0;
        public float2 uv1;
        public float2 uv2;
        public float2 uv3;
        public float2 uv4;
        public float2 uv5;
        public float2 uv6;
        public float2 uv7;
    }

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct VBones
    {
        public fixed float weights[4];
        public fixed uint joints[4];

#if DEBUG
        public override string ToString() {
            return $"{joints[0]}/{weights[0]}, {joints[1]}/{weights[1]}, {joints[2]}/{weights[2]}, {joints[3]}/{weights[3]}";
        }
#endif
    }

    // ReSharper restore MemberCanBePrivate.Global
}
