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

using System.Collections.Generic;
using UnityEngine;

namespace GLTFast {

    using Materials;
   
    using AlphaMode = Schema.Material.AlphaMode;

    public class DefaultMaterialGenerator : IMaterialGenerator {

        Shader pbrMetallicRoughnessShader;
        Shader pbrMetallicRoughnessDoubleSideShader;
        Shader pbrSpecularGlossinessShader;
        Shader pbrSpecularGlossinessDoubleSideShader;
        Shader unlitShader;

        public UnityEngine.Material GetPbrMetallicRoughnessMaterial(bool doubleSided=false) {
            if(pbrMetallicRoughnessShader==null) {
                pbrMetallicRoughnessShader = Shader.Find("glTF/PbrMetallicRoughness");
            }
            var mat = new Material(pbrMetallicRoughnessShader);
            if(doubleSided) {
                // Turn of back-face culling
                mat.SetFloat(StandardShaderHelper.cullModePropId,0);
            }
            return mat;
        }

        public UnityEngine.Material GetPbrSpecularGlossinessMaterial(bool doubleSided=false) {
            if(pbrSpecularGlossinessShader==null) {
                pbrSpecularGlossinessShader = Shader.Find("glTF/PbrSpecularGlossiness");
            }
            var mat = new Material(pbrSpecularGlossinessShader);
            if(doubleSided) {
                // Turn of back-face culling
                mat.SetFloat(StandardShaderHelper.cullModePropId,0);
            }
            return mat;
        }

        public UnityEngine.Material GetUnlitMaterial(bool doubleSided=false) {
            if(unlitShader==null) {
                unlitShader = Shader.Find("glTF/Unlit");
            }
            var mat = new Material(unlitShader);
            if(doubleSided) {
                // Turn of back-face culling
                mat.SetFloat(StandardShaderHelper.cullModePropId,0);
            }
            return mat;
        }

        public UnityEngine.Material GenerateMaterial(
            Schema.Material gltfMaterial,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref Dictionary<int,Texture2D>[] imageVariants
        ) {
            UnityEngine.Material material;
            
            if (gltfMaterial.extensions!=null && gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness!=null) {
                material = GetPbrSpecularGlossinessMaterial(gltfMaterial.doubleSided);
            } else
            if (gltfMaterial.extensions.KHR_materials_unlit!=null) {
                material = GetUnlitMaterial(gltfMaterial.doubleSided);
            } else {
                material = GetPbrMetallicRoughnessMaterial(gltfMaterial.doubleSided);
            }

            material.name = gltfMaterial.name;

            //added support for KHR_materials_pbrSpecularGlossiness
            if (gltfMaterial.extensions != null) {
                Schema.PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null) {
                    material.color = specGloss.diffuseColor.gamma;
                    material.SetVector(StandardShaderHelper.specColorPropId, specGloss.specularColor);
                    material.SetFloat(StandardShaderHelper.glossinessPropId,specGloss.glossinessFactor);

                    TrySetTexture(specGloss.diffuseTexture,material,StandardShaderHelper.mainTexPropId,ref textures,ref schemaImages, ref imageVariants);

                    if (TrySetTexture(specGloss.specularGlossinessTexture,material,StandardShaderHelper.specGlossMapPropId,ref textures,ref schemaImages, ref imageVariants)) {
                        material.EnableKeyword(StandardShaderHelper.KW_SPEC_GLOSS_MAP);
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness!=null) {
                material.color = gltfMaterial.pbrMetallicRoughness.baseColor.gamma;
                material.SetFloat(StandardShaderHelper.metallicPropId, gltfMaterial.pbrMetallicRoughness.metallicFactor );
                material.SetFloat(StandardShaderHelper.roughnessPropId, gltfMaterial.pbrMetallicRoughness.roughnessFactor );

                TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                    material,
                    StandardShaderHelper.mainTexPropId,
                    ref textures,
                    ref schemaImages,
                    ref imageVariants
                    );
                
                if(TrySetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,material,StandardShaderHelper.metallicGlossMapPropId,ref textures,ref schemaImages, ref imageVariants)) {
                    material.EnableKeyword(StandardShaderHelper.KW_METALLIC_ROUGNESS_MAP);
                }
            }

            if(TrySetTexture(gltfMaterial.normalTexture,material,StandardShaderHelper.bumpMapPropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword("_NORMALMAP");
                material.SetFloat(StandardShaderHelper.bumpScalePropId,gltfMaterial.normalTexture.scale);
            }

            if(TrySetTexture(gltfMaterial.occlusionTexture,material,StandardShaderHelper.occlusionMapPropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(StandardShaderHelper.KW_OCCLUSION);
            }

            if(TrySetTexture(gltfMaterial.emissiveTexture,material,StandardShaderHelper.emissionMapPropId,ref textures,ref schemaImages, ref imageVariants)) {
                material.EnableKeyword(StandardShaderHelper.KW_EMISSION);
            }
            
            if(gltfMaterial.alphaModeEnum == AlphaMode.MASK) {
                material.SetFloat(StandardShaderHelper.cutoffPropId, gltfMaterial.alphaCutoff);
                StandardShaderHelper.SetAlphaModeMask( material, gltfMaterial);
            } else if(gltfMaterial.alphaModeEnum == AlphaMode.BLEND) {
                StandardShaderHelper.SetAlphaModeBlend( material );
            } else {
                StandardShaderHelper.SetOpaqueMode(material);
            }

            if(gltfMaterial.emissive != Color.black) {
                material.SetColor("_EmissionColor", gltfMaterial.emissive.gamma);
                material.EnableKeyword(StandardShaderHelper.KW_EMISSION);
            }

            return material;
        }

        static bool TrySetTexture(
            Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int propertyId,
            ref Schema.Texture[] textures,
            ref Schema.Image[] schemaImages,
            ref Dictionary<int,Texture2D>[] imageVariants
            )
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int bcTextureIndex = textureInfo.index;
                if (textures != null && textures.Length > bcTextureIndex)
                {
                    var txt = textures[bcTextureIndex];
                    var imageIndex = txt.GetImageIndex();

                    Texture2D img = null;
                    if( imageVariants!=null
                        && imageIndex >= 0
                        && imageVariants.Length > imageIndex
                        && imageVariants[imageIndex]!=null
                        && imageVariants[imageIndex].TryGetValue(txt.sampler,out img)
                        )
                    {
                        material.SetTexture(propertyId,img);
                        var isKtx = txt.isKtx;
                        TrySetTextureTransform(textureInfo,material,propertyId,isKtx);
                        return true;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Image #{0} not found", imageIndex);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Texture #{0} not found", bcTextureIndex);
                }
            }
            return false;
        }

        static void TrySetTextureTransform(
            Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int propertyId,
            bool flipY = false
            )
        {
            Vector2 offset = Vector2.zero;
            Vector2 scale = Vector2.one;

            if(textureInfo.extensions != null && textureInfo.extensions.KHR_texture_transform!=null) {
                var tt = textureInfo.extensions.KHR_texture_transform;
                if(tt.texCoord!=0) {
                    Debug.LogError("Multiple UV sets are not supported!");
                }

                float cos = 1;
                float sin = 0;

                if(tt.offset!=null) {
                    offset.x = tt.offset[0];
                    offset.y = 1-tt.offset[1];
                }
                if(tt.scale!=null) {
                    scale.x = tt.scale[0];
                    scale.y = tt.scale[1];
                    material.SetTextureScale(propertyId,scale);
                }
                if(tt.rotation!=0) {
                    cos = Mathf.Cos(tt.rotation);
                    sin = Mathf.Sin(tt.rotation);
                    material.SetVector(StandardShaderHelper.mainTexRotatePropId,new Vector4(cos,sin,-sin,cos));
                    material.EnableKeyword(StandardShaderHelper.KW_UV_ROTATION);
                    offset.x += scale.y * sin;
                }
                offset.y -= scale.y * cos;
                material.SetTextureOffset(propertyId,offset);
            }

            if(flipY) {
                offset.y = 1-offset.y;
                scale.y = -scale.y;
            }

            material.SetTextureOffset(propertyId,offset);
            material.SetTextureScale(propertyId,scale);
        }
    }
}
