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

#if !GLTFAST_SHADER_GRAPH
#define GLTFAST_BUILTIN_RP
#endif

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace GLTFast.Materials {

    using Schema;

    public static class StandardShaderHelper {

        public const string KW_EMISSION = "_EMISSION";
        public const string KW_NORMALMAP = "_NORMALMAP";
        public const string KW_UV_ROTATION = "_UV_ROTATION";

        private const string KW_ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";
        private const string KW_ALPHATEST_ON = "_ALPHATEST_ON";
        
        public static readonly int bumpMapPropId = Shader.PropertyToID("_BumpMap");
        public static readonly int bumpScalePropId = Shader.PropertyToID("_BumpScale");
        public static readonly int cutoffPropId = Shader.PropertyToID("_Cutoff");
        public static readonly int emissionColorPropId = Shader.PropertyToID("_EmissionColor");
        public static readonly int emissionMapPropId = Shader.PropertyToID("_EmissionMap");
        public static readonly int mainTexRotation = Shader.PropertyToID("_MainTexRotation");
        public static readonly int mainTexScaleTransform = Shader.PropertyToID("_MainTex_ST");
        public static readonly int metallicGlossMapPropId = Shader.PropertyToID("_MetallicGlossMap");
        public static readonly int metallicPropId = Shader.PropertyToID("_Metallic");
        public static readonly int occlusionMapPropId = Shader.PropertyToID("_OcclusionMap");
        public static readonly int specColorPropId = Shader.PropertyToID("_SpecColor");
        public static readonly int specGlossMapPropId = Shader.PropertyToID("_SpecGlossMap");

        private static readonly int dstBlendPropId = Shader.PropertyToID("_DstBlend");
        private static readonly int srcBlendPropId = Shader.PropertyToID("_SrcBlend");
        private static readonly int zWritePropId = Shader.PropertyToID("_ZWrite");

#if GLTFAST_BUILTIN_RP || UNITY_EDITOR

        public enum StandardShaderMode {
            Opaque = 0,
            Cutout = 1,
            Fade = 2,
            Transparent = 3
        }

        public const string TAG_RENDER_TYPE = "RenderType";
        public const string TAG_RENDER_TYPE_CUTOUT = "TransparentCutout";
        public const string TAG_RENDER_TYPE_OPAQUE = "Opaque";
        public const string TAG_RENDER_TYPE_FADE = "Fade";
        public const string TAG_RENDER_TYPE_TRANSPARENT = "Transparent";

        public const string KW_METALLIC_ROUGNESS_MAP = "_METALLICGLOSSMAP";
        public const string KW_OCCLUSION = "_OCCLUSION";        
        public const string KW_SPEC_GLOSS_MAP = "_SPECGLOSSMAP";

        private const string KW_ALPHABLEND_ON = "_ALPHABLEND_ON";
        private const string KW_MAIN_MAP = "_MainTex";

        public static int cullModePropId = Shader.PropertyToID("_CullMode");
        public static int glossinessPropId = Shader.PropertyToID("_Glossiness");
        public static int mainTexPropId = Shader.PropertyToID(KW_MAIN_MAP);
        public static int roughnessPropId = Shader.PropertyToID("_Roughness");

        private static int modePropId = Shader.PropertyToID("_Mode");

#endif

        public static void SetAlphaModeMask(UnityEngine.Material material, float alphaCutoff)
        {
            material.EnableKeyword(KW_ALPHATEST_ON);
            material.SetInt(zWritePropId, 1);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;  //2450

#if GLTFAST_BUILTIN_RP || UNITY_EDITOR
            material.SetFloat(cutoffPropId, alphaCutoff);
            material.SetFloat(modePropId, (int)StandardShaderMode.Cutout);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_CUTOUT);
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.DisableKeyword(KW_ALPHABLEND_ON);
#endif
        }

        public static void SetAlphaModeMask(UnityEngine.Material material, Schema.Material gltfMaterial)
        {
            SetAlphaModeMask(material, gltfMaterial.alphaCutoff);
        }

        public static void SetAlphaModeBlend( UnityEngine.Material material ) {
#if GLTFAST_BUILTIN_RP || UNITY_EDITOR
            material.SetFloat(modePropId, (int)StandardShaderMode.Fade);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_FADE);
            material.EnableKeyword(KW_ALPHABLEND_ON);
#endif

            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);//5
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(zWritePropId, 0);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
        }

        public static void SetAlphaModeTransparent( UnityEngine.Material material ) {
#if GLTFAST_BUILTIN_RP || UNITY_EDITOR
            material.SetFloat(modePropId, (int)StandardShaderMode.Fade);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_TRANSPARENT);
            material.EnableKeyword(KW_ALPHAPREMULTIPLY_ON);
#endif
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);//1
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(zWritePropId, 0);
            material.DisableKeyword(KW_ALPHABLEND_ON);
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
        }

        public static void SetOpaqueMode(UnityEngine.Material material) {
#if GLTFAST_BUILTIN_RP || UNITY_EDITOR
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_OPAQUE);
            material.DisableKeyword(KW_ALPHABLEND_ON);
            material.renderQueue = -1;
#endif
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(zWritePropId, 1);
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
        }

    }
}
