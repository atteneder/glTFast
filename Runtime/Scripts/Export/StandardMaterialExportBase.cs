// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Materials;
using Unity.Cloud.Gltfast.Objects;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Color = UnityEngine.Color;
using Material = Unity.Cloud.Gltfast.Objects.Material;

namespace Unity.Cloud.Gltfast.Export
{
    /// <summary>
    /// Converts URP/HDRP Lit and Built-In Standard shader based materials to glTF materials
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Export", sourceAssembly: "glTFast.Export")]
    public abstract class StandardMaterialExportBase : MaterialExportBase
    {
        const string k_KeywordBumpMap = "_BUMPMAP";
        const string k_KeywordEmission = "_EMISSION";
        const string k_KeywordSmoothnessTextureAlbedoChannelA = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A";

        static readonly int k_EmissionColor = Shader.PropertyToID("_EmissionColor");
        static readonly int k_EmissionMap = Shader.PropertyToID("_EmissionMap");
        static readonly int k_BumpMap = Shader.PropertyToID("_BumpMap");
        static readonly int k_BumpScale = Shader.PropertyToID("_BumpScale");
        static readonly int k_MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");
        static readonly int k_OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        static readonly int k_OcclusionStrength = Shader.PropertyToID("_OcclusionStrength");
        static readonly int k_BaseMap = Shader.PropertyToID("_BaseMap");
        static readonly int k_ColorTexture = Shader.PropertyToID("_ColorTexture");
        static readonly int k_TintColor = Shader.PropertyToID("_TintColor");

        /// <summary>
        /// Converts a Unity material to a glTF material.
        /// </summary>
        /// <param name="uMaterial">Source material</param>
        /// <param name="material">Resulting material</param>
        /// <param name="gltf">Associated IGltfWriter. Is used for adding images and textures.</param>
        /// <param name="logger">Logger used for reporting</param>
        /// <returns>True if no errors occured, false otherwise</returns>
        public override bool ConvertMaterial(
            UnityEngine.Material uMaterial,
            out Material material,
            IGltfWritable gltf,
            ICodeLogger logger
            )
        {
            var success = true;
            material = new Material
            {
                Name = uMaterial.name,
                PbrMetallicRoughness = new PbrMetallicRoughness
                {
                    MetallicFactor = 0,
                    RoughnessFactor = 1.0f
                }
            };

            var mainTexProperty = MainTexProperty;
            if (uMaterial.HasProperty(k_BaseMap))
            {
                mainTexProperty = k_BaseMap;
            }
            else if (uMaterial.HasProperty(k_ColorTexture))
            {
                mainTexProperty = k_ColorTexture;
            }

            SetAlphaModeAndCutoff(uMaterial, material);
            material.DoubleSided = IsDoubleSided(uMaterial, MaterialProperty.Cull);

            if (uMaterial.IsKeywordEnabled(k_KeywordEmission))
            {
                if (uMaterial.HasProperty(k_EmissionColor))
                {
                    var emissionColor = uMaterial.GetColor(k_EmissionColor);

                    // Clamp emissionColor to 0..1
                    var maxFactor = math.max(emissionColor.r, math.max(emissionColor.g, emissionColor.b));
                    if (maxFactor > 1f)
                    {
                        emissionColor.r /= maxFactor;
                        emissionColor.g /= maxFactor;
                        emissionColor.b /= maxFactor;
                        // TODO: use maxFactor as emissiveStrength (KHR_materials_emissive_strength)
                    }

                    material.EmissiveFactor = emissionColor;
                }

                if (uMaterial.HasProperty(k_EmissionMap))
                {
                    var emissionTex = uMaterial.GetTexture(k_EmissionMap);

                    if (emissionTex != null)
                    {
                        if (emissionTex is Texture2D)
                        {
                            material.EmissiveTexture = ExportTextureInfo(emissionTex, gltf);
                            if (material.EmissiveTexture != null)
                            {
                                ExportTextureTransform(material.EmissiveTexture, uMaterial, mainTexProperty, gltf);
                            }
                        }
                        else
                        {
                            logger?.Error(LogCode.TextureInvalidType, "emission", material.Name);
                            success = false;
                        }
                    }
                }
            }
            if (
                uMaterial.HasProperty(k_BumpMap)
                && (uMaterial.IsKeywordEnabled(Materials.Constants.NormalMapKeyword)
                    || uMaterial.IsKeywordEnabled(k_KeywordBumpMap))
            )
            {
                var normalTex = uMaterial.GetTexture(k_BumpMap);

                if (normalTex != null)
                {
                    if (normalTex is Texture2D)
                    {
                        material.NormalTexture = ExportNormalTextureInfo(normalTex, uMaterial, gltf, k_BumpScale);
                        if (material.NormalTexture != null)
                        {
                            ExportTextureTransform(material.NormalTexture, uMaterial, mainTexProperty, gltf);
                        }
                    }
                    else
                    {
                        logger?.Error(LogCode.TextureInvalidType, "normal", uMaterial.name);
                        success = false;
                    }
                }
            }

            var isPbrMetallicRoughness = IsPbrMetallicRoughness(uMaterial);

            Texture2D occlusionTexture = null;
            Texture2D metalGlossTexture = null;
            Texture2D smoothnessTexture = null;
            var smoothnessFactor = 1.0f;

            var needsMetalRoughTexture =
                isPbrMetallicRoughness &&
                (
                    HasMetallicGlossMap(uMaterial)
                    || HasSmoothnessInAlbedoMapAlpha(uMaterial, mainTexProperty)
                );

            if (IsUnlit(uMaterial))
            {
                ExportUnlit(material, uMaterial, mainTexProperty, gltf, logger);
            }
            else if (isPbrMetallicRoughness)
            {
                success &= ExportPbrMetallicRoughness(
                    uMaterial,
                    material,
                    mainTexProperty,
                    gltf,
                    logger,
                    out metalGlossTexture,
                    out smoothnessTexture,
                    out smoothnessFactor
                );
            }
            else if (uMaterial.HasProperty(mainTexProperty))
            {
                var mainTex = uMaterial.GetTexture(mainTexProperty);
                material.PbrMetallicRoughness = new PbrMetallicRoughness
                {
                    MetallicFactor = 0,
                    RoughnessFactor = 1.0f,
                    BaseColorFactor = uMaterial.HasProperty(BaseColorProperty)
                        ? uMaterial.GetColor(BaseColorProperty).linear
                        : Color.white
                };
                if (mainTex != null)
                {
                    material.PbrMetallicRoughness.BaseColorTexture = ExportTextureInfo(mainTex, gltf);
                    if (material.PbrMetallicRoughness.BaseColorTexture != null)
                    {
                        ExportTextureTransform(material.PbrMetallicRoughness.BaseColorTexture, uMaterial, mainTexProperty, gltf);
                    }
                }
                if (uMaterial.HasProperty(k_TintColor))
                {
                    //particles use _TintColor instead of _Color
                    material.PbrMetallicRoughness.BaseColorFactor = uMaterial.GetColor(k_TintColor).linear;
                }
            }

            if (uMaterial.HasProperty(k_OcclusionMap))
            {
                var occTex = uMaterial.GetTexture(k_OcclusionMap);
                if (occTex != null)
                {
                    if (occTex is Texture2D occTex2d)
                    {
                        if (!needsMetalRoughTexture)
                        {
                            material.OcclusionTexture = ExportOcclusionTextureInfo(occTex2d, gltf);
                        }
                        else
                        {
                            material.OcclusionTexture = new OcclusionTextureInfo();
                            occlusionTexture = occTex2d;
                        }
                        if (material.OcclusionTexture != null)
                        {
                            ExportTextureTransform(
                                material.OcclusionTexture,
                                uMaterial,
                                mainTexProperty, // Standard and Lit re-use main texture transform
                                gltf
                            );
                        }
                    }
                    else
                    {
                        logger?.Error(LogCode.TextureInvalidType, "occlusion", material.Name);
                        success = false;
                    }
                }
            }

            if (needsMetalRoughTexture && material.PbrMetallicRoughness != null)
            {
                var ormImageExport = new OrmImageExport(
                    metalGlossTexture, occlusionTexture, smoothnessTexture, smoothnessFactor);
                if (MaterialExport.TryAddImageExport(gltf, ormImageExport, out var ormTextureId))
                {
                    if (material.PbrMetallicRoughness.MetallicRoughnessTexture != null)
                    {
                        material.PbrMetallicRoughness.MetallicRoughnessTexture.Index = ormTextureId;
                        ExportTextureTransform(material.PbrMetallicRoughness.MetallicRoughnessTexture, uMaterial, mainTexProperty, gltf);
                    }

                    if (ormImageExport.HasOcclusion)
                    {
                        material.OcclusionTexture.Index = ormTextureId;
                    }
                }
#if UNITY_IMAGECONVERSION
                else
                {
                    logger?.Error(LogCode.ExportImageFailed);
                }
#endif
            }

            if (material.OcclusionTexture != null)
            {
                if (uMaterial.HasProperty(k_OcclusionStrength))
                {
                    material.OcclusionTexture.Strength = uMaterial.GetFloat(k_OcclusionStrength);
                }
            }

            return success;
        }

        static bool HasSmoothnessInAlbedoMapAlpha(UnityEngine.Material uMaterial, int mainTexProperty)
        {
            return uMaterial.IsKeywordEnabled(k_KeywordSmoothnessTextureAlbedoChannelA)
                && uMaterial.HasProperty(mainTexProperty)
                && uMaterial.GetTexture(mainTexProperty) is not null;
        }

        /// <summary>
        /// Detects whether the source material should be exported as a PBR Metallic-Roughness material.
        /// </summary>
        /// <param name="material">Unity source material</param>
        /// <returns>True when the exported glTF material should be PBR Metallic-Rougness based,
        /// false otherwise.</returns>
        protected abstract bool IsPbrMetallicRoughness(UnityEngine.Material material);

        bool ExportPbrMetallicRoughness(
            UnityEngine.Material uMaterial,
            Material material,
            int mainTexProperty,
            IGltfWritable gltf,
            ICodeLogger logger,
            out Texture2D metalGlossTexture,
            out Texture2D smoothnessTexture,
            out float smoothnessFactor
        )
        {
            metalGlossTexture = null;
            smoothnessTexture = null;
            var success = true;
            var pbr = new PbrMetallicRoughness { MetallicFactor = 0, RoughnessFactor = 1.0f };

            var hasAlphaSmoothness = uMaterial.IsKeywordEnabled(k_KeywordSmoothnessTextureAlbedoChannelA);

            if (uMaterial.HasProperty(BaseColorProperty))
            {
                pbr.BaseColorFactor = uMaterial.GetColor(BaseColorProperty).linear;
            }
            else if (uMaterial.HasProperty(ColorProperty))
            {
                pbr.BaseColorFactor = uMaterial.GetColor(ColorProperty).linear;
            }

            if (uMaterial.HasProperty(k_TintColor))
            {
                //particles use _TintColor instead of _Color
                float white = 1;
                if (uMaterial.HasProperty(ColorProperty))
                {
                    var c = uMaterial.GetColor(ColorProperty);
                    white = (c.r + c.g + c.b) / 3.0f; //multiply alpha by overall whiteness of TintColor
                }

                pbr.BaseColorFactor = (uMaterial.GetColor(k_TintColor) * white).linear;
            }

            if (uMaterial.HasProperty(mainTexProperty))
            {
                // TODO if additive particle, render black into alpha
                // TODO use private Material.GetFirstPropertyNameIdByAttribute here, supported from 2020.1+
                var mainTex = uMaterial.GetTexture(mainTexProperty);

                if (mainTex)
                {
                    if (mainTex is Texture2D)
                    {
                        pbr.BaseColorTexture = ExportTextureInfo(
                            mainTex,
                            gltf,
                            // Force RGB for the baseColor, so that the alpha (which is smoothness)
                            // is not used for alpha-opacity
                            hasAlphaSmoothness
                                ? ImageFormat.Jpeg
                                : ImageFormat.Unknown
                        );
                        if (pbr.BaseColorTexture != null)
                        {
                            ExportTextureTransform(pbr.BaseColorTexture, uMaterial, mainTexProperty, gltf);
                        }
                    }
                    else
                    {
                        logger?.Error(LogCode.TextureInvalidType, "main", uMaterial.name);
                        success = false;
                    }
                }
            }

            if (uMaterial.HasProperty(MetallicProperty) && !HasMetallicGlossMap(uMaterial))
            {
                pbr.MetallicFactor = uMaterial.GetFloat(MetallicProperty);
            }

            metalGlossTexture = GetMetallicGlossMap(uMaterial);

            smoothnessFactor = 0f;
            var smoothnessSourceAlbedoAlpha = uMaterial.IsKeywordEnabled(k_KeywordSmoothnessTextureAlbedoChannelA);
            var hasSmoothnessTexture = false;

            if (metalGlossTexture is not null)
            {
                if (!smoothnessSourceAlbedoAlpha)
                {
                    hasSmoothnessTexture = true;
                    // smoothness channel * smoothnessFactor will be baked into ORM texture.
                    pbr.RoughnessFactor = 1f;
                }
                pbr.MetallicRoughnessTexture ??= new TextureInfo();
                if (HasMetallicGlossMap(uMaterial))
                {
                    pbr.MetallicFactor = 1.0f;
                }
                ExportTextureTransform(pbr.MetallicRoughnessTexture, uMaterial, k_MetallicGlossMap, gltf);
            }

            if (smoothnessSourceAlbedoAlpha && uMaterial.GetTexture(mainTexProperty) is Texture2D smoothnessTex)
            {
                // smoothnessFactor will be baked into ORM texture
                pbr.RoughnessFactor = 1f;
                pbr.MetallicRoughnessTexture ??= new TextureInfo();
                smoothnessTexture = smoothnessTex;
                ExportTextureTransform(pbr.MetallicRoughnessTexture, uMaterial, mainTexProperty, gltf);
                hasSmoothnessTexture = true;
            }

            var smoothnessProperty = GetSmoothnessProperty(
                smoothnessSourceAlbedoAlpha, metalGlossTexture is not null);
            if (uMaterial.HasProperty(smoothnessProperty))
            {
                smoothnessFactor = uMaterial.GetFloat(smoothnessProperty);
                if (!hasSmoothnessTexture)
                {
                    pbr.RoughnessFactor = 1f - smoothnessFactor;
                }
            }

            if (!hasSmoothnessTexture)
            {
                // Set smoothnessFactor to 0
                // to prevent albedo/metallic alpha getting baked into the ORM texture inadvertently.
                smoothnessFactor = 0f;
            }

            material.PbrMetallicRoughness = pbr;
            return success;
        }

        static Texture2D GetMetallicGlossMap(UnityEngine.Material uMaterial)
        {
            if (uMaterial.HasProperty(k_MetallicGlossMap))
            {
                var metallicGlossMap = uMaterial.GetTexture(k_MetallicGlossMap);
                // Use implicit Unity object lifetime check to convert to explicit null value.
                if (metallicGlossMap == null)
                {
                    return null;
                }

                if (metallicGlossMap is Texture2D mgTex2d)
                {
                    return mgTex2d;
                }
            }

            return null;
        }

        /// <summary>
        /// Detects whether a metallic-glossiness map is assigned to the source material.
        /// </summary>
        /// <param name="uMaterial">Unity source material.</param>
        /// <returns>True if the material has a metallic-glossiness map assigned, false otherwise.</returns>
        protected abstract bool HasMetallicGlossMap(UnityEngine.Material uMaterial);

        /// <summary>
        /// Retrieves the smoothness property ID for the source material's smoothness value.
        /// </summary>
        /// <param name="sourceAlbedoAlpha">True when the smoothness source is the Albedo map's alpha channel.</param>
        /// <param name="hasMetallicGlossinessMap">True when a metallic-glossiness map is assigned
        /// to the source material.</param>
        /// <returns>The property ID for the smoothness value property in use.</returns>
        protected abstract int GetSmoothnessProperty(bool sourceAlbedoAlpha, bool hasMetallicGlossinessMap);

        static OcclusionTextureInfo ExportOcclusionTextureInfo(
            UnityEngine.Texture texture,
            IGltfWritable gltf
        )
        {
            var texture2d = texture as Texture2D;
            if (texture2d == null)
            {
                return null;
            }
            var imageExport = new ImageExport(texture2d);
            if (MaterialExport.TryAddImageExport(gltf, imageExport, out var textureId))
            {
                return new OcclusionTextureInfo
                {
                    Index = textureId
                };
            }
            return null;
        }
    }
}
