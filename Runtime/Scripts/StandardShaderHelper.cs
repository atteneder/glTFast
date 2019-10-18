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

        public static int modePropId = Shader.PropertyToID("_Mode");
        public static int dstBlendPropId = Shader.PropertyToID("_DstBlend");
        public static int srcBlendPropId = Shader.PropertyToID("_SrcBlend");
        public static int zWritePropId = Shader.PropertyToID("_ZWrite");
        public static int glossinessPropId = Shader.PropertyToID("_Glossiness");
        public static int metallicPropId = Shader.PropertyToID("_Metallic");
        public static int cutoffPropId = Shader.PropertyToID("_Cutoff");
        public static int bumpMapPropId = Shader.PropertyToID("_BumpMap");
        public static int occlusionMapPropId = Shader.PropertyToID("_OcclusionMap");
        public static int emissionMapPropId = Shader.PropertyToID("_EmissionMap");
        public static int specGlossMapPropId = Shader.PropertyToID("_SpecGlossMap");
        public static int specColorPropId = Shader.PropertyToID("_SpecColor");
        public static int metallicGlossMapPropId = Shader.PropertyToID("_MetallicGlossMap");
        public static int glossMapScaleId = Shader.PropertyToID("_GlossMapScale");

        public static void SetAlphaModeMask( UnityEngine.Material material,Schema.Material gltfMaterial) {
			material.SetFloat(modePropId, (int)StandardShaderMode.Cutout);
            material.SetOverrideTag("RenderType", "TransparentCutout");
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(zWritePropId, 1);
            material.EnableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;  //2450
            material.SetFloat(cutoffPropId, gltfMaterial.alphaCutoff);
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