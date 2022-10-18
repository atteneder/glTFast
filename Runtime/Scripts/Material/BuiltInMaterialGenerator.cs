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

#if ! ( USING_URP || USING_HDRP || (UNITY_SHADER_GRAPH_12_OR_NEWER && GLTFAST_BUILTIN_SHADER_GRAPH) )
#define GLTFAST_BUILTIN_RP
#endif

#if GLTFAST_BUILTIN_RP || UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using Material = UnityEngine.Material;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GLTFast.Materials {

    using Logging;
    using AlphaMode = Schema.Material.AlphaMode;

    /// <summary>
    /// Built-In render pipeline Standard shader modes
    /// </summary>
    public enum StandardShaderMode {
        /// <summary>
        /// Opaque mode
        /// </summary>
        Opaque = 0,
        /// <summary>
        /// Cutout mode (alpha test)
        /// </summary>
        Cutout = 1,
        /// <summary>
        /// Fade mode (alpha blended opacity)
        /// </summary>
        Fade = 2,
        /// <summary>
        /// Transparent mode (alpha blended transmission; e.g. glass)
        /// </summary>
        Transparent = 3
    }
    
    /// <summary>
    /// Converts glTF materials to Unity materials for the Built-in Render Pipeline
    /// </summary>
    public class BuiltInMaterialGenerator : MaterialGenerator {

        // Built-in Render Pipeline
        const string KW_ALPHABLEND_ON = "_ALPHABLEND_ON";
        const string KW_ALPHAPREMULTIPLY_ON = "_ALPHAPREMULTIPLY_ON";
        const string KW_EMISSION = "_EMISSION";
        const string KW_METALLIC_ROUGNESS_MAP = "_METALLICGLOSSMAP";
        const string KW_OCCLUSION = "_OCCLUSION";
        const string KW_SPEC_GLOSS_MAP = "_SPECGLOSSMAP";

        static readonly int modePropId = Shader.PropertyToID("_Mode");

#if UNITY_EDITOR
        const string SHADER_PATH_PREFIX = "Packages/com.atteneder.gltfast/Runtime/Shader/Built-In/";
        const string SHADER_PATH_PBR_METALLIC_ROUGHNESS = "glTFPbrMetallicRoughness.shader";
        const string SHADER_PATH_PBR_SPECULAR_GLOSSINESS = "glTFPbrSpecularGlossiness.shader";
        const string SHADER_PATH_UNLIT = "glTFUnlit.shader";
#else
        const string SHADER_PBR_METALLIC_ROUGHNESS = "glTF/PbrMetallicRoughness";
        const string SHADER_PBR_SPECULAR_GLOSSINESS = "glTF/PbrSpecularGlossiness";
        const string SHADER_UNLIT = "glTF/Unlit";
#endif

        Shader pbrMetallicRoughnessShader;
        Shader pbrSpecularGlossinessShader;
        Shader unlitShader;

        static bool defaultMaterialGenerated;
        static Material defaultMaterial;

        /// <inheritdoc />
        protected override Material GenerateDefaultMaterial(bool pointsSupport = false) {
            if(pointsSupport) {
                logger?.Warning(LogCode.TopologyPointsMaterialUnsupported);
            }
            if (!defaultMaterialGenerated) {
                defaultMaterial = GetPbrMetallicRoughnessMaterial();
                defaultMaterialGenerated = true;
                // Material works on lines as well
                // TODO: Create dedicated point cloud material
            }

            return defaultMaterial;
        }
        
        /// <summary>
        /// Finds the shader required for metallic/roughness based materials.
        /// </summary>
        /// <returns>Metallic/Roughness shader</returns>
        protected virtual Shader FinderShaderMetallicRoughness() {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{SHADER_PATH_PREFIX}{SHADER_PATH_PBR_METALLIC_ROUGHNESS}");
#else
            return FindShader(SHADER_PBR_METALLIC_ROUGHNESS);
#endif
        }
        
        /// <summary>
        /// Finds the shader required for specular/glossiness based materials.
        /// </summary>
        /// <returns>Specular/Glossiness shader</returns>
        protected virtual Shader FinderShaderSpecularGlossiness() {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{SHADER_PATH_PREFIX}{SHADER_PATH_PBR_SPECULAR_GLOSSINESS}");
#else
            return FindShader(SHADER_PBR_SPECULAR_GLOSSINESS);
#endif
        }

        /// <summary>
        /// Finds the shader required for unlit materials.
        /// </summary>
        /// <returns>Unlit shader</returns>
        protected virtual Shader FinderShaderUnlit() {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{SHADER_PATH_PREFIX}{SHADER_PATH_UNLIT}");
#else
            return FindShader(SHADER_UNLIT);
#endif
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

        /// <inheritdoc />
        public override Material GenerateMaterial(
            Schema.Material gltfMaterial,
            IGltfReadable gltf,
            bool pointsSupport = false
        ) {
            Material material;

            var isUnlit = gltfMaterial.extensions?.KHR_materials_unlit != null;
            
            if (gltfMaterial.extensions?.KHR_materials_pbrSpecularGlossiness != null) {
                material = GetPbrSpecularGlossinessMaterial(gltfMaterial.doubleSided);
            } else
            if (isUnlit) {
                material = GetUnlitMaterial(gltfMaterial.doubleSided);
            } else {
                material = GetPbrMetallicRoughnessMaterial(gltfMaterial.doubleSided);
            }

            if(material==null) return null;

            if (!isUnlit && pointsSupport) {
                logger?.Warning(LogCode.TopologyPointsMaterialUnsupported);
            }
            
            material.name = gltfMaterial.name;

            StandardShaderMode shaderMode = StandardShaderMode.Opaque;
            Color baseColorLinear = Color.white;

            if(gltfMaterial.alphaModeEnum == AlphaMode.MASK) {
                material.SetFloat(alphaCutoffPropId, gltfMaterial.alphaCutoff);
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
                    material.SetVector(specularFactorPropId, specGloss.specularColor);
                    material.SetFloat(glossinessFactorPropId,specGloss.glossinessFactor);

                    TrySetTexture(
                        specGloss.diffuseTexture,
                        material,
                        gltf,
                        baseColorTexturePropId,
                        baseColorTextureScaleTransformPropId,
                        baseColorTextureRotationPropId,
                        baseColorTextureTexCoordPropId
                        );

                    if (TrySetTexture(
                        specGloss.specularGlossinessTexture,
                        material,
                        gltf,
                        specularGlossinessTexturePropId,
                        specularGlossinessTextureScaleTransformPropId,
                        specularGlossinessTextureRotationPropId,
                        specularGlossinessTextureTexCoordPropId
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
                material.SetFloat(roughnessFactorPropId, gltfMaterial.pbrMetallicRoughness.roughnessFactor );

                TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                    material,
                    gltf,
                    baseColorTexturePropId,
                    baseColorTextureScaleTransformPropId,
                    baseColorTextureRotationPropId,
                    baseColorTextureTexCoordPropId
                    );
                
                if(TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,
                    material,
                    gltf,
                    metallicRoughnessMapPropId,
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
                normalTexturePropId,
                normalTextureScaleTransformPropId,
                normalTextureRotationPropId,
                normalTextureTexCoordPropId
            )) {
                material.EnableKeyword(Constants.kwNormalMap);
                material.SetFloat(normalTextureScalePropId,gltfMaterial.normalTexture.scale);
            }

            if(TrySetTexture(
                gltfMaterial.occlusionTexture,
                material,
                gltf,
                occlusionTexturePropId,
                occlusionTextureScaleTransformPropId,
                occlusionTextureRotationPropId,
                occlusionTextureTexCoordPropId
                )) {
                material.EnableKeyword(KW_OCCLUSION);
                material.SetFloat(occlusionTextureStrengthPropId,gltfMaterial.occlusionTexture.strength);
            }

            if(TrySetTexture(
                gltfMaterial.emissiveTexture,
                material,
                gltf,
                emissiveTexturePropId,
                emissiveTextureScaleTransformPropId,
                emissiveTextureRotationPropId,
                emissiveTextureTexCoordPropId
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

            material.SetVector(baseColorPropId, baseColorLinear.gamma);
            
            if(gltfMaterial.emissive != Color.black) {
                material.SetColor(emissiveFactorPropId, gltfMaterial.emissive.gamma);
                material.EnableKeyword(KW_EMISSION);
            }

            return material;
        }

        /// <summary>
        /// Configures material for alpha masking.
        /// </summary>
        /// <param name="material">Target material</param>
        /// <param name="alphaCutoff">Threshold value for alpha masking</param>
        public static void SetAlphaModeMask(UnityEngine.Material material, float alphaCutoff)
        {
            material.EnableKeyword(KW_ALPHATEST_ON);
            material.SetInt(zWritePropId, 1);
            material.DisableKeyword(KW_ALPHAPREMULTIPLY_ON);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;  //2450
            material.SetFloat(alphaCutoffPropId, alphaCutoff);
            material.SetFloat(modePropId, (int)StandardShaderMode.Cutout);
            material.SetOverrideTag(TAG_RENDER_TYPE, TAG_RENDER_TYPE_CUTOUT);
            material.SetInt(srcBlendPropId, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(dstBlendPropId, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.DisableKeyword(KW_ALPHABLEND_ON);
        }

        /// <summary>
        /// Configures material for alpha masking.
        /// </summary>
        /// <param name="material">Target material</param>
        /// <param name="gltfMaterial">Source material</param>
        public static void SetAlphaModeMask(UnityEngine.Material material, Schema.Material gltfMaterial)
        {
            SetAlphaModeMask(material, gltfMaterial.alphaCutoff);
        }

        /// <summary>
        /// Configures material for alpha blending.
        /// </summary>
        /// <param name="material">Target material</param>
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

        /// <summary>
        /// Configures material for transparency.
        /// </summary>
        /// <param name="material">Target material</param>
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

        /// <summary>
        /// Configures material to be opaque.
        /// </summary>
        /// <param name="material">Target material</param>
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
