// Copyright 2020 Andreas Atteneder
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

// Based on Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef GLTF_INCLUDED
#define GLTF_INCLUDED

#include "glTFUnityStandardCore.cginc"

void fragDeferredFacing (
    VertexOutputDeferred i,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3          // RT3: emission (rgb), --unused-- (a)
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    ,out half4 outShadowMask : SV_Target4       // RT4: shadowmask (rgba)
#endif
    ,half facing : VFACE
)
{
#ifdef _TANGENT_TO_WORLD
    i.tangentToWorldAndPackedData[0].xyz *= facing;
    i.tangentToWorldAndPackedData[1].xyz *= facing;
#endif
    i.tangentToWorldAndPackedData[2].xyz *= facing;
    fragDeferred(
        i,
        outGBuffer0,
        outGBuffer1,
        outGBuffer2,
        outEmission
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
        ,outShadowMask
#endif
    );
}

#endif // GLTF_INCLUDED