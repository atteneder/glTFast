// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if ! ( USING_URP || USING_HDRP || GLTFAST_BUILTIN_SHADER_GRAPH )
#define GLTFAST_BUILTIN_RP
#endif

#if GLTFAST_BUILTIN_RP || UNITY_EDITOR

using Unity.Cloud.Gltfast.Objects;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Material = UnityEngine.Material;

namespace Unity.Cloud.Gltfast.Materials
{

    using Logging;
    using AlphaMode = AlphaMode;

    /// <summary>
    /// Converts glTF materials to Unity materials for the Built-in Render Pipeline
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Materials", sourceAssembly: "glTFast")]
    public class BuiltInMaterialGenerator : MaterialGenerator
    {

        // Built-in Render Pipeline
        const string k_AlphaBlendOnKeyword = "_ALPHABLEND_ON";
        const string k_AlphaPremultiplyOnKeyword = "_ALPHAPREMULTIPLY_ON";
        const string k_EmissionKeyword = "_EMISSION";
        const string k_MetallicRoughnessMapKeyword = "_METALLICGLOSSMAP";
        const string k_OcclusionKeyword = "_OCCLUSION";
        const string k_SpecGlossMapKeyword = "_SPECGLOSSMAP";

#if UNITY_EDITOR
        const string k_ShaderPathPrefix = "Packages/" + GltfGlobals.GltfPackageName + "/Runtime/Shader/Built-In/";
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
                if (s_DefaultMaterial != null)
                {
                    s_DefaultMaterial.name = DefaultMaterialName;
                }
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
        // Needs to be non-static outside of the Editor.
        // ReSharper disable once MemberCanBeMadeStatic.Local
        protected virtual Shader FindShaderMetallicRoughness()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{k_ShaderPathPrefix}{k_PbrMetallicRoughnessShaderPath}");
#else
            return FindShader(k_PbrMetallicRoughnessShaderName);
#endif
        }

        /// <summary>
        /// Finds the shader required for specular/glossiness based materials.
        /// </summary>
        /// <returns>Specular/Glossiness shader</returns>
        // Needs to be non-static outside of the Editor.
        // ReSharper disable once MemberCanBeMadeStatic.Local
        protected virtual Shader FindShaderSpecularGlossiness()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{k_ShaderPathPrefix}{k_PbrSpecularGlossinessShaderPath}");
#else
            return FindShader(k_PbrSpecularGlossinessShaderName);
#endif
        }

        /// <summary>
        /// Finds the shader required for unlit materials.
        /// </summary>
        /// <returns>Unlit shader</returns>
        // Needs to be non-static outside of the Editor.
        // ReSharper disable once MemberCanBeMadeStatic.Local
        protected virtual Shader FindShaderUnlit()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Shader>($"{k_ShaderPathPrefix}{k_UnlitShaderPath}");
#else
            return FindShader(k_UnlitShaderName);
#endif
        }

        Material GetPbrMetallicRoughnessMaterial(bool doubleSided = false)
        {
            if (m_PbrMetallicRoughnessShader == null)
            {
                m_PbrMetallicRoughnessShader = FindShaderMetallicRoughness();
            }
            if (m_PbrMetallicRoughnessShader == null)
            {
                return null;
            }
            var mat = new Material(m_PbrMetallicRoughnessShader);
            if (doubleSided)
            {
                // Turn off back-face culling
                mat.SetFloat(MaterialProperty.CullMode, 0);
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
                m_PbrSpecularGlossinessShader = FindShaderSpecularGlossiness();
            }
            if (m_PbrSpecularGlossinessShader == null)
            {
                return null;
            }
            var mat = new Material(m_PbrSpecularGlossinessShader);
            if (doubleSided)
            {
                // Turn off back-face culling
                mat.SetFloat(MaterialProperty.CullMode, 0);
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
                m_UnlitShader = FindShaderUnlit();
            }
            if (m_UnlitShader == null)
            {
                return null;
            }
            var mat = new Material(m_UnlitShader);
            if (doubleSided)
            {
                // Turn off back-face culling
                mat.SetFloat(MaterialProperty.CullMode, 0);
#if UNITY_EDITOR
                mat.doubleSidedGI = true;
#endif
            }
            return mat;
        }

        /// <inheritdoc />
        public override Material GenerateMaterial(
            Objects.Material gltfMaterial,
            IGltfReadable gltf,
            bool pointsSupport = false
        )
        {
            Material material;

            var isUnlit = gltfMaterial.Extensions?.Unlit != null;

            if (gltfMaterial.Extensions?.PbrSpecularGlossiness != null)
            {
                material = GetPbrSpecularGlossinessMaterial(gltfMaterial.DoubleSided);
            }
            else if (isUnlit)
            {
                material = GetUnlitMaterial(gltfMaterial.DoubleSided);
            }
            else
            {
                material = GetPbrMetallicRoughnessMaterial(gltfMaterial.DoubleSided);
            }

            if (material == null) return null;

            if (!isUnlit && pointsSupport)
            {
                Logger?.Warning(LogCode.TopologyPointsMaterialUnsupported);
            }

            material.name = gltfMaterial.Name;

            StandardShaderMode shaderMode = StandardShaderMode.Opaque;
            var baseColorLinear = UnityEngine.Color.white;

            if (gltfMaterial.AlphaMode == AlphaMode.Mask)
            {
                material.SetFloat(MaterialProperty.AlphaCutoff, gltfMaterial.AlphaCutoff);
                shaderMode = StandardShaderMode.Cutout;
            }
            else if (gltfMaterial.AlphaMode == AlphaMode.Blend)
            {
                SetAlphaModeBlend(material);
                shaderMode = StandardShaderMode.Fade;
            }

            if (gltfMaterial.Extensions != null)
            {
                // Specular glossiness
                Objects.PbrSpecularGlossiness specGloss = gltfMaterial.Extensions.PbrSpecularGlossiness;
                if (specGloss != null)
                {
                    baseColorLinear = specGloss.DiffuseFactor;
                    material.SetVector(MaterialProperty.SpecularFactor, (UnityEngine.Color)specGloss.SpecularFactor);
                    material.SetFloat(MaterialProperty.GlossinessFactor, specGloss.GlossinessFactor);

                    TrySetTexture(
                        specGloss.DiffuseTexture,
                        material,
                        gltf,
                        MaterialProperty.BaseColorTexture,
                        MaterialProperty.BaseColorTextureScaleTransform,
                        MaterialProperty.BaseColorTextureRotation,
                        MaterialProperty.BaseColorTextureTexCoord
                        );

                    if (TrySetTexture(
                        specGloss.SpecularGlossinessTexture,
                        material,
                        gltf,
                        MaterialProperty.SpecularGlossinessTexture,
                        MaterialProperty.SpecularGlossinessTextureScaleTransform,
                        MaterialProperty.SpecularGlossinessTextureRotation,
                        MaterialProperty.SpecularGlossinessTextureTexCoord
                        ))
                    {
                        material.EnableKeyword(k_SpecGlossMapKeyword);
                    }
                }
            }

            if (gltfMaterial.PbrMetallicRoughness != null
                // If there's a specular-glossiness extension, ignore metallic-roughness
                // (according to extension specification)
                && gltfMaterial.Extensions?.PbrSpecularGlossiness == null)
            {
                baseColorLinear = gltfMaterial.PbrMetallicRoughness.BaseColorFactor;
                material.SetFloat(MaterialProperty.Metallic, gltfMaterial.PbrMetallicRoughness.MetallicFactor);
                material.SetFloat(MaterialProperty.RoughnessFactor, gltfMaterial.PbrMetallicRoughness.RoughnessFactor);

                TrySetTexture(
                    gltfMaterial.PbrMetallicRoughness.BaseColorTexture,
                    material,
                    gltf,
                    MaterialProperty.BaseColorTexture,
                    MaterialProperty.BaseColorTextureScaleTransform,
                    MaterialProperty.BaseColorTextureRotation,
                    MaterialProperty.BaseColorTextureTexCoord
                    );

                if (TrySetTexture(
                    gltfMaterial.PbrMetallicRoughness.MetallicRoughnessTexture,
                    material,
                    gltf,
                    MaterialProperty.MetallicRoughnessMap,
                    MaterialProperty.MetallicRoughnessMapScaleTransform,
                    MaterialProperty.MetallicRoughnessMapRotation,
                    MaterialProperty.MetallicRoughnessMapTexCoord
                    ))
                {
                    material.EnableKeyword(k_MetallicRoughnessMapKeyword);
                }
            }

            if (TrySetTexture(
                gltfMaterial.NormalTexture,
                material,
                gltf,
                MaterialProperty.NormalTexture,
                MaterialProperty.NormalTextureScaleTransform,
                MaterialProperty.NormalTextureRotation,
                MaterialProperty.NormalTextureTexCoord
            ))
            {
                material.EnableKeyword(Constants.NormalMapKeyword);
                material.SetFloat(MaterialProperty.NormalTextureScale, gltfMaterial.NormalTexture.Scale);
            }

            if (TrySetTexture(
                gltfMaterial.OcclusionTexture,
                material,
                gltf,
                MaterialProperty.OcclusionTexture,
                MaterialProperty.OcclusionTextureScaleTransform,
                MaterialProperty.OcclusionTextureRotation,
                MaterialProperty.OcclusionTextureTexCoord
                ))
            {
                material.EnableKeyword(k_OcclusionKeyword);
                material.SetFloat(MaterialProperty.OcclusionTextureStrength, gltfMaterial.OcclusionTexture.Strength);
            }

            if (TrySetTexture(
                gltfMaterial.EmissiveTexture,
                material,
                gltf,
                MaterialProperty.EmissiveTexture,
                MaterialProperty.EmissiveTextureScaleTransform,
                MaterialProperty.EmissiveTextureRotation,
                MaterialProperty.EmissiveTextureTexCoord
                ))
            {
                material.EnableKeyword(k_EmissionKeyword);
            }

            if (gltfMaterial.Extensions != null)
            {

                // Transmission - Approximation
                var transmission = gltfMaterial.Extensions.Transmission;
                if (transmission != null)
                {
#if UNITY_EDITOR
                    Logger?.Warning(LogCode.MaterialTransmissionApprox);
#endif
                    // Correct transmission is not supported in Built-In renderer
                    // This is an approximation for some corner cases
                    if (transmission.TransmissionFactor > 0f
                        && (transmission.TransmissionTexture == null
                           || transmission.TransmissionTexture.Index < 0)
                        )
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

            material.SetVector(MaterialProperty.BaseColor, baseColorLinear.gamma);

            if (gltfMaterial.EmissiveFactor != Objects.Color.Black)
            {
                material.SetColor(MaterialProperty.EmissiveFactor, gltfMaterial.EmissiveFactor);
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
            material.SetInt(MaterialProperty.ZWrite, 1);
            material.DisableKeyword(k_AlphaPremultiplyOnKeyword);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;  //2450
            material.SetFloat(MaterialProperty.AlphaCutoff, alphaCutoff);
            material.SetFloat(MaterialProperty.Mode, (int)StandardShaderMode.Cutout);
            material.SetOverrideTag(RenderTypeTag, TransparentCutoutRenderType);
            material.SetInt(MaterialProperty.SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(MaterialProperty.DstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.DisableKeyword(k_AlphaBlendOnKeyword);
        }

        /// <summary>
        /// Configures material for alpha masking.
        /// </summary>
        /// <param name="material">Target material</param>
        /// <param name="gltfMaterial">Source material</param>
        static void SetAlphaModeMask(Material material, Objects.Material gltfMaterial)
        {
            SetAlphaModeMask(material, gltfMaterial.AlphaCutoff);
        }

        /// <summary>
        /// Configures material for alpha blending.
        /// </summary>
        /// <param name="material">Target material</param>
        public static void SetAlphaModeBlend(Material material)
        {
            material.SetFloat(MaterialProperty.Mode, (int)StandardShaderMode.Fade);
            material.SetOverrideTag(RenderTypeTag, FadeRenderType);
            material.EnableKeyword(k_AlphaBlendOnKeyword);
            material.SetInt(MaterialProperty.SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);//5
            material.SetInt(MaterialProperty.DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(MaterialProperty.ZWrite, 0);
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
            material.SetFloat(MaterialProperty.Mode, (int)StandardShaderMode.Fade);
            material.SetOverrideTag(RenderTypeTag, TransparentRenderType);
            material.EnableKeyword(k_AlphaPremultiplyOnKeyword);
            material.SetInt(MaterialProperty.SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);//1
            material.SetInt(MaterialProperty.DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);//10
            material.SetInt(MaterialProperty.ZWrite, 0);
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
            material.SetInt(MaterialProperty.SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt(MaterialProperty.DstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt(MaterialProperty.ZWrite, 1);
            material.DisableKeyword(AlphaTestOnKeyword);
            material.DisableKeyword(k_AlphaPremultiplyOnKeyword);
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticsOnLoad()
        {
            // Reset static state
            s_DefaultMaterialGenerated = false;
            s_DefaultMaterial = null;
        }
#endif
    }
}
#endif // GLTFAST_BUILTIN_RP || UNITY_EDITOR
