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
half4       baseColorFactor;
half        alphaCutoff;

sampler2D   baseColorTexture;
float4      baseColorTexture_ST;
float2      baseColorTexture_Rotation;
half        baseColorTexture_texCoord;

sampler2D   _DetailAlbedoMap;
float4      _DetailAlbedoMap_ST;

sampler2D   normalTexture;
float4      normalTexture_ST;
float2      normalTexture_Rotation;
half        normalTexture_texCoord;
half        normalTexture_scale;

sampler2D   _DetailMask;
sampler2D   _DetailNormalMap;
half        _DetailNormalMapScale;

sampler2D   specularGlossinessTexture;
float4      specularGlossinessTexture_ST;
float2      specularGlossinessTexture_Rotation;
half        specularGlossinessTexture_texCoord;

sampler2D   metallicRoughnessTexture;
float4      metallicRoughnessTexture_ST;
float2      metallicRoughnessTexture_Rotation;
half        metallicRoughnessTexture_texCoord;
half        metallicFactor;
float       glossinessFactor;
float       roughnessFactor;

sampler2D   occlusionTexture;
float4      occlusionTexture_ST;
float2      occlusionTexture_Rotation;
half        occlusionTexture_texCoord;
half        occlusionTexture_strength;

sampler2D   _ParallaxMap;
half        _Parallax;
half        _UVSec;

half4       emissiveFactor;
sampler2D   emissiveTexture;
float4      emissiveTexture_ST;
float2      emissiveTexture_Rotation;
half        emissiveTexture_texCoord;

half4       specularFactor;

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
#ifdef _TEXTURE_TRANSFORM

    // Scale and Rotation: 2x2 matrix multiplication
    texcoord.z = v.uv0.x * baseColorTexture_ST.x + v.uv0.y * baseColorTexture_Rotation.y;
    texcoord.w = v.uv0.x * baseColorTexture_Rotation.x + v.uv0.y * baseColorTexture_ST.y;

    // Transform/Offset
    texcoord.xy = texcoord.zw + baseColorTexture_ST.zw;

    texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
#else
    texcoord.xy = TRANSFORM_TEX(v.uv0, baseColorTexture); // Always source from uv0
    texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
#endif
    return texcoord;
}

#ifdef _TEXTURE_TRANSFORM
#define TexCoordsSingle(uv,map) TexCoordsSingleIntern(uv,map##_ST,map##_Rotation);
#else
#define TexCoordsSingle(uv,map) uv
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
    half3 albedo = baseColorFactor.rgb * tex2D (baseColorTexture, texcoords).rgb;
    return albedo;
}

half Alpha(float2 uv)
{
#if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
    return baseColorFactor.a;
#else
    return tex2D(baseColorTexture, uv).a * baseColorFactor.a;
#endif
}

half Occlusion(float2 uv)
{
#if (SHADER_TARGET < 30)
    // SM20: instruction count limitation
    // SM20: simpler occlusion
    return tex2D(occlusionTexture, uv).r;
#else
    half occ = tex2D(occlusionTexture, uv).r;
    return LerpOneTo (occ, occlusionTexture_strength);
#endif
}

half4 SpecularGloss(float2 uv)
{
    half4 sg;
#ifdef _SPECGLOSSMAP
    sg = tex2D(specularGlossinessTexture, uv);
    sg.rgb = sg.rgb * specularFactor.rgb;
    sg.a *= glossinessFactor;
#else
    sg.rgb = specularFactor.rgb;
    sg.a = glossinessFactor;
#endif
    return sg;
}

half2 MetallicGloss(float2 uv)
{
    half2 mg;

#ifdef _METALLICGLOSSMAP
    mg.rg = tex2D(metallicRoughnessTexture, uv).bg;
    mg.r *= metallicFactor;
    mg.g = 1-(mg.g*roughnessFactor);
#else
    mg.r = metallicFactor;
    mg.g = 1-roughnessFactor;
#endif
    return mg;
}

half2 MetallicRough(float2 uv)
{
    half2 mg;
#ifdef _METALLICGLOSSMAP
    mg.r = tex2D(metallicRoughnessTexture, uv).b * metallicFactor;
#else
    mg.r = metallicFactor;
#endif

#ifdef _SPECGLOSSMAP
    mg.g = 1.0f - (tex2D(specularGlossinessTexture, uv).a*glossinessFactor);
#else
    mg.g = 1.0f - glossinessFactor;
#endif
    return mg;
}

half3 Emission(float2 uv)
{
#ifndef _EMISSION
    return 0;
#else
    return tex2D(emissiveTexture, uv).rgb * emissiveFactor.rgb;
#endif
}

#ifdef _NORMALMAP
half3 NormalInTangentSpace(float2 texcoords)
{
    half3 normalTangent = UnpackScaleNormal(tex2D (normalTexture, texcoords), normalTexture_scale);
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
