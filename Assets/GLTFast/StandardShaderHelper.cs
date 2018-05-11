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
        public static int glossinessPropId = Shader.PropertyToID("_Glossiness");
        public static int metallicPropId = Shader.PropertyToID("_Metallic");
        public static int cutoffPropId = Shader.PropertyToID("_Cutoff");

        public static int bumpMapPropId = Shader.PropertyToID("_BumpMap");
        public static int occlusionMapPropId = Shader.PropertyToID("_OcclusionMap");
        public static int emissionMapPropId = Shader.PropertyToID("_EmissionMap");
        public static int metallicGlossMapPropId = Shader.PropertyToID("_MetallicGlossMap");

        public static void SetAlphaModeMask( UnityEngine.Material material ) {
			material.SetFloat(modePropId, (int)StandardShaderMode.Cutout);
			material.SetOverrideTag("RenderType", "TransparentCutout"); 
			material.EnableKeyword("_ALPHATEST_ON");
			material.renderQueue = 2450;
        }

        public static void SetAlphaModeBlend( UnityEngine.Material material ) {
            material.SetFloat(modePropId, (int)StandardShaderMode.Transparent);
            material.SetFloat(dstBlendPropId, 10);
            material.SetOverrideTag("RenderType", "Transparent");
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
    }
}