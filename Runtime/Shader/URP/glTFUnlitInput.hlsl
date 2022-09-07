// Based on Unity UnlitInput.hlsl shader source from com.unity.render-pipelines.universal v12.1.7.

// com.unity.render-pipelines.universal copyright © 2020 Unity Technologies ApS
// Licensed under the Unity Companion License for Unity-dependent projects--see [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.

// Modifications Copyright 2022 Spatial

#ifndef UNIVERSAL_UNLIT_INPUT_INCLUDED
#define UNIVERSAL_UNLIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float _BaseMapUVChannel;
    float4 _BaseMapRotation;
    half4 _BaseColor;
    half _Cutoff;
    half _Surface;
CBUFFER_END

#endif
