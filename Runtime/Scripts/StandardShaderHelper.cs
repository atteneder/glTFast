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
using UnityEngine;

namespace GLTFast.Materials {

    using Schema;

    public static class StandardShaderHelper {

        public const string KW_ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";
        public const string KW_ALPHATEST_ON = "_ALPHATEST_ON";
        public const string KW_EMISSION = "_EMISSION";
        public const string KW_NORMALMAP = "_NORMALMAP";
        public const string KW_UV_ROTATION = "_UV_ROTATION";

        public static int bumpMapPropId = Shader.PropertyToID("_BumpMap");
        public static int bumpScalePropId = Shader.PropertyToID("_BumpScale");
        public static int cutoffPropId = Shader.PropertyToID("_Cutoff");
        public static int dstBlendPropId = Shader.PropertyToID("_DstBlend");
        public static int emissionColorPropId = Shader.PropertyToID("_EmissionColor");
        public static int emissionMapPropId = Shader.PropertyToID("_EmissionMap");
        public static int mainTexRotatePropId = Shader.PropertyToID("_MainTexRotation");
        public static int metallicGlossMapPropId = Shader.PropertyToID("_MetallicGlossMap");
        public static int metallicPropId = Shader.PropertyToID("_Metallic");
        public static int occlusionMapPropId = Shader.PropertyToID("_OcclusionMap");
        public static int specColorPropId = Shader.PropertyToID("_SpecColor");
        public static int specGlossMapPropId = Shader.PropertyToID("_SpecGlossMap");
        public static int srcBlendPropId = Shader.PropertyToID("_SrcBlend");
        public static int zWritePropId = Shader.PropertyToID("_ZWrite");

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
        public const string TAG_RENDER_TYPE_TRANSPARENT = "Transparent";

        public const string KW_ALPHABLEND_ON = "_ALPHABLEND_ON";
        public const string KW_MAIN_MAP = "_MainTex";
        public const string KW_METALLIC_ROUGNESS_MAP = "_METALLICGLOSSMAP";
        public const string KW_OCCLUSION = "_OCCLUSION";        
        public const string KW_SPEC_GLOSS_MAP = "_SPECGLOSSMAP";

        public static int cullModePropId = Shader.PropertyToID("_CullMode");
        public static int glossinessPropId = Shader.PropertyToID("_Glossiness");
        public static int mainTexPropId = Shader.PropertyToID(KW_MAIN_MAP);
        public static int modePropId = Shader.PropertyToID("_Mode");
        public static int roughnessPropId = Shader.PropertyToID("_Roughness");

#endif

#if GLTFAST_SHADER_GRAPH || UNITY_EDITOR

        public const string KW_METALLICSPECGLOSSMAP = "_METALLICSPECGLOSSMAP";
        public const string KW_OCCLUSIONMAP = "_OCCLUSIONMAP";
        public const string KW_SPECULAR_SETUP = "_SPECULAR_SETUP";

        public static int alphaClipPropId = Shader.PropertyToID("_AlphaClip");
        public static int baseColorPropId = Shader.PropertyToID("_BaseColor");
        public static int baseMapPropId = Shader.PropertyToID("_BaseMap");
        public static int blendPropId = Shader.PropertyToID("_Blend");
        public static int cullPropId = Shader.PropertyToID("_Cull");
        public static int smoothnessPropId = Shader.PropertyToID("_Smoothness");
        public static int workflowModePropId = Shader.PropertyToID("_WorkflowMode");

#endif

        public static void SetAlphaModeMask(UnityEngine.Material material, float alphaCutoff)
        {
            material.EnableKeyword(KW_ALPHATEST_ON);
            material.SetInt(zWritePropId, 1);
            material.SetFloat(cutoffPropId, alphaCutoff);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;  //2450

#if GLTFAST_SHADER_GRAPH
            material.SetFloat(alphaClipPropId, 1);
#else
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
#if GLTFAST_SHADER_GRAPH
            material.SetInt(blendPropId, 0);
#else
            material.SetFloat(modePropId, (int)StandardShaderMode.Transparent);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_TRANSPARENT);
            material.EnableKeyword(KW_ALPHABLEND_ON);
#endif

            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);//5
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(zWritePropId, 0);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
        }

        public static void SetOpaqueMode(UnityEngine.Material material) {
#if GLTFAST_SHADER_GRAPH
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;  //2000;
#else
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
