// Copyright 2020-2021 Andreas Atteneder
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

#if USING_HDRP

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace GLTFast.Materials {

    public class HighDefinitionRPMaterialGenerator : ShaderGraphMaterialGenerator {
        
        static readonly int k_AlphaCutoffEnable = Shader.PropertyToID("_AlphaCutoffEnable");
        static readonly int k_ZTestGBufferPropId = Shader.PropertyToID("_ZTestGBuffer");

#if USING_HDRP_10_OR_NEWER
        // const string KW_DISABLE_DECALS = "_DISABLE_DECALS";
        const string KW_DISABLE_SSR_TRANSPARENT = "_DISABLE_SSR_TRANSPARENT";
        const string KW_DOUBLESIDED_ON = "_DOUBLESIDED_ON";
        const string KW_ENABLE_FOG_ON_TRANSPARENT = "_ENABLE_FOG_ON_TRANSPARENT";
        const string KW_SURFACE_TYPE_TRANSPARENT = "_SURFACE_TYPE_TRANSPARENT";
        
        const string k_ShaderPassTransparentDepthPrepass = "TransparentDepthPrepass";
        const string k_ShaderPassTransparentDepthPostpass = "TransparentDepthPostpass";
        const string k_ShaderPassTransparentBackface = "TransparentBackface";
        const string k_ShaderPassRayTracingPrepass = "RayTracingPrepass";

        static readonly int k_DoubleSidedEnablePropId = Shader.PropertyToID("_DoubleSidedEnable");
        static readonly int k_DoubleSidedNormalModePropId = Shader.PropertyToID("_DoubleSidedNormalMode");
        static readonly int k_DoubleSidedConstantsPropId = Shader.PropertyToID("_DoubleSidedConstants");
        static readonly int k_AlphaDstBlendPropId = Shader.PropertyToID("_AlphaDstBlend");
        static readonly int k_CullModeForwardPropId = Shader.PropertyToID("_CullModeForward");
#endif
        
#if USING_HDRP_10_OR_NEWER
        protected override void SetDoubleSided(Schema.Material gltfMaterial, Material material) {
            base.SetDoubleSided(gltfMaterial,material);

            material.EnableKeyword(KW_DOUBLESIDED_ON);
            material.SetFloat(k_DoubleSidedEnablePropId, 1);
                
            // UnityEditor.Rendering.HighDefinition.DoubleSidedNormalMode.Flip
            material.SetFloat(k_DoubleSidedNormalModePropId, 0);
            material.SetVector(k_DoubleSidedConstantsPropId, new Vector4(-1,-1,-1,0));
                
            material.SetFloat("_CullMode", (int)CullMode.Off);
        }
#endif

        protected override void SetAlphaModeMask(Schema.Material gltfMaterial, Material material) {
            base.SetAlphaModeMask(gltfMaterial,material);
            material.EnableKeyword(KW_ALPHATEST_ON);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_CUTOUT);
            material.SetFloat(k_AlphaCutoffEnable, 1);
            material.renderQueue = (int) RenderQueue.AlphaTest;
            material.SetFloat(k_ZTestGBufferPropId, (int)CompareFunction.Equal); //3
            material.SetOverrideTag("MotionVector","User");
            material.SetShaderPassEnabled("MOTIONVECTORS",false);
        }

#if USING_HDRP_10_OR_NEWER
        protected override void SetShaderModeBlend(Schema.Material gltfMaterial, Material material) {
            material.EnableKeyword(KW_ALPHATEST_ON);
            material.EnableKeyword(KW_SURFACE_TYPE_TRANSPARENT);
            // material.EnableKeyword(KW_DISABLE_DECALS);
            material.EnableKeyword(KW_DISABLE_SSR_TRANSPARENT);
            material.EnableKeyword(KW_ENABLE_FOG_ON_TRANSPARENT);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_TRANSPARENT);
            material.SetShaderPassEnabled(k_ShaderPassTransparentDepthPrepass, false);
            material.SetShaderPassEnabled(k_ShaderPassTransparentDepthPostpass, false);
            material.SetShaderPassEnabled(k_ShaderPassTransparentBackface, false);
            material.SetShaderPassEnabled(k_ShaderPassRayTracingPrepass, false);
            material.SetFloat(k_ZTestGBufferPropId, (int)CompareFunction.Equal); //3
            material.SetFloat(k_AlphaDstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(dstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(srcBlendPropId, (int) BlendMode.SrcAlpha);//5
            material.SetFloat(cullModePropId, (int)CullMode.Off);
            material.SetFloat(k_CullModeForwardPropId, (int)CullMode.Off);
        }
#endif
    }
}
#endif // USING_URP
