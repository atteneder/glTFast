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
        Shader pbrMetallicRoughnessDoubleSideShader;
        Shader pbrSpecularGlossinessShader;
        Shader pbrSpecularGlossinessDoubleSideShader;
        Shader unlitShader;

        public UnityEngine.Material GetPbrMetallicRoughnessMaterial(bool doubleSided=false) {
            if(doubleSided) {
                if(pbrMetallicRoughnessDoubleSideShader==null) {
                    pbrMetallicRoughnessDoubleSideShader = Shader.Find("glTF/PbrMetallicRoughnessDouble");
                }
                return new Material(pbrMetallicRoughnessDoubleSideShader);
            } else {
                if(pbrMetallicRoughnessShader==null) {
                    pbrMetallicRoughnessShader = Shader.Find("glTF/PbrMetallicRoughness");
                }
                return new Material(pbrMetallicRoughnessShader);
            }
        }

        public UnityEngine.Material GetPbrSpecularGlossinessMaterial(bool doubleSided=false) {
            if(doubleSided) {
                if(pbrSpecularGlossinessDoubleSideShader==null) {
                    pbrSpecularGlossinessDoubleSideShader = Shader.Find("glTF/PbrSpecularGlossinessDouble");
                }
                return new Material(pbrSpecularGlossinessDoubleSideShader);
            } else {
                if(pbrSpecularGlossinessShader==null) {
                    pbrSpecularGlossinessShader = Shader.Find("glTF/PbrSpecularGlossiness");
                }
                return new Material(pbrSpecularGlossinessShader);
            }
        }

        public UnityEngine.Material GetUnlitMaterial(bool doubleSided=false) {
            if(unlitShader==null) {
                unlitShader = Shader.Find("glTF/Unlit");
            }
            var mat = new Material(unlitShader);
            if(doubleSided) {
                // Turn of back-face culling
                mat.SetFloat(StandardShaderHelper.doubleSidedPropId,0);
            }
            return mat;
        }

        public UnityEngine.Material GenerateMaterial( Schema.Material gltfMaterial, Schema.Texture[] textures, Texture2D[] images ) {
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

            if(material.HasProperty(StandardShaderHelper.KW_MAIN_MAP)) {
                // Initialize texture transform
                material.mainTextureScale = TEXTURE_SCALE;
                material.mainTextureOffset = TEXTURE_OFFSET;
            }

            //added support for KHR_materials_pbrSpecularGlossiness
            if (gltfMaterial.extensions != null) {
                Schema.PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null) {
                    material.color = specGloss.diffuseColor;
                    material.SetVector(StandardShaderHelper.specColorPropId, specGloss.specularColor);
                    material.SetFloat(StandardShaderHelper.glossinessPropId,specGloss.glossinessFactor);

                    TrySetTexture(specGloss.diffuseTexture,material,StandardShaderHelper.mainTexPropId,textures,images);

                    if (TrySetTexture(specGloss.specularGlossinessTexture,material,StandardShaderHelper.specGlossMapPropId,textures,images)) {
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
                    textures,
                    images
                    );
                
                if(TrySetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,material,StandardShaderHelper.metallicGlossMapPropId,textures,images)) {
                    material.EnableKeyword(StandardShaderHelper.KW_METALLIC_ROUGNESS_MAP);
                }
            }

            if(TrySetTexture(gltfMaterial.normalTexture,material,StandardShaderHelper.bumpMapPropId,textures,images)) {
                material.EnableKeyword("_NORMALMAP");
            }

            if(TrySetTexture(gltfMaterial.occlusionTexture,material,StandardShaderHelper.occlusionMapPropId,textures,images)) {
                material.EnableKeyword(StandardShaderHelper.KW_OCCLUSION);
            }

            if(TrySetTexture(gltfMaterial.emissiveTexture,material,StandardShaderHelper.emissionMapPropId,textures,images)) {
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
            Schema.Texture[] textures,
            Texture2D[] images
            )
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int bcTextureIndex = textureInfo.index;
                if (textures != null && textures.Length > bcTextureIndex)
                {
                    var txt = textures[bcTextureIndex];
                    if (images != null && images.Length > txt.source)
                    {
                        material.SetTexture(propertyId,images[txt.source]);
                        TrySetTextureTransform(textureInfo,material,propertyId);
                        return true;
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
            return false;
        }

        static void TrySetTextureTransform(
            Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int propertyId
            )
        {
            if(textureInfo.extensions != null && textureInfo.extensions.KHR_texture_transform!=null) {
                var tt = textureInfo.extensions.KHR_texture_transform;
                if(tt.texCoord!=0) {
                    Debug.LogError("Multiple UV sets are not supported!");
                }
                if(tt.offset!=null) {
                    material.SetTextureOffset(propertyId,new Vector2(tt.offset[0],1-tt.offset[1]));
                }
                if(tt.rotation!=0) {
                    float cos = Mathf.Cos(tt.rotation);
                    float sin = Mathf.Sin(tt.rotation);
                    material.SetVector(StandardShaderHelper.mainTexRotatePropId,new Vector4(cos,-sin,sin,cos));
                    material.EnableKeyword(StandardShaderHelper.KW_UV_ROTATION);
                }
                if(tt.scale!=null) {
                    material.SetTextureScale(propertyId,new Vector2(tt.scale[0],-tt.scale[1]));
                }
            }
        }
    }
}
