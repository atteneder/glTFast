using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;


namespace GLTFast {

    using Materials;
   
    using AlphaMode = Schema.Material.AlphaMode;

    public class DefaultMaterialGenerator : IMaterialGenerator {

        static readonly Vector2 TEXTURE_SCALE = new Vector2(1,-1);
        static readonly Vector2 TEXTURE_OFFSET = new Vector2(0,1);

        Shader pbrMetallicRoughnessShader;
        Shader pbrSpecularGlossinessShader;
        Shader unlitShader;

        public UnityEngine.Material GetPbrMetallicRoughnessMaterial() {
            if(pbrMetallicRoughnessShader==null) {
                pbrMetallicRoughnessShader = Shader.Find("glTF/PbrMetallicRoughness");
            }
            return new Material(pbrMetallicRoughnessShader);
        }

        public UnityEngine.Material GetPbrSpecularGlossinessMaterial() {
            if(pbrSpecularGlossinessShader==null) {
                pbrSpecularGlossinessShader = Shader.Find("GLTF/PbrSpecularGlossiness");
            }
            return new Material(pbrSpecularGlossinessShader);
        }

        public UnityEngine.Material GetUnlitMaterial() {
            if(unlitShader==null) {
                unlitShader = Shader.Find("Unlit/Color");
            }
            return new Material(unlitShader);
        }

        public UnityEngine.Material GenerateMaterial( Schema.Material gltfMaterial, Schema.Texture[] textures, Texture2D[] images, List<UnityEngine.Object> additionalResources ) {
            UnityEngine.Material material;
            
            if (gltfMaterial.extensions!=null && gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness!=null) {
                material = GetPbrSpecularGlossinessMaterial();
            } else
            if (gltfMaterial.extensions.KHR_materials_unlit!=null) {
                material = GetUnlitMaterial();
            } else {
                material = GetPbrMetallicRoughnessMaterial();
            }

            material.name = gltfMaterial.name;

            material.mainTextureScale = TEXTURE_SCALE;
            material.mainTextureOffset = TEXTURE_OFFSET;

            //added support for KHR_materials_pbrSpecularGlossiness
            if (gltfMaterial.extensions != null) {
                Schema.PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null) {
                    var diffuseTexture = GetTexture(specGloss.diffuseTexture, textures, images);
                    if (diffuseTexture != null) {   
                        material.mainTexture = diffuseTexture;
                    }
                    else {
                        material.color = specGloss.diffuseColor;
                    }
                    var specGlossTexture = GetTexture(specGloss.specularGlossinessTexture, textures, images);
                    if (specGlossTexture != null) {
                        material.SetTexture(StandardShaderHelper.specGlossMapPropId, specGlossTexture);
                        material.EnableKeyword("_SPECGLOSSMAP");
                    }
                    else {
                        material.SetVector(StandardShaderHelper.specColorPropId, specGloss.specularColor);
                        material.SetFloat(StandardShaderHelper.glossinessPropId, (float)specGloss.glossinessFactor);
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness!=null) {
                material.color = gltfMaterial.pbrMetallicRoughness.baseColor;
                material.SetFloat(StandardShaderHelper.metallicPropId, gltfMaterial.pbrMetallicRoughness.metallicFactor );
                material.SetFloat(StandardShaderHelper.glossinessPropId, 1-gltfMaterial.pbrMetallicRoughness.roughnessFactor );

                var mainTxt = GetTexture(gltfMaterial.pbrMetallicRoughness.baseColorTexture,textures,images);
                if(mainTxt!=null) {
                    material.mainTexture = mainTxt;
                }

                var metallicRoughnessTxt = GetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,textures,images);
                if(metallicRoughnessTxt!=null) {
                    material.SetTexture( StandardShaderHelper.metallicGlossMapPropId, metallicRoughnessTxt );
                    material.EnableKeyword(StandardShaderHelper.KW_METALLIC_ROUGNESS_MAP);
                    additionalResources.Add(metallicRoughnessTxt);
                }
            }

            var normalTxt = GetTexture(gltfMaterial.normalTexture,textures,images);
            if(normalTxt!=null) {
                material.SetTexture( StandardShaderHelper.bumpMapPropId, normalTxt);
                material.EnableKeyword("_NORMALMAP");
            }
            
            var occlusionTxt = GetTexture(gltfMaterial.occlusionTexture,textures,images);
            if(occlusionTxt !=null) {
                material.SetTexture(StandardShaderHelper.occlusionMapPropId, occlusionTxt);
                material.EnableKeyword(StandardShaderHelper.KW_OCCLUSION);
                additionalResources.Add(occlusionTxt);
            }
            
            var emmissiveTxt = GetTexture(gltfMaterial.emissiveTexture,textures,images);
            if(emmissiveTxt!=null) {
                material.SetTexture( StandardShaderHelper.emissionMapPropId, emmissiveTxt);
                material.EnableKeyword("_EMISSION");
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
                material.SetColor("_EmissionColor", gltfMaterial.emissive);
                material.EnableKeyword("_EMISSION");
            }

            if(gltfMaterial.doubleSided) {
                Debug.LogWarning("Double sided shading is not supported!");
            }
            return material;
        }

        static Texture2D GetTexture(Schema.TextureInfo textureInfo, Schema.Texture[] textures, Texture2D[] images )
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int bcTextureIndex = textureInfo.index;
                if (textures != null && textures.Length > bcTextureIndex)
                {
                    var txt = textures[bcTextureIndex];
                    if (images != null && images.Length > txt.source)
                    {
                        return images[txt.source];
                    }
                    else
                    {
                        Debug.LogErrorFormat("Image #{0} not found", txt.source);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Texture #{0} not found", bcTextureIndex);
                }
            }
            return null;
        }
    }
}
