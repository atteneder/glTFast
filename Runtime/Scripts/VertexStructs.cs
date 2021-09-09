﻿// Copyright 2020-2021 Andreas Atteneder
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

    [StructLayout(LayoutKind.Sequential)]
    struct VPosNormTan {
        public float3 pos;
        public float3 nrm;
        public float4 tan;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VPosNorm {
        public float3 pos;
        public float3 nrm;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VPos {
        public float3 pos;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VTexCoord1 {
        public float2 uv0;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct VTexCoord2 {
        public float2 uv0;
        public float2 uv1;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    struct VBones {
        public float weight0;
        public float weight1;
        public float weight2;
        public float weight3;

        public uint joint0;
        public uint joint1;
        public uint joint2;
        public uint joint3;
    }
}
