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
            ref UnityEngine.Texture2D[] images
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
                    material.color = specGloss.diffuseColor;
                    material.SetVector(StandardShaderHelper.specColorPropId, specGloss.specularColor);
                    material.SetFloat(StandardShaderHelper.glossinessPropId,specGloss.glossinessFactor);

                    TrySetTexture(specGloss.diffuseTexture,material,StandardShaderHelper.mainTexPropId,ref textures,ref schemaImages,ref images);

                    if (TrySetTexture(specGloss.specularGlossinessTexture,material,StandardShaderHelper.specGlossMapPropId,ref textures,ref schemaImages,ref images)) {
                        material.EnableKeyword(StandardShaderHelper.KW_SPEC_GLOSS_MAP);
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness!=null) {
                material.color = gltfMaterial.pbrMetallicRoughness.baseColor;
                material.SetFloat(StandardShaderHelper.metallicPropId, gltfMaterial.pbrMetallicRoughness.metallicFactor );
                material.SetFloat(StandardShaderHelper.roughnessPropId, gltfMaterial.pbrMetallicRoughness.roughnessFactor );

                TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                    material,
                    StandardShaderHelper.mainTexPropId,
                    ref textures,
                    ref schemaImages,
                    ref images
                    );
                
                if(TrySetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,material,StandardShaderHelper.metallicGlossMapPropId,ref textures,ref schemaImages,ref images)) {
                    material.EnableKeyword(StandardShaderHelper.KW_METALLIC_ROUGNESS_MAP);
                }
            }

            if(TrySetTexture(gltfMaterial.normalTexture,material,StandardShaderHelper.bumpMapPropId,ref textures,ref schemaImages,ref images)) {
                material.EnableKeyword("_NORMALMAP");
            }

            if(TrySetTexture(gltfMaterial.occlusionTexture,material,StandardShaderHelper.occlusionMapPropId,ref textures,ref schemaImages,ref images)) {
                material.EnableKeyword(StandardShaderHelper.KW_OCCLUSION);
            }

            if(TrySetTexture(gltfMaterial.emissiveTexture,material,StandardShaderHelper.emissionMapPropId,ref textures,ref schemaImages,ref images)) {
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
                material.SetColor("_EmissionColor", gltfMaterial.emissive);
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
            ref Texture2D[] images
            )
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int bcTextureIndex = textureInfo.index;
                if (textures != null && textures.Length > bcTextureIndex)
                {
                    var txt = textures[bcTextureIndex];
                    var imageIndex = txt.source;

                    if(txt.extensions!=null) {
                        if (txt.extensions.KHR_texture_basisu!=null) {
                            imageIndex = txt.extensions.KHR_texture_basisu.source;
                        }
                    }

                    if (images != null && imageIndex >= 0 && images.Length > imageIndex)
                    {
                        material.SetTexture(propertyId,images[imageIndex]);
                        var isKtx = schemaImages[imageIndex].isKtx;
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
