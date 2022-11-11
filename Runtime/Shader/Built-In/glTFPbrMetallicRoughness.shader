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

Shader "glTF/PbrMetallicRoughness"
{
    Properties
    {
        [MainColor] baseColorFactor("Base Color", Color) = (1,1,1,1)
        [MainTexture] baseColorTexture("Base Color Tex", 2D) = "white" {}
        baseColorTexture_Rotation ("Base Color Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] baseColorTexture_texCoord ("Base Color Tex UV", Float) = 0
        
        alphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        roughnessFactor("Roughness", Range(0.0, 1.0)) = 1
        // _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        // [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] metallicFactor("Metallic", Range(0.0, 1.0)) = 1.0
        metallicRoughnessTexture("Metallic-Roughness Tex", 2D) = "white" {}
        metallicRoughnessTexture_Rotation ("Metallic-Roughness Map Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] metallicRoughnessTexture_texCoord ("Metallic-Roughness Tex UV", Float) = 0

        // [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        // [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        normalTexture_scale("Normal Scale", Float) = 1.0
        [Normal] normalTexture("Normal Tex", 2D) = "bump" {}
        normalTexture_Rotation ("Normal Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] normalTexture_texCoord ("Normal Tex UV", Float) = 0

        // _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        // _ParallaxMap ("Height Map", 2D) = "black" {}

        occlusionTexture_strength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
        occlusionTexture("Occlusion Tex", 2D) = "white" {}
        occlusionTexture_Rotation ("Occlusion Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] occlusionTexture_texCoord ("Occlusion Tex UV", Float) = 0
        
        [HDR] emissiveFactor("Emissive", Color) = (0,0,0)
        emissiveTexture("Emission Tex", 2D) = "white" {}
        emissiveTexture_Rotation ("Emission Tex Rotation", Vector) = (0,0,0,0)
        [Enum(UV0,0,UV1,1)] emissiveTexture_texCoord ("Emission Tex UV", Float) = 0
        
        // _DetailMask("Detail Mask", 2D) = "white" {}

        // _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        // _DetailNormalMapScale("Scale", Float) = 1.0
        // [Normal] _DetailNormalMap("Normal Map", 2D) = "bump" {}

        // [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0

        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("Cull Mode", Float) = 2.0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300


        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_CullMode]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _EMISSION

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _TEXTURE_TRANSFORM
            // #pragma shader_feature_local _DETAIL_MULX2
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
            // #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertBase
            #pragma fragment fragBaseFacing
            #include "glTFIncludes/glTFUnityStandardCoreForward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual
            Cull [_CullMode]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _NORMALMAP

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _TEXTURE_TRANSFORM
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local _DETAIL_MULX2
            // #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAdd
            #pragma fragment fragAddFacing
            #include "glTFIncludes/glTFUnityStandardCoreForward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual
            Cull [_CullMode]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _TEXTURE_TRANSFORM
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Deferred pass
        Pass
        {
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }
            Cull [_CullMode]

            CGPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt


            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _EMISSION

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _TEXTURE_TRANSFORM
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local _DETAIL_MULX2
            // #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertDeferred
            #pragma fragment fragDeferredFacing

            #include "glTFIncludes/glTF.cginc"
            #include "glTFIncludes/glTFUnityStandardCore.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature _EMISSION

            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _TEXTURE_TRANSFORM
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "glTFIncludes/glTFUnityStandardMeta.cginc"
            ENDCG
        }
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 150

        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_CullMode]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _EMISSION

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _TEXTURE_TRANSFORM
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
            // SM2.0: NOT SUPPORTED shader_feature_local _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP

            #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #pragma vertex vertBase
            #pragma fragment fragBaseFacing

            #include "glTFIncludes/glTFUnityStandardCoreForward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual
            Cull [_CullMode]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _TEXTURE_TRANSFORM
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP
            #pragma skip_variants SHADOWS_SOFT

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex vertAdd
            #pragma fragment fragAddFacing
            #include "glTFIncludes/glTFUnityStandardCoreForward.cginc"

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual
            Cull [_CullMode]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _TEXTURE_TRANSFORM
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster

            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster

            #include "UnityStandardShadow.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature _EMISSION

            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _TEXTURE_TRANSFORM
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "glTFIncludes/glTFUnityStandardMeta.cginc"
            ENDCG
        }
    }


    FallBack "VertexLit"
    // CustomEditor "StandardShaderGUI"
    CustomEditor "GLTFast.Editor.BuiltInShaderGUI"
}
