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


#ifndef UNITY_STANDARD_INPUT_INCLUDED
#define UNITY_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
#include "UnityStandardUtils.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || _PARALLAXMAP)
    #define _TANGENT_TO_WORLD 1
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
    #define _DETAIL 1
#endif

//---------------------------------------
half4       _Color;
half        _Cutoff;

sampler2D   _MainTex;
float4      _MainTex_ST;
float2      _MainTexRotation;
half        _MainTexUVChannel;

sampler2D   _DetailAlbedoMap;
float4      _DetailAlbedoMap_ST;

sampler2D   _BumpMap;
float4      _BumpMap_ST;
float2      _BumpMapRotation;
half        _BumpMapUVChannel;
half        _BumpScale;

sampler2D   _DetailMask;
sampler2D   _DetailNormalMap;
half        _DetailNormalMapScale;

sampler2D   _SpecGlossMap;
float4      _SpecGlossMap_ST;
float2      _SpecGlossMapRotation;
half        _SpecGlossMapUVChannel;

sampler2D   _MetallicGlossMap;
float4      _MetallicGlossMap_ST;
float2      _MetallicGlossMapRotation;
half        _MetallicGlossMapUVChannel;
half        _Metallic;
float       _Glossiness;
float       _Roughness;

sampler2D   _OcclusionMap;
float4      _OcclusionMap_ST;
float2      _OcclusionMapRotation;
half        _OcclusionMapUVChannel;
half        _OcclusionStrength;

sampler2D   _ParallaxMap;
half        _Parallax;
half        _UVSec;

half4       _EmissionColor;
sampler2D   _EmissionMap;
float4      _EmissionMap_ST;
float2      _EmissionMapRotation;
half        _EmissionMapUVChannel;

//-------------------------------------------------------------------------------------
// Input functions

struct VertexInput
{
    float4 vertex   : POSITION;
    half3 normal    : NORMAL;
    float2 uv0      : TEXCOORD0;
    float2 uv1      : TEXCOORD1;
#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
    float2 uv2      : TEXCOORD2;
#endif
#ifdef _TANGENT_TO_WORLD
    half4 tangent   : TANGENT;
#endif
    half4 color     : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 TexCoords(VertexInput v)
{
    float4 texcoord;
#ifdef _UV_ROTATION

    // Scale and Rotation: 2x2 matrix multiplication
    texcoord.z = v.uv0.x * _MainTex_ST.x + v.uv0.y * _MainTexRotation.y;
    texcoord.w = v.uv0.x * _MainTexRotation.x + v.uv0.y * _MainTex_ST.y;

    // Transform/Offset
    texcoord.xy = texcoord.zw + _MainTex_ST.zw;

    texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
#else
    texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
    texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
#endif
    return texcoord;
}

#ifdef _UV_ROTATION
#define TexCoordsSingle(uv,map) TexCoordsSingleIntern(uv,map##_ST,map##Rotation);
#else
#define TexCoordsSingle(uv,map) TexCoordsSingleSimple(uv,map##_ST);
#endif

float2 TexCoordsSingleSimple(float2 uv, float4 st)
{
    return uv * st.xy + st.zw;
}

float2 TexCoordsSingleIntern(float2 uv, float4 st, float2 rotation)
{
    float2 texcoord;
    // Scale and Rotation: 2x2 matrix multiplication
    float2 sr;
    sr.x = uv.x * st.x + uv.y * rotation.y;
    sr.y = uv.x * rotation.x + uv.y * st.y;
    // Transform/Offset
    texcoord.xy = sr + st.zw;
    return texcoord;
}

half DetailMask(float2 uv)
{
    return tex2D (_DetailMask, uv).a;
}

half3 Albedo(float2 texcoords)
{
    half3 albedo = _Color.rgb * tex2D (_MainTex, texcoords).rgb;
    return albedo;
}

half Alpha(float2 uv)
{
#if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
    return _Color.a;
#else
    return tex2D(_MainTex, uv).a * _Color.a;
#endif
}

half Occlusion(float2 uv)
{
#if (SHADER_TARGET < 30)
    // SM20: instruction count limitation
    // SM20: simpler occlusion
    return tex2D(_OcclusionMap, uv).r;
#else
    half occ = tex2D(_OcclusionMap, uv).r;
    return LerpOneTo (occ, _OcclusionStrength);
#endif
}

half4 SpecularGloss(float2 uv)
{
    half4 sg;
#ifdef _SPECGLOSSMAP
    sg = tex2D(_SpecGlossMap, uv);
    sg.rgb = sg.rgb * _SpecColor.rgb;
    sg.a *= _Glossiness;
#else
    sg.rgb = _SpecColor.rgb;
    sg.a = _Glossiness;
#endif
    return sg;
}

half2 MetallicGloss(float2 uv)
{
    half2 mg;

#ifdef _METALLICGLOSSMAP
    mg.rg = tex2D(_MetallicGlossMap, uv).bg;
    mg.r *= _Metallic;
    mg.g = 1-(mg.g*_Roughness);
#else
    mg.r = _Metallic;
    mg.g = 1-_Roughness;
#endif
    return mg;
}

half2 MetallicRough(float2 uv)
{
    half2 mg;
#ifdef _METALLICGLOSSMAP
    mg.r = tex2D(_MetallicGlossMap, uv).b * _Metallic;
#else
    mg.r = _Metallic;
#endif

#ifdef _SPECGLOSSMAP
    mg.g = 1.0f - (tex2D(_SpecGlossMap, uv).a*_Glossiness);
#else
    mg.g = 1.0f - _Glossiness;
#endif
    return mg;
}

half3 Emission(float2 uv)
{
#ifndef _EMISSION
    return 0;
#else
    return tex2D(_EmissionMap, uv).rgb * _EmissionColor.rgb;
#endif
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float2 texcoords)
{
    half3 normalTangent = UnpackScaleNormal(tex2D (_BumpMap, texcoords), _BumpScale);
    return normalTangent;
}
#endif

float4 Parallax (float4 texcoords, half3 viewDir)
{
#if !defined(_PARALLAXMAP) || (SHADER_TARGET < 30)
    // Disable parallax on pre-SM3.0 shader target models
    return texcoords;
#else
    half h = tex2D (_ParallaxMap, texcoords.xy).g;
    float2 offset = ParallaxOffset1Step (h, _Parallax, viewDir);
    return float4(texcoords.xy + offset, texcoords.zw + offset);
#endif

}

#endif // UNITY_STANDARD_INPUT_INCLUDED
