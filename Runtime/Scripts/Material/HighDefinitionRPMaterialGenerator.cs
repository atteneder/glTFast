// Copyright 2020-2022 Andreas Atteneder
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
using UnityEngine.Rendering.HighDefinition;

namespace GLTFast.Materials {

    public class HighDefinitionRPMaterialGenerator : ShaderGraphMaterialGenerator {
        
        static readonly int k_AlphaCutoffEnable = Shader.PropertyToID("_AlphaCutoffEnable");
        static readonly int k_RenderQueueType = Shader.PropertyToID("_RenderQueueType");
        static readonly int k_SurfaceType = Shader.PropertyToID("_SurfaceType");
        static readonly int k_ZTestDepthEqualForOpaque = Shader.PropertyToID("_ZTestDepthEqualForOpaque");

        protected const string k_DistortionVectorsPass = "DistortionVectors";

        public static readonly int cullModeForwardPropId = Shader.PropertyToID("_CullModeForward");
        
#if USING_HDRP_10_OR_NEWER
        const string KW_DOUBLESIDED_ON = "_DOUBLESIDED_ON";

        static readonly int k_DoubleSidedEnablePropId = Shader.PropertyToID("_DoubleSidedEnable");
        static readonly int k_DoubleSidedNormalModePropId = Shader.PropertyToID("_DoubleSidedNormalMode");
        static readonly int k_DoubleSidedConstantsPropId = Shader.PropertyToID("_DoubleSidedConstants");
#endif
        
#if USING_HDRP_10_OR_NEWER
#if !UNITY_SHADER_GRAPH_12_OR_NEWER
        protected override string GetMetallicShaderName(MetallicShaderFeatures metallicShaderFeatures) {
            return SHADER_METALLIC;
        }
        
        protected override string GetSpecularShaderName(SpecularShaderFeatures features) {
            return SHADER_SPECULAR;
        }
        
        protected override string GetUnlitShaderName(UnlitShaderFeatures features) {
            return SHADER_UNLIT;
        }
#endif

        protected override void SetDoubleSided(Schema.Material gltfMaterial, Material material) {
            base.SetDoubleSided(gltfMaterial,material);

            material.EnableKeyword(KW_DOUBLESIDED_ON);
            material.SetFloat(k_DoubleSidedEnablePropId, 1);
                
            // UnityEditor.Rendering.HighDefinition.DoubleSidedNormalMode.Flip
            material.SetFloat(k_DoubleSidedNormalModePropId, 0);
            material.SetVector(k_DoubleSidedConstantsPropId, new Vector4(-1,-1,-1,0));
                
            material.SetFloat(cullModePropId, (int)CullMode.Off);
            material.SetFloat(cullModeForwardPropId, (int)CullMode.Off);
        }
#endif

        protected override void SetAlphaModeMask(Schema.Material gltfMaterial, Material material) {
            base.SetAlphaModeMask(gltfMaterial,material);
            
            material.SetFloat(k_AlphaCutoffEnable, 1);
            material.SetOverrideTag(TAG_MOTION_VECTOR,TAG_MOTION_VECTOR_USER);
            material.SetShaderPassEnabled(k_MotionVectorsPass,false);
            
            
            if (gltfMaterial.extensions?.KHR_materials_unlit != null) {
#if USING_HDRP_10_OR_NEWER
                material.EnableKeyword(KW_SURFACE_TYPE_TRANSPARENT);
                material.EnableKeyword(KW_DISABLE_SSR_TRANSPARENT);
                material.EnableKeyword(KW_ENABLE_FOG_ON_TRANSPARENT);
                
                material.SetShaderPassEnabled(k_ShaderPassTransparentDepthPrepass, false);
                material.SetShaderPassEnabled(k_ShaderPassTransparentDepthPostpass, false);
                material.SetShaderPassEnabled(k_ShaderPassTransparentBackface, false);
                material.SetShaderPassEnabled(k_ShaderPassRayTracingPrepass, false);
                material.SetShaderPassEnabled(k_ShaderPassDepthOnlyPass, false);

                material.SetFloat(k_AlphaDstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
#endif
                material.SetOverrideTag(TAG_RENDER_TYPE,TAG_RENDER_TYPE_TRANSPARENT);
                material.SetShaderPassEnabled(k_DistortionVectorsPass,false);
                material.SetFloat(dstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
                material.SetFloat(srcBlendPropId, (int) BlendMode.One);
                // material.SetFloat(k_RenderQueueType, 4);
                // material.SetFloat(k_SurfaceType, 1);
                material.SetFloat(k_ZTestDepthEqualForOpaque, (int)CompareFunction.LessEqual);
                material.SetFloat(zWritePropId, 0);
            }
        }

#if USING_HDRP_10_OR_NEWER
        protected override void SetShaderModeBlend(Schema.Material gltfMaterial, Material material) {
            
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.EnableKeyword(KW_SURFACE_TYPE_TRANSPARENT);
            // material.EnableKeyword(KW_DISABLE_DECALS);
            material.EnableKeyword(KW_DISABLE_SSR_TRANSPARENT);
            material.EnableKeyword(KW_ENABLE_FOG_ON_TRANSPARENT);
            
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_TRANSPARENT);
            
            material.SetShaderPassEnabled(k_ShaderPassTransparentDepthPrepass, false);
            material.SetShaderPassEnabled(k_ShaderPassTransparentDepthPostpass, false);
            material.SetShaderPassEnabled(k_ShaderPassTransparentBackface, false);
            material.SetShaderPassEnabled(k_ShaderPassRayTracingPrepass, false);
            material.SetShaderPassEnabled(k_ShaderPassDepthOnlyPass, false);
            
            material.SetFloat(k_AlphaCutoffEnable, 0);
            material.SetFloat(k_RenderQueueType, (int)CustomPass.RenderQueueType.PreRefraction );// 4
            material.SetFloat(k_SurfaceType, 1 );
            material.SetFloat(zWritePropId, 0);
            material.SetFloat(k_ZTestGBufferPropId, (int)CompareFunction.LessEqual); //4
            material.SetFloat(k_ZTestDepthEqualForOpaque, (int)CompareFunction.LessEqual); //4
            material.SetFloat(k_AlphaDstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(dstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetFloat(srcBlendPropId, (int) BlendMode.SrcAlpha);//5
        }
#endif
    }
}
#endif // USING_URP
