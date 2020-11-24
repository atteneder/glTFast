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

#if GLTFAST_URP

using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {

    using Materials;
   
    using AlphaMode = Schema.Material.AlphaMode;

    public class URPMaterialGenerator : MaterialGenerator {

        const string SHADER_LIT = "Universal Render Pipeline/Lit";
        const string SHADER_UNLIT = "Universal Render Pipeline/Unlit";

        Shader litShader;
        Shader unlitShader;

        public override UnityEngine.Material GetDefaultMaterial() {
            return GetLitMaterial();
        }

        UnityEngine.Material GetLitMaterial(bool doubleSided=false, bool metallic = true) {
            if(litShader==null) {
                litShader = FindShader(SHADER_LIT);
            }
            if(litShader==null) {
                return null;
            }
            var mat = new Material(litShader);
            if(!metallic) {
                mat.EnableKeyword(StandardShaderHelper.KW_SPECULAR_SETUP);
                mat.SetFloat(StandardShaderHelper.workflowModePropId,0);
            }
            if(doubleSided) {
                // Turn off back-face culling
                mat.SetFloat(StandardShaderHelper.cullPropId,0);
#if UNITY_EDITOR
                mat.doubleSidedGI = true;
#endif
            }
            return mat;
        }

        UnityEngine.Material GetUnlitMaterial(bool doubleSided=false) {
            if(unlitShader==null) {
                unlitShader = FindShader(SHADER_UNLIT);
            }
            if(unlitShader==null) {
                return null;
            }
            var mat = new Material(unlitShader);
            if(doubleSided) {
                // Turn off back-face culling
                mat.SetFloat(StandardShaderHelper.cullPropId,0);
#if UNITY_EDITOR
                mat.doubleSidedGI = true;
#endif
            }
            return mat;
        }

        public override UnityEngine.Material GenerateMaterial(
            Schema.Material gltfMaterial,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref Dictionary<int,Texture2D>[] imageVariants
        ) {
            UnityEngine.Material material;
            
            if (gltfMaterial.extensions!=null && gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness!=null) {
                material = GetLitMaterial(gltfMaterial.doubleSided,false);
            } else
            if (gltfMaterial.extensions.KHR_materials_unlit!=null) {
                material = GetUnlitMaterial(gltfMaterial.doubleSided);
            } else {
                material = GetLitMaterial(gltfMaterial.doubleSided);
            }

            if(material==null) return null;

            material.name = gltfMaterial.name;

            //added support for KHR_materials_pbrSpecularGlossiness
            if (gltfMaterial.extensions != null) {
                Schema.PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null) {
                    material.SetVector(StandardShaderHelper.baseColorPropId, specGloss.diffuseColor.gamma);
                    material.SetVector(StandardShaderHelper.specColorPropId, specGloss.specularColor);
                    material.SetFloat(StandardShaderHelper.smoothnessPropId,specGloss.glossinessFactor);

                    TrySetTexture(specGloss.diffuseTexture,material,StandardShaderHelper.baseMapPropId,ref textures,ref schemaImages, ref imageVariants);

                    if (TrySetTexture(specGloss.specularGlossinessTexture,material,StandardShaderHelper.specGlossMapPropId,ref textures,ref schemaImages, ref imageVariants)) {
                        material.EnableKeyword(StandardShaderHelper.KW_METALLICSPECGLOSSMAP);
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness!=null) {
                material.SetVector(StandardShaderHelper.baseColorPropId, gltfMaterial.pbrMetallicRoughness.baseColor.gamma);
                material.SetFloat(StandardShaderHelper.metallicPropId, gltfMaterial.pbrMetallicRoughness.metallicFactor );
                material.SetFloat(StandardShaderHelper.smoothnessPropId, 1-gltfMaterial.pbrMetallicRoughness.roughnessFactor );

                TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                    material,
                    StandardShaderHelper.baseMapPropId,
                    ref textures,
                    ref schemaImages,
                    ref imageVariants
                    );
                
                if(TrySetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,material,StandardShaderHelper.metallicGlossMapPropId,ref textures,ref schemaImages, ref imageVariants)) {
                    material.EnableKeyword(StandardShaderHelper.KW_METALLICSPECGLOSSMAP);
                }
            }

            if(TrySetTexture(gltfMaterial.normalTexture,material,StandardShaderHelper.bumpMapPropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(StandardShaderHelper.KW_NORMALMAP);
                material.SetFloat(StandardShaderHelper.bumpScalePropId,gltfMaterial.normalTexture.scale);
            }

            if(TrySetTexture(gltfMaterial.occlusionTexture,material,StandardShaderHelper.occlusionMapPropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(StandardShaderHelper.KW_OCCLUSIONMAP);
            }

            if(TrySetTexture(gltfMaterial.emissiveTexture,material,StandardShaderHelper.emissionMapPropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(StandardShaderHelper.KW_EMISSION);
            }
            
            if(gltfMaterial.alphaModeEnum == AlphaMode.MASK) {
                StandardShaderHelper.SetAlphaModeMask(material, gltfMaterial.alphaCutoff);
            } else if(gltfMaterial.alphaModeEnum == AlphaMode.BLEND) {
                StandardShaderHelper.SetAlphaModeBlend(material);
            } else {
                StandardShaderHelper.SetOpaqueMode(material);
            }

            if(gltfMaterial.emissive != Color.black) {
                material.SetColor(StandardShaderHelper.emissionColorPropId, gltfMaterial.emissive.gamma);
                material.EnableKeyword(StandardShaderHelper.KW_EMISSION);
            }

            return material;
        }
    }
}
#endif // GLTFAST_URP
