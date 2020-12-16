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

#if USING_HDRP

using System;
using System.Collections.Generic;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Image = GLTFast.Schema.Image;
using Material = UnityEngine.Material;
using Texture = GLTFast.Schema.Texture;

namespace GLTFast.Materials {

    public class HighDefinitionRPMaterialGenerator : ShaderGraphMaterialGenerator {
        
        const string KW_DISABLE_SSR_TRANSPARENT = "_DISABLE_SSR_TRANSPARENT";
        const string KW_DOUBLESIDED_ON = "_DOUBLESIDED_ON";
        const string KW_ENABLE_FOG_ON_TRANSPARENT = "_ENABLE_FOG_ON_TRANSPARENT";
        const string KW_NORMALMAP_TANGENT_SPACE = "_NORMALMAP_TANGENT_SPACE";
        const string KW_SURFACE_TYPE_TRANSPARENT = "_SURFACE_TYPE_TRANSPARENT";
        
        static readonly int alphaCutoffEnablePropId = Shader.PropertyToID("_AlphaCutoffEnable");
        static readonly int alphaDstBlendPropId = Shader.PropertyToID("_AlphaDstBlend");
        static readonly int alphaSrcBlendPropId = Shader.PropertyToID("_AlphaSrcBlend");
        static readonly int cullModePropId = Shader.PropertyToID("_CullMode");
        static readonly int cullModeForwardPropId = Shader.PropertyToID("_CullModeForward");
        static readonly int doubleSidedEnablePropId = Shader.PropertyToID("_DoubleSidedEnable");
        static readonly int doubleSidedNormalModePropId = Shader.PropertyToID("_DoubleSidedNormalMode");
        static readonly int enableBlendModePreserveSpecularLightingPropId = Shader.PropertyToID("_EnableBlendModePreserveSpecularLighting");

        public override Material GenerateMaterial(
            Schema.Material gltfMaterial,
            ref Texture[] textures,
            ref Image[] schemaImages,
            ref Dictionary<int, Texture2D>[] imageVariants
        ) {
            var material = base.GenerateMaterial(gltfMaterial, ref textures, ref schemaImages, ref imageVariants);
            material.EnableKeyword(KW_NORMALMAP_TANGENT_SPACE);
            material.EnableKeyword(KW_DISABLE_SSR_TRANSPARENT);
            if (gltfMaterial.doubleSided) {
                material.EnableKeyword(KW_DOUBLESIDED_ON);
                material.SetInt(doubleSidedEnablePropId,1);
                material.SetInt(doubleSidedNormalModePropId,0);
                material.SetInt(cullModePropId,0);
                material.SetInt(cullModeForwardPropId,0);
            }
            return material;
        }

        protected override void ApplyAlphaCutoff(Material material, bool enable, float alphaCutoff) {
            if (enable) {
                material.EnableKeyword(KW_ALPHATEST_ON);
                material.SetFloat(alphaCutoffPropId, alphaCutoff);
                material.SetFloat(alphaCutoffEnablePropId, 1);
            } else {
                material.DisableKeyword(KW_ALPHATEST_ON);
                material.SetFloat(alphaCutoffPropId, 0);
                material.SetFloat(alphaCutoffEnablePropId, 0);
            }
        }

        protected override void ApplyClearcoat(ref Texture[] textures, ref Image[] schemaImages, ref Dictionary<int, Texture2D>[] imageVariants, Material material, ClearCoat clearcoat) {
            base.ApplyClearcoat(ref textures,ref schemaImages,ref imageVariants,material,clearcoat);

            if (TrySetTexture(clearcoat.clearcoatNormalTexture, material, clearcoatNormalTexturePropId, ref textures, ref schemaImages, ref imageVariants)) { }
        }
        
        /// <summary>
        /// Get the shader's name that supports the required features.
        /// </summary>
        /// <param name="metallicShaderFeatures">Required features</param>
        protected override string GetShaderName(MetallicShaderFeatures metallicShaderFeatures) {
            ShaderMode mode = (ShaderMode) (metallicShaderFeatures & MetallicShaderFeatures.ModeMask);
            bool coat = (metallicShaderFeatures & MetallicShaderFeatures.ClearCoat) != 0;
            // TODO: add sheen support
            bool sheen = false; // (metallicShaderFeatures & MetallicShaderFeatures.Sheen) != 0;
            bool doubleSided = (metallicShaderFeatures & MetallicShaderFeatures.DoubleSided) != 0;
            return $"Shader Graphs/glTF-metallic-Opaque{(coat ? "-coat" : "")}{(sheen ? "-sheen" : "")}";
        }
        
        public override void SetAlphaModeBlend( Material material ) {
            material.EnableKeyword(KW_ENABLE_FOG_ON_TRANSPARENT);
            material.EnableKeyword(KW_SURFACE_TYPE_TRANSPARENT);
            
            material.SetInt(srcBlendPropId, (int)BlendMode.One);//5
            material.SetInt(dstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(alphaSrcBlendPropId, (int)BlendMode.One);//5
            material.SetInt(alphaDstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(zWritePropId, 0);
            material.SetInt(enableBlendModePreserveSpecularLightingPropId, 0);
        }

        public override   void SetAlphaModeTransparent( Material material ) {
            material.EnableKeyword(KW_ENABLE_FOG_ON_TRANSPARENT);
            material.EnableKeyword(KW_SURFACE_TYPE_TRANSPARENT);

            material.SetInt(srcBlendPropId, (int)BlendMode.One);//5
            material.SetInt(dstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(alphaSrcBlendPropId, (int)BlendMode.One);//5
            material.SetInt(alphaDstBlendPropId, (int)BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(zWritePropId, 0);
            material.SetInt(enableBlendModePreserveSpecularLightingPropId, 1);
        }
    }
}
#endif // USING_URP
