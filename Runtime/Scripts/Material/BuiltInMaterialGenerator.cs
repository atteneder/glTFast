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

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Material = UnityEngine.Material;

namespace GLTFast.Materials
{

    using Logging;
    using AlphaMode = Schema.Material.AlphaMode;

    /// <summary>
    /// Built-In render pipeline Standard shader modes
    /// </summary>
    public enum StandardShaderMode
    {
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
    public class BuiltInMaterialGenerator : MaterialGenerator
    {

        // Built-in Render Pipeline
        const string k_AlphaBlendOnKeyword = "_ALPHABLEND_ON";
        const string k_AlphaPremultiplyOnKeyword = "_ALPHAPREMULTIPLY_ON";
        const string k_EmissionKeyword = "_EMISSION";
        const string k_MetallicRoughnessMapKeyword = "_METALLICGLOSSMAP";
        const string k_OcclusionKeyword = "_OCCLUSION";
        const string k_SpecGlossMapKeyword = "_SPECGLOSSMAP";

        static readonly int k_ModePropId = Shader.PropertyToID("_Mode");

#if UNITY_EDITOR
        const string k_ShaderPathPrefix = "Packages/com.atteneder.gltfast/Runtime/Shader/Built-In/";
        const string k_PbrMetallicRoughnessShaderPath = "glTFPbrMetallicRoughness.shader";
        const string k_PbrSpecularGlossinessShaderPath = "glTFPbrSpecularGlossiness.shader";
        const string k_UnlitShaderPath = "glTFUnlit.shader";
#else
        const string k_PbrMetallicRoughnessShaderName = "glTF/PbrMetallicRoughness";
        const string k_PbrSpecularGlossinessShaderName = "glTF/PbrSpecularGlossiness";
        const string k_UnlitShaderName = "glTF/Unlit";
#endif

        Shader m_PbrMetallicRoughnessShader;
        Shader m_PbrSpecularGlossinessShader;
        Shader m_UnlitShader;

        static bool s_DefaultMaterialGenerated;
        static Material s_DefaultMaterial;

        /// <inheritdoc />
        protected override Material GenerateDefaultMaterial(bool pointsSupport = false)
        {
            if (pointsSupport)
            {
                Logger?.Warning(LogCode.TopologyPointsMaterialUnsupported);
            }
            if (!s_DefaultMaterialGenerated)
            {
                s_DefaultMaterial = GetPbrMetallicRoughnessMaterial();
                s_DefaultMaterial.name = DefaultMaterialName;
                s_DefaultMaterialGenerated = true;
                // Material works on lines as well
                // TODO: Create dedicated point cloud material
            }

            return s_DefaultMaterial;
        }

        /// <summary>
        /// Finds the shader required for metallic/roughness based materials.
        /// </summary>
        /// <returns>Metallic/Roughness shader</returns>
        Shader FinderShaderMetallicRoughness()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{k_ShaderPathPrefix}{k_PbrMetallicRoughnessShaderPath}");
#else
            return FindShader(k_PbrMetallicRoughnessShaderName, Logger);
#endif
        }

        /// <summary>
        /// Finds the shader required for specular/glossiness based materials.
        /// </summary>
        /// <returns>Specular/Glossiness shader</returns>
        Shader FinderShaderSpecularGlossiness()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{k_ShaderPathPrefix}{k_PbrSpecularGlossinessShaderPath}");
#else
            return FindShader(k_PbrSpecularGlossinessShaderName, Logger);
#endif
        }

        /// <summary>
        /// Finds the shader required for unlit materials.
        /// </summary>
        /// <returns>Unlit shader</returns>
        Shader FinderShaderUnlit()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{k_ShaderPathPrefix}{k_UnlitShaderPath}");
#else
            return FindShader(k_UnlitShaderName, Logger);
#endif
        }

        Material GetPbrMetallicRoughnessMaterial(bool doubleSided = false)
        {
            if (m_PbrMetallicRoughnessShader == null)
            {
                m_PbrMetallicRoughnessShader = FinderShaderMetallicRoughness();
            }
            if (m_PbrMetallicRoughnessShader == null)
            {
                return null;
            }
            var mat = new Material(m_PbrMetallicRoughnessShader);
            if (doubleSided)
            {
                // Turn off back-face culling
                mat.SetFloat(CullModeProperty, 0);
#if UNITY_EDITOR
                mat.doubleSidedGI = true;
#endif
            }
            return mat;
        }

        Material GetPbrSpecularGlossinessMaterial(bool doubleSided = false)
        {
            if (m_PbrSpecularGlossinessShader == null)
            {
                m_PbrSpecularGlossinessShader = FinderShaderSpecularGlossiness();
            }
            if (m_PbrSpecularGlossinessShader == null)
            {
                return null;
            }
            var mat = new Material(m_PbrSpecularGlossinessShader);
            if (doubleSided)
            {
                // Turn off back-face culling
                mat.SetFloat(CullModeProperty, 0);
#if UNITY_EDITOR
                mat.doubleSidedGI = true;
#endif
            }
            return mat;
        }

        Material GetUnlitMaterial(bool doubleSided = false)
        {
            if (m_UnlitShader == null)
            {
                m_UnlitShader = FinderShaderUnlit();
            }
            if (m_UnlitShader == null)
            {
                return null;
            }
            var mat = new Material(m_UnlitShader);
            if (doubleSided)
            {
                // Turn off back-face culling
                mat.SetFloat(CullModeProperty, 0);
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
        )
        {
            Material material;

            var isUnlit = gltfMaterial.extensions?.KHR_materials_unlit != null;

            if (gltfMaterial.extensions?.KHR_materials_pbrSpecularGlossiness != null)
            {
                material = GetPbrSpecularGlossinessMaterial(gltfMaterial.doubleSided);
            }
            else
            if (isUnlit)
            {
                material = GetUnlitMaterial(gltfMaterial.doubleSided);
            }
            else
            {
                material = GetPbrMetallicRoughnessMaterial(gltfMaterial.doubleSided);
            }

            if (material == null) return null;

            if (!isUnlit && pointsSupport)
            {
                Logger?.Warning(LogCode.TopologyPointsMaterialUnsupported);
            }

            material.name = gltfMaterial.name;

            StandardShaderMode shaderMode = StandardShaderMode.Opaque;
            Color baseColorLinear = Color.white;

            if (gltfMaterial.GetAlphaMode() == AlphaMode.Mask)
            {
                material.SetFloat(AlphaCutoffProperty, gltfMaterial.alphaCutoff);
                shaderMode = StandardShaderMode.Cutout;
            }
            else if (gltfMaterial.GetAlphaMode() == AlphaMode.Blend)
            {
                SetAlphaModeBlend(material);
                shaderMode = StandardShaderMode.Fade;
            }

            if (gltfMaterial.extensions != null)
            {
                // Specular glossiness
                Schema.PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null)
                {
                    baseColorLinear = specGloss.DiffuseColor;
                    material.SetVector(SpecularFactorProperty, specGloss.SpecularColor);
                    material.SetFloat(GlossinessFactorProperty, specGloss.glossinessFactor);

                    TrySetTexture(
                        specGloss.diffuseTexture,
                        material,
                        gltf,
                        BaseColorTextureProperty,
                        BaseColorTextureScaleTransformProperty,
                        BaseColorTextureRotationProperty,
                        BaseColorTextureTexCoordProperty
                        );

                    if (TrySetTexture(
                        specGloss.specularGlossinessTexture,
                        material,
                        gltf,
                        SpecularGlossinessTextureProperty,
                        SpecularGlossinessTextureScaleTransformProperty,
                        SpecularGlossinessTextureRotationProperty,
                        SpecularGlossinessTextureTexCoordProperty
                        ))
                    {
                        material.EnableKeyword(k_SpecGlossMapKeyword);
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness != null
                // If there's a specular-glossiness extension, ignore metallic-roughness
                // (according to extension specification)
                && gltfMaterial.extensions?.KHR_materials_pbrSpecularGlossiness == null)
            {
                baseColorLinear = gltfMaterial.pbrMetallicRoughness.BaseColor;
                material.SetFloat(MetallicProperty, gltfMaterial.pbrMetallicRoughness.metallicFactor);
                material.SetFloat(RoughnessFactorProperty, gltfMaterial.pbrMetallicRoughness.roughnessFactor);

                TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.baseColorTexture,
                    material,
                    gltf,
                    BaseColorTextureProperty,
                    BaseColorTextureScaleTransformProperty,
                    BaseColorTextureRotationProperty,
                    BaseColorTextureTexCoordProperty
                    );

                if (TrySetTexture(
                    gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture,
                    material,
                    gltf,
                    MetallicRoughnessMapProperty,
                    MetallicRoughnessMapScaleTransformProperty,
                    MetallicRoughnessMapRotationProperty,
                    MetallicRoughnessMapUVChannelProperty
                    ))
                {
                    material.EnableKeyword(k_MetallicRoughnessMapKeyword);
                }
            }

            if (TrySetTexture(
                gltfMaterial.normalTexture,
                material,
                gltf,
                NormalTextureProperty,
                NormalTextureScaleTransformProperty,
                NormalTextureRotationProperty,
                NormalTextureTexCoordProperty
            ))
            {
                material.EnableKeyword(Constants.NormalMapKeyword);
                material.SetFloat(NormalTextureScaleProperty, gltfMaterial.normalTexture.scale);
            }

            if (TrySetTexture(
                gltfMaterial.occlusionTexture,
                material,
                gltf,
                OcclusionTextureProperty,
                OcclusionTextureScaleTransformProperty,
                OcclusionTextureRotationProperty,
                OcclusionTextureTexCoordProperty
                ))
            {
                material.EnableKeyword(k_OcclusionKeyword);
                material.SetFloat(OcclusionTextureStrengthProperty, gltfMaterial.occlusionTexture.strength);
            }

            if (TrySetTexture(
                gltfMaterial.emissiveTexture,
                material,
                gltf,
                EmissiveTextureProperty,
                EmissiveTextureScaleTransformProperty,
                EmissiveTextureRotationProperty,
                EmissiveTextureTexCoordProperty
                ))
            {
                material.EnableKeyword(k_EmissionKeyword);
            }

            if (gltfMaterial.extensions != null)
            {

                // Transmission - Approximation
                var transmission = gltfMaterial.extensions.KHR_materials_transmission;
                if (transmission != null)
                {
#if UNITY_EDITOR
                    Logger?.Warning(LogCode.MaterialTransmissionApprox);
#endif
                    // Correct transmission is not supported in Built-In renderer
                    // This is an approximation for some corner cases
                    if (transmission.transmissionFactor > 0f && transmission.transmissionTexture.index < 0)
                    {
                        var premultiply = TransmissionWorkaroundShaderMode(transmission, ref baseColorLinear);
                        shaderMode = premultiply ? StandardShaderMode.Transparent : StandardShaderMode.Fade;
                    }
                }
            }

            switch (shaderMode)
            {
                case StandardShaderMode.Cutout:
                    SetAlphaModeMask(material, gltfMaterial);
                    break;
                case StandardShaderMode.Fade:
                    SetAlphaModeBlend(material);
                    break;
                case StandardShaderMode.Transparent:
                    SetAlphaModeTransparent(material);
                    break;
                default:
                    SetOpaqueMode(material);
                    break;
            }

            material.SetVector(BaseColorProperty, baseColorLinear.gamma);

            if (gltfMaterial.Emissive != Color.black)
            {
                material.SetColor(EmissiveFactorProperty, gltfMaterial.Emissive.gamma);
                material.EnableKeyword(k_EmissionKeyword);
            }

            return material;
        }

        /// <summary>
        /// Configures material for alpha masking.
        /// </summary>
        /// <param name="material">Target material</param>
        /// <param name="alphaCutoff">Threshold value for alpha masking</param>
        public static void SetAlphaModeMask(Material material, float alphaCutoff)
        {
            material.EnableKeyword(AlphaTestOnKeyword);
            material.SetInt(ZWriteProperty, 1);
            material.DisableKeyword(k_AlphaPremultiplyOnKeyword);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;  //2450
            material.SetFloat(AlphaCutoffProperty, alphaCutoff);
            material.SetFloat(k_ModePropId, (int)StandardShaderMode.Cutout);
            material.SetOverrideTag(RenderTypeTag, TransparentCutoutRenderType);
            material.SetInt(SrcBlendProperty, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(DstBlendProperty, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.DisableKeyword(k_AlphaBlendOnKeyword);
        }

        /// <summary>
        /// Configures material for alpha masking.
        /// </summary>
        /// <param name="material">Target material</param>
        /// <param name="gltfMaterial">Source material</param>
        static void SetAlphaModeMask(Material material, Schema.Material gltfMaterial)
        {
            SetAlphaModeMask(material, gltfMaterial.alphaCutoff);
        }

        /// <summary>
        /// Configures material for alpha blending.
        /// </summary>
        /// <param name="material">Target material</param>
        public static void SetAlphaModeBlend(Material material)
        {
            material.SetFloat(k_ModePropId, (int)StandardShaderMode.Fade);
            material.SetOverrideTag(RenderTypeTag, FadeRenderType);
            material.EnableKeyword(k_AlphaBlendOnKeyword);
            material.SetInt(SrcBlendProperty, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);//5
            material.SetInt(DstBlendProperty, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(ZWriteProperty, 0);
            material.DisableKeyword(k_AlphaPremultiplyOnKeyword);
            material.DisableKeyword(AlphaTestOnKeyword);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
        }

        /// <summary>
        /// Configures material for transparency.
        /// </summary>
        /// <param name="material">Target material</param>
        public static void SetAlphaModeTransparent(Material material)
        {
            material.SetFloat(k_ModePropId, (int)StandardShaderMode.Fade);
            material.SetOverrideTag(RenderTypeTag, TransparentRenderType);
            material.EnableKeyword(k_AlphaPremultiplyOnKeyword);
            material.SetInt(SrcBlendProperty, (int)UnityEngine.Rendering.BlendMode.One);//1
            material.SetInt(DstBlendProperty, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(ZWriteProperty, 0);
            material.DisableKeyword(k_AlphaBlendOnKeyword);
            material.DisableKeyword(AlphaTestOnKeyword);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;  //3000
        }

        /// <summary>
        /// Configures material to be opaque.
        /// </summary>
        /// <param name="material">Target material</param>
        public static void SetOpaqueMode(Material material)
        {
            material.SetOverrideTag(RenderTypeTag, OpaqueRenderType);
            material.DisableKeyword(k_AlphaBlendOnKeyword);
            material.renderQueue = -1;
            material.SetInt(SrcBlendProperty, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(DstBlendProperty, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(ZWriteProperty, 1);
            material.DisableKeyword(AlphaTestOnKeyword);
            material.DisableKeyword(k_AlphaPremultiplyOnKeyword);
        }
    }
}
#endif // GLTFAST_BUILTIN_RP || UNITY_EDITOR
