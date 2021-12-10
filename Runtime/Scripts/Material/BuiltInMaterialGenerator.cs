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

#if ! ( USING_URP || USING_HDRP )
#define GLTFAST_BUILTIN_RP
#endif

#if GLTFAST_BUILTIN_RP || UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Material = UnityEngine.Material;

namespace GLTFast.Materials {

    using AlphaMode = Schema.Material.AlphaMode;

    public enum StandardShaderMode {
        Opaque = 0,
        Cutout = 1,
        Fade = 2,
        Transparent = 3
    }
    
    public class BuiltInMaterialGenerator : MaterialGenerator {

        // Built-in Render Pipeline
        public const string KW_ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";
        public const string KW_EMISSION = "_EMISSION";
        public const string KW_METALLIC_ROUGNESS_MAP = "_METALLICGLOSSMAP";
        public const string KW_OCCLUSION = "_OCCLUSION";        
        public const string KW_SPEC_GLOSS_MAP = "_SPECGLOSSMAP";

        const string KW_ALPHABLEND_ON = "_ALPHABLEND_ON";
        
        public static readonly int glossinessPropId = Shader.PropertyToID("_Glossiness");
        public static readonly int metallicGlossMapPropId = Shader.PropertyToID("_MetallicGlossMap");
        public static readonly int roughnessPropId = Shader.PropertyToID("_Roughness");
        public static readonly int zWritePropId = Shader.PropertyToID("_ZWrite");

        static readonly int metallicRoughnessMapScaleTransformPropId = Shader.PropertyToID("_MetallicGlossMap_ST");
        static readonly int metallicRoughnessMapRotationPropId = Shader.PropertyToID("_MetallicGlossMapRotation");
        static readonly int metallicRoughnessMapUVChannelPropId = Shader.PropertyToID("_MetallicGlossMapUVChannel");
        static readonly int modePropId = Shader.PropertyToID("_Mode");

        const string SHADER_PBR_METALLIC_ROUGHNESS = "glTF/PbrMetallicRoughness";
        const string SHADER_PBR_SPECULAR_GLOSSINESS = "glTF/PbrSpecularGlossiness";
        const string SHADER_UNLIT = "glTF/Unlit";
        
        Shader pbrMetallicRoughnessShader;
        Shader pbrSpecularGlossinessShader;
        Shader unlitShader;

        public override Material GetDefaultMaterial() {
            return GetPbrMetallicRoughnessMaterial();
        }
        
        protected virtual Shader FinderShaderMetallicRoughness() {
            return FindShader(SHADER_PBR_METALLIC_ROUGHNESS);
        }
        
        protected virtual Shader FinderShaderSpecularGlossiness() {
            return FindShader(SHADER_PBR_SPECULAR_GLOSSINESS);
        }

        protected virtual Shader FinderShaderUnlit() {
            return FindShader(SHADER_UNLIT);
        }

        Material GetPbrMetallicRoughnessMaterial(bool doubleSided=false) {
            if(pbrMetallicRoughnessShader==null)
            {
                pbrMetallicRoughnessShader = FinderShaderMetallicRoughness();
            }
            if(pbrMetallicRoughnessShader==null) {
                return null;
            }
            var mat = new Material(pbrMetallicRoughnessShader);
            if(doubleSided) {
                // Turn off back-face culling
                mat.SetFloat(cullModePropId,0);
#if UNITY_EDITOR
                mat.doubleSidedGI = true;
#endif
            }
            return mat;
        }

        Material GetPbrSpecularGlossinessMaterial(bool doubleSided=false) {
            if(pbrSpecularGlossinessShader==null) {
                pbrSpecularGlossinessShader = FinderShaderSpecularGlossiness();
            }
            if(pbrSpecularGlossinessShader==null) {
                return null;
            }
            var mat = new Material(pbrSpecularGlossinessShader);
            if(doubleSided) {
                // Turn off back-face culling
                mat.SetFloat(cullModePropId,0);
#if UNITY_EDITOR
                mat.doubleSidedGI = true;
#endif
            }
            return mat;
        }

        Material GetUnlitMaterial(bool doubleSided=false) {
            if(unlitShader==null) {
                unlitShader = FinderShaderUnlit();
            }
            if(unlitShader==null) {
                return null;
            }
            var mat = new Material(unlitShader);
            if(doubleSided) {
                // Turn off back-face culling
                mat.SetFloat(cullModePropId,0);
#if UNITY_EDITOR
                mat.doubleSidedGI = true;
#endif
            }
            return mat;
        }

        public override Material GenerateMaterial(
            Schema.Material gltfMaterial,
            IGltfReadable gltf
        ) {
            Material material;
            
            if (gltfMaterial.extensions?.KHR_materials_pbrSpecularGlossiness != null) {
                material = GetPbrSpecularGlossinessMaterial(gltfMaterial.doubleSided);
            } else
            if (gltfMaterial.extensions?.KHR_materials_unlit!=null) {
                material = GetUnlitMaterial(gltfMaterial.doubleSided);
            } else {
                material = GetPbrMetallicRoughnessMaterial(gltfMaterial.doubleSided);
            }

            if(material==null) return null;

            material.name = gltfMaterial.name;

            StandardShaderMode shaderMode = StandardShaderMode.Opaque;
            Color baseColorLinear = Color.white;

            if(gltfMaterial.alphaModeEnum == AlphaMode.MASK) {
                material.SetFloat(cutoffPropId, gltfMaterial.alphaCutoff);
                shaderMode = StandardShaderMode.Cutout;
            } else if(gltfMaterial.alphaModeEnum == AlphaMode.BLEND) {
                SetAlphaModeBlend( material );
                shaderMode = StandardShaderMode.Fade;
            }

            if (gltfMaterial.extensions != null) {
                // Specular glossiness
                Schema.PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null) {
                    baseColorLinear = specGloss.diffuseColor;
                    material.SetVector(specColorPropId, specGloss.specularColor);
                    material.SetFloat(glossinessPropId,specGloss.glossinessFactor);

                    TrySetTexture(
                        specGloss.diffuseTexture,
                        material,
                        gltf,
                        mainTexPropId,
                        mainTexScaleTransform,
                        mainTexRotation,
                        mainTexUVChannelPropId
                        );

                    if (TrySetTexture(
                        specGloss.specularGlossinessTexture,
                        material,
                        gltf,
                        specGlossMapPropId,
                        specGlossScaleTransformMapPropId,
                        specGlossMapRotationPropId,
                        specGlossMapUVChannelPropId
                        )) {
                        material.EnableKeyword(KW_SPEC_GLOSS_MAP);
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness!=null
                // If there's a specular-glossiness extension, ignore metallic-roughness
                // (according to extension specification)
                && gltfMaterial.extensions?.KHR_materials_pbrSpecularGlossiness == null)
            {
                baseColorLinear = gltfMaterial.pbrMetallicRoughness.baseColor;
                material.SetFloat(metallicPropId, gltfMaterial.pbrMetallicRoughness.metallicFactor );
                material.SetFloat(roughnessPropId, gltfMaterial.pbrMetallicRoughness.roughnessFactor );

                TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                    material,
                    gltf,
                    mainTexPropId,
                    mainTexScaleTransform,
                    mainTexRotation,
                    mainTexUVChannelPropId
                    );
                
                if(TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,
                    material,
                    gltf,
                    metallicGlossMapPropId,
                    metallicRoughnessMapScaleTransformPropId,
                    metallicRoughnessMapRotationPropId,
                    metallicRoughnessMapUVChannelPropId
                    )) {
                    material.EnableKeyword(KW_METALLIC_ROUGNESS_MAP);
                }
            }

            if(TrySetTexture(
                gltfMaterial.normalTexture,
                material,
                gltf,
                bumpMapPropId,
                bumpMapScaleTransformPropId,
                bumpMapRotationPropId,
                bumpMapUVChannelPropId
            )) {
                material.EnableKeyword(Constants.kwNormalMap);
                material.SetFloat(bumpScalePropId,gltfMaterial.normalTexture.scale);
            }

            if(TrySetTexture(
                gltfMaterial.occlusionTexture,
                material,
                gltf,
                occlusionMapPropId,
                occlusionMapScaleTransformPropId,
                occlusionMapRotationPropId,
                occlusionMapUVChannelPropId
                )) {
                material.EnableKeyword(KW_OCCLUSION);
                material.SetFloat(occlusionStrengthPropId,gltfMaterial.occlusionTexture.strength);
            }

            if(TrySetTexture(
                gltfMaterial.emissiveTexture,
                material,
                gltf,
                emissionMapPropId,
                emissionMapScaleTransformPropId,
                emissionMapRotationPropId,
                emissionMapUVChannelPropId
                )) {
                material.EnableKeyword(KW_EMISSION);
            }

            if (gltfMaterial.extensions != null) {

                // Transmission - Approximation
                var transmission = gltfMaterial.extensions.KHR_materials_transmission;
                if (transmission != null) {
#if UNITY_EDITOR
                    logger?.Warning(LogCode.MaterialTransmissionApprox);
#endif
                    // Correct transmission is not supported in Built-In renderer
                    // This is an approximation for some corner cases
                    if (transmission.transmissionFactor > 0f && transmission.transmissionTexture.index < 0) {
                        var premul = TransmissionWorkaroundShaderMode(transmission, ref baseColorLinear);
                        shaderMode = premul ? StandardShaderMode.Transparent : StandardShaderMode.Fade;
                    }
                }
            }
            
            switch (shaderMode)
            {
                case StandardShaderMode.Cutout:
                    SetAlphaModeMask( material, gltfMaterial);
                    break;
                case StandardShaderMode.Fade:
                    SetAlphaModeBlend( material );
                    break;
                case StandardShaderMode.Transparent:
                    SetAlphaModeTransparent( material );
                    break;
                default:
                    SetOpaqueMode(material);
                    break;
            }

            material.color = baseColorLinear.gamma;
            
            if(gltfMaterial.emissive != Color.black) {
                material.SetColor(emissionColorPropId, gltfMaterial.emissive.gamma);
                material.EnableKeyword(KW_EMISSION);
            }

            return material;
        }
        
        public static void SetAlphaModeMask(UnityEngine.Material material, float alphaCutoff)
        {
            material.EnableKeyword(KW_ALPHATEST_ON);
            material.SetInt(zWritePropId, 1);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;  //2450
            material.SetFloat(cutoffPropId, alphaCutoff);
            material.SetFloat(modePropId, (int)StandardShaderMode.Cutout);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_CUTOUT);
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.DisableKeyword(KW_ALPHABLEND_ON);
        }

        public static void SetAlphaModeMask(UnityEngine.Material material, Schema.Material gltfMaterial)
        {
            SetAlphaModeMask(material, gltfMaterial.alphaCutoff);
        }

        public static void SetAlphaModeBlend( UnityEngine.Material material ) {
            material.SetFloat(modePropId, (int)StandardShaderMode.Fade);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_FADE);
            material.EnableKeyword(KW_ALPHABLEND_ON);
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);//5
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(zWritePropId, 0);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
        }

        public static void SetAlphaModeTransparent( UnityEngine.Material material ) {
            material.SetFloat(modePropId, (int)StandardShaderMode.Fade);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_TRANSPARENT);
            material.EnableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);//1
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(zWritePropId, 0);
            material.DisableKeyword(KW_ALPHABLEND_ON);
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
        }

        public static void SetOpaqueMode(UnityEngine.Material material) {
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_OPAQUE);
            material.DisableKeyword(KW_ALPHABLEND_ON);
            material.renderQueue = -1;
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(zWritePropId, 1);
            material.DisableKeyword(KW_ALPHATEST_ON);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
        }
    }
}
#endif // GLTFAST_BUILTIN_RP || UNITY_EDITOR
