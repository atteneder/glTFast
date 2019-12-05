// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "glTF/PbrMetallicRoughnessDouble"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}
        _MainTexRotation ("Texture rotation", Vector) = (1,0,0,1)

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Roughness("Rougness", Range(0.0, 1.0)) = 1
        // _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        // [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        // [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        // [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}

        // _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        // _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

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

        _DoubleSided ("_DoubleSided", Float) = 0
    }

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 300

        UsePass "glTF/PbrMetallicRoughness/FORWARD"
        UsePass "glTF/PbrMetallicRoughness/FORWARD_DELTA"
        
        Pass
        {
            Name "FORWARD_BACK"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull Front

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _EMISSION
            #if UNITY_VERSION >= 201900
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _UV_ROTATION
            #else
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _OCCLUSION
            #pragma shader_feature _UV_ROTATION
            #endif
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

            #pragma vertex vertBaseBack
            #pragma fragment fragBase
            #include "glTFIncludes/glTFUnityStandardCoreForward.cginc"

            #if UNITY_STANDARD_SIMPLE
            VertexOutputBaseSimple vertBaseBack (VertexInput v) {
                v.normal = v.normal * -1;
                return vertBase(v);
            }
            #else
            VertexOutputForwardBase vertBaseBack (VertexInput v) {
                v.normal = v.normal * -1;
                return vertBase(v);
            }
            #endif
            ENDCG
        }

        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA_BACK"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual
            Cull Front

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _NORMALMAP
            #if UNITY_VERSION >= 201900
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _UV_ROTATION
            #else
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _OCCLUSION
            #pragma shader_feature _UV_ROTATION
            #endif
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local _DETAIL_MULX2
            // #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAddBack
            #pragma fragment fragAdd
            #include "glTFIncludes/glTFUnityStandardCoreForward.cginc"

            #if UNITY_STANDARD_SIMPLE
                VertexOutputForwardAddSimple vertAddBack (VertexInput v) {
                    v.normal = v.normal * -1;
                    return vertAdd(v);
                }
            #else
                VertexOutputForwardAdd vertAddBack (VertexInput v) {
                    v.normal = v.normal * -1;
                    return vertAdd(v);
                }
            #endif

            ENDCG
        }

        UsePass "glTF/PbrMetallicRoughness/ShadowCaster"
        UsePass "glTF/PbrMetallicRoughness/DEFERRED"

        Pass
        {
            Name "DEFERRED_BACK"
            Tags { "LightMode" = "Deferred" }
            Cull Front

            CGPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt


            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _EMISSION
            #if UNITY_VERSION >= 201900
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _UV_ROTATION
            #else
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _OCCLUSION
            #pragma shader_feature _UV_ROTATION
            #endif
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local _DETAIL_MULX2
            // #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertDeferredBack
            #pragma fragment fragDeferred
            #include "glTFIncludes/glTFUnityStandardCore.cginc"

            VertexOutputDeferred vertDeferredBack (VertexInput v) {
                v.normal = v.normal * -1;
                return vertDeferred(v);
            }
            ENDCG
        }

        UsePass "glTF/PbrMetallicRoughness/META"
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" }
        LOD 150

        UsePass "glTF/PbrMetallicRoughness/FORWARD"
        UsePass "glTF/PbrMetallicRoughness/FORWARD_DELTA"

        Pass
        {
            Name "FORWARD_BACK"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _EMISSION
            #if UNITY_VERSION >= 201900
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _UV_ROTATION
            #else
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _OCCLUSION
            #pragma shader_feature _UV_ROTATION
            #endif
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local _GLOSSYREFLECTIONS_OFF
            // SM2.0: NOT SUPPORTED shader_feature_local _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP

            #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #pragma vertex vertBaseBack
            #pragma fragment fragBase
            #include "glTFIncludes/glTFUnityStandardCoreForward.cginc"

            #if UNITY_STANDARD_SIMPLE
            VertexOutputBaseSimple vertBaseBack (VertexInput v) {
                v.normal = v.normal * -1;
                return vertBase(v);
            }
            #else
            VertexOutputForwardBase vertBaseBack (VertexInput v) {
                v.normal = v.normal * -1;
                return vertBase(v);
            }
            #endif
            ENDCG
        }
        
        Pass
        {
            Name "FORWARD_DELTA_BACK"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #if UNITY_VERSION >= 201900
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _OCCLUSION
            #pragma shader_feature_local _UV_ROTATION
            #else
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _OCCLUSION
            #pragma shader_feature _UV_ROTATION
            #endif
            // #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            // #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            // #pragma shader_feature_local _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature_local _PARALLAXMAP
            #pragma skip_variants SHADOWS_SOFT

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            #pragma vertex vertAddBack
            #pragma fragment fragAdd
            #include "glTFIncludes/glTFUnityStandardCoreForward.cginc"

            #if UNITY_STANDARD_SIMPLE
                VertexOutputForwardAddSimple vertAddBack (VertexInput v) {
                    v.normal = v.normal * -1;
                    return vertAdd(v);
                }
            #else
                VertexOutputForwardAdd vertAddBack (VertexInput v) {
                    v.normal = v.normal * -1;
                    return vertAdd(v);
                }
            #endif
            ENDCG
        }

        UsePass "glTF/PbrMetallicRoughness/ShadowCaster"
        UsePass "glTF/PbrMetallicRoughness/META"
    }

    FallBack "glTF/PbrMetallicRoughness"
}
