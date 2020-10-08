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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GLTFast.Materials {

    using Schema;

    public static class StandardShaderHelper {

        public enum StandardShaderMode {
            Opaque = 0,
            Cutout = 1,
            Fade = 2,
            Transparent = 3
        }

        public const string KW_MAIN_MAP = "_MainTex";
        public const string KW_METALLIC_ROUGNESS_MAP = "_METALLICGLOSSMAP";
        public const string KW_SPEC_GLOSS_MAP = "_SPECGLOSSMAP";
        public const string KW_OCCLUSION = "_OCCLUSION";
        public const string KW_UV_ROTATION = "_UV_ROTATION";
        public const string KW_EMISSION = "_EMISSION";

        public static int modePropId = Shader.PropertyToID("_Mode");
        public static int dstBlendPropId = Shader.PropertyToID("_DstBlend");
        public static int srcBlendPropId = Shader.PropertyToID("_SrcBlend");
        public static int zWritePropId = Shader.PropertyToID("_ZWrite");
        public static int glossinessPropId = Shader.PropertyToID("_Glossiness");
        public static int roughnessPropId = Shader.PropertyToID("_Roughness");
        public static int metallicPropId = Shader.PropertyToID("_Metallic");
        public static int cutoffPropId = Shader.PropertyToID("_Cutoff");
        public static int mainTexPropId = Shader.PropertyToID(KW_MAIN_MAP);
        public static int mainTexRotatePropId = Shader.PropertyToID("_MainTexRotation");
        public static int bumpMapPropId = Shader.PropertyToID("_BumpMap");
        public static int bumpScalePropId = Shader.PropertyToID("_BumpScale");
        public static int occlusionMapPropId = Shader.PropertyToID("_OcclusionMap");
        public static int emissionMapPropId = Shader.PropertyToID("_EmissionMap");
        public static int specGlossMapPropId = Shader.PropertyToID("_SpecGlossMap");
        public static int specColorPropId = Shader.PropertyToID("_SpecColor");
        public static int metallicGlossMapPropId = Shader.PropertyToID("_MetallicGlossMap");
        // public static int glossMapScaleId = Shader.PropertyToID("_GlossMapScale");
        public static int cullModePropId = Shader.PropertyToID("_CullMode");

        public static void SetAlphaModeMask(UnityEngine.Material material, float alphaCutoff)
        {
            material.SetFloat(modePropId, (int)StandardShaderMode.Cutout);
            material.SetOverrideTag("RenderType", "TransparentCutout");
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(zWritePropId, 1);
            material.EnableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;  //2450
            material.SetFloat(cutoffPropId, alphaCutoff);
        }

        public static void SetAlphaModeMask(UnityEngine.Material material, Schema.Material gltfMaterial)
        {
            SetAlphaModeMask(material, gltfMaterial.alphaCutoff);
        }

        public static void SetAlphaModeBlend( UnityEngine.Material material ) {
            material.SetFloat(modePropId, (int)StandardShaderMode.Transparent);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);//5
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(zWritePropId, 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
        }

        public static void SetOpaqueMode(UnityEngine.Material material) {
            material.SetOverrideTag("RenderType", "Opaque");
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(zWritePropId, 1);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = -1;
        }

    }
}