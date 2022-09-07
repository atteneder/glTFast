// Based on Unity shader sources from com.unity.render-pipelines.universal v12.1.7.

// com.unity.render-pipelines.universal copyright © 2020 Unity Technologies ApS
// Licensed under the Unity Companion License for Unity-dependent projects--see [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.

// Modifications Copyright 2022 Spatial

///////////////////////////////////////////////////////////////////////////////
//                          UV Transform                                     //
///////////////////////////////////////////////////////////////////////////////

// Refered from TRANSFORM_TEX in "Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"
#ifdef _UV_ROTATION
    #define TRANSFORM_TEX_GLTF(uv0, uv1, map) GetTransformedTexcoord(uv0, uv1, map##UVChannel, map##_ST, map##Rotation)
#else
    #define TRANSFORM_TEX_GLTF(uv0, uv1, map) GetTransformedTexcoord(uv0, uv1, map##UVChannel, map##_ST)
#endif

float2 GetTransformedTexcoord(float2 uv0, float2 uv1, float uvChannel, float4 st, float2 rotation)
{
    float2 uv = (uvChannel==0)? uv0 : uv1;
    float2 texcoord;
    // Scale and Rotation: 2x2 matrix multiplication
    float2 sr;
    sr.x = uv.x * st.x + uv.y * rotation.y;
    sr.y = uv.x * rotation.x + uv.y * st.y;
    // Transform/Offset
    texcoord.xy = sr + st.zw;
    return texcoord;
}

float2 GetTransformedTexcoord(float2 uv0, float2 uv1, float uvChannel, float4 st)
{
    float2 uv = (uvChannel==0)? uv0 : uv1;
    return uv * st.xy + st.zw;
}
