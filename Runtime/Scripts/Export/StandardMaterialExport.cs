// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

using Unity.Mathematics;
using UnityEngine;

namespace GLTFast.Export
{

    using Logging;
    using Schema;

    /// <summary>
    /// Converts URP/HDRP Lit and Built-In Standard shader based materials to glTF materials
    /// </summary>
    public class StandardMaterialExport : MaterialExportBase
    {

        const string k_KeywordBumpMap = "_BUMPMAP";
        const string k_KeywordEmission = "_EMISSION";
        const string k_KeywordMetallicGlossMap = "_METALLICGLOSSMAP"; // Built-In Standard
#if USING_URP || USING_HDRP
        const string k_KeywordMetallicSpecGlossMap = "_METALLICSPECGLOSSMAP"; // URP Lit
#endif
        const string k_KeywordSmoothnessTextureAlbedoChannelA = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A";

        static readonly int k_Cull = Shader.PropertyToID("_Cull");
        static readonly int k_EmissionColor = Shader.PropertyToID("_EmissionColor");
        static readonly int k_EmissionMap = Shader.PropertyToID("_EmissionMap");
        static readonly int k_BumpMap = Shader.PropertyToID("_BumpMap");
        static readonly int k_BumpScale = Shader.PropertyToID("_BumpScale");
        static readonly int k_OcclusionMap = Shader.PropertyToID("_OcclusionMap");
        static readonly int k_OcclusionStrength = Shader.PropertyToID("_OcclusionStrength");
        static readonly int k_BaseMap = Shader.PropertyToID("_BaseMap");
        static readonly int k_ColorTexture = Shader.PropertyToID("_ColorTexture");
        static readonly int k_TintColor = Shader.PropertyToID("_TintColor");
        static readonly int k_MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");
        static readonly int k_Glossiness = Shader.PropertyToID("_Glossiness");
        static readonly int k_GlossMapScale = Shader.PropertyToID("_GlossMapScale");

        /// <summary>
        /// Converts a Unity material to a glTF material.
        /// </summary>
        /// <param name="uMaterial">Source material</param>
        /// <param name="material">Resulting material</param>
        /// <param name="gltf">Associated IGltfWriter. Is used for adding images and textures.</param>
        /// <param name="logger">Logger used for reporting</param>
        /// <returns>True if no errors occured, false otherwise</returns>
        public override bool ConvertMaterial(UnityEngine.Material uMaterial, out Material material, IGltfWritable gltf, ICodeLogger logger)
        {
            var success = true;
            material = new Material
            {
                name = uMaterial.name,
                pbrMetallicRoughness = new PbrMetallicRoughness
                {
                    metallicFactor = 0,
                    roughnessFactor = 1.0f
                }
            };

            SetAlphaModeAndCutoff(uMaterial, material);
            material.doubleSided = IsDoubleSided(uMaterial, k_Cull);

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

                    material.Emissive = emissionColor;
                }

                if (uMaterial.HasProperty(k_EmissionMap))
                {
                    var emissionTex = uMaterial.GetTexture(k_EmissionMap);

                    if (emissionTex != null)
                    {
                        if (emissionTex is Texture2D)
                        {
                            material.emissiveTexture = ExportTextureInfo(emissionTex, gltf);
                            if (material.emissiveTexture != null)
                            {
                                ExportTextureTransform(material.emissiveTexture, uMaterial, k_EmissionMap, gltf);
                            }
                        }
                        else
                        {
                            logger?.Error(LogCode.TextureInvalidType, "emission", material.name);
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
                        material.normalTexture = ExportNormalTextureInfo(normalTex, uMaterial, gltf, k_BumpScale);
                        if (material.normalTexture != null)
                        {
                            ExportTextureTransform(material.normalTexture, uMaterial, k_BumpMap, gltf);
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
            var needsMetalRoughTexture =
                isPbrMetallicRoughness &&
                (
                    HasMetallicGlossMap(uMaterial)
                    || uMaterial.IsKeywordEnabled(k_KeywordSmoothnessTextureAlbedoChannelA)
                );

            OrmImageExport ormImageExport = null;
            var mainTexProperty = MainTexProperty;
            if (uMaterial.HasProperty(k_BaseMap))
            {
                mainTexProperty = k_BaseMap;
            }
            else if (uMaterial.HasProperty(k_ColorTexture))
            {
                mainTexProperty = k_ColorTexture;
            }

            if (needsMetalRoughTexture)
            {
                ormImageExport = new OrmImageExport();
            }

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
                    ormImageExport,
                    gltf,
                    logger
                );
            }
            else if (uMaterial.HasProperty(mainTexProperty))
            {
                var mainTex = uMaterial.GetTexture(mainTexProperty);
                material.pbrMetallicRoughness = new PbrMetallicRoughness
                {
                    metallicFactor = 0,
                    roughnessFactor = 1.0f,
                    BaseColor = uMaterial.HasProperty(BaseColorProperty)
                        ? uMaterial.GetColor(BaseColorProperty)
                        : Color.white
                };
                if (mainTex != null)
                {
                    material.pbrMetallicRoughness.baseColorTexture = ExportTextureInfo(mainTex, gltf);
                    if (material.pbrMetallicRoughness.baseColorTexture != null)
                    {
                        ExportTextureTransform(material.pbrMetallicRoughness.baseColorTexture, uMaterial, mainTexProperty, gltf);
                    }
                }
                if (uMaterial.HasProperty(k_TintColor))
                {
                    //particles use _TintColor instead of _Color
                    material.pbrMetallicRoughness.BaseColor = uMaterial.GetColor(k_TintColor);
                }
            }

            if (uMaterial.HasProperty(k_OcclusionMap))
            {
                var occTex = uMaterial.GetTexture(k_OcclusionMap);
                if (occTex != null)
                {
                    if (occTex is Texture2D occTex2d)
                    {
                        if (ormImageExport == null)
                        {
                            material.occlusionTexture = ExportOcclusionTextureInfo(occTex2d, gltf);
                        }
                        else
                        {
                            material.occlusionTexture = new OcclusionTextureInfo();
                            ormImageExport.SetOcclusionTexture(occTex2d);
                        }
                        if (material.occlusionTexture != null)
                        {
                            ExportTextureTransform(
                                material.occlusionTexture,
                                uMaterial,
                                mainTexProperty, // Standard and Lit re-use main texture transform
                                gltf
                            );
                        }
                    }
                    else
                    {
                        logger?.Error(LogCode.TextureInvalidType, "occlusion", material.name);
                        success = false;
                    }
                }
            }

            if (ormImageExport != null && material.pbrMetallicRoughness != null)
            {
                if (AddImageExport(gltf, ormImageExport, out var ormTextureId))
                {
                    if (material.pbrMetallicRoughness.MetallicRoughnessTexture != null)
                    {
                        material.PbrMetallicRoughness.MetallicRoughnessTexture.index = ormTextureId;
                        ExportTextureTransform(material.PbrMetallicRoughness.MetallicRoughnessTexture, uMaterial, k_MetallicGlossMap, gltf);
                    }

                    if (ormImageExport.HasOcclusion)
                    {
                        material.occlusionTexture.index = ormTextureId;
                    }
                }
#if UNITY_IMAGECONVERSION
                else {
                    logger?.Error(LogCode.ExportImageFailed);
                }
#endif
            }

            if (material.occlusionTexture != null)
            {
                if (uMaterial.HasProperty(k_OcclusionStrength))
                {
                    material.occlusionTexture.strength = uMaterial.GetFloat(k_OcclusionStrength);
                }
            }

            return success;
        }

        static bool IsPbrMetallicRoughness(UnityEngine.Material material)
        {
            return material.HasProperty(MetallicProperty)
                && (
                    HasMetallicGlossMap(material)
                    || material.HasProperty(k_Glossiness)
                    || material.HasProperty(SmoothnessProperty)
                );
        }

        static bool ExportPbrMetallicRoughness(
            UnityEngine.Material uMaterial,
            Material material,
            int mainTexProperty,
            OrmImageExport ormImageExport,
            IGltfWritable gltf,
            ICodeLogger logger
        )
        {
            var success = true;
            var pbr = new PbrMetallicRoughness { metallicFactor = 0, roughnessFactor = 1.0f };

            var hasAlphaSmoothness = uMaterial.IsKeywordEnabled(k_KeywordSmoothnessTextureAlbedoChannelA);

            if (uMaterial.HasProperty(BaseColorProperty))
            {
                pbr.BaseColor = uMaterial.GetColor(BaseColorProperty);
            }
            else
            if (uMaterial.HasProperty(ColorProperty))
            {
                pbr.BaseColor = uMaterial.GetColor(ColorProperty);
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

                pbr.BaseColor = uMaterial.GetColor(k_TintColor) * white;
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
                        pbr.baseColorTexture = ExportTextureInfo(
                            mainTex,
                            gltf,
                            // Force RGB for the baseColor, so that the alpha (which is smoothness)
                            // is not used for alpha-opacity
                            hasAlphaSmoothness
                                ? ImageFormat.Jpg
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
                pbr.metallicFactor = uMaterial.GetFloat(MetallicProperty);
            }

            if (uMaterial.HasProperty(k_Glossiness) || uMaterial.HasProperty(SmoothnessProperty))
            {
                var smoothnessPropId = uMaterial.HasProperty(SmoothnessProperty) ? SmoothnessProperty : k_Glossiness;
                var metallicGlossMap = uMaterial.HasProperty(k_MetallicGlossMap) ? uMaterial.GetTexture(k_MetallicGlossMap) : null;
                var smoothness = uMaterial.GetFloat(smoothnessPropId);
                pbr.roughnessFactor = (metallicGlossMap != null || hasAlphaSmoothness) && uMaterial.HasProperty(k_GlossMapScale)
                    ? uMaterial.GetFloat(k_GlossMapScale)
                    : 1f - smoothness;
            }

            if (uMaterial.HasProperty(k_MetallicGlossMap))
            {
                var mrTex = uMaterial.GetTexture(k_MetallicGlossMap);
                if (mrTex != null)
                {
                    if (mrTex is Texture2D mrTex2d)
                    {
                        pbr.metallicRoughnessTexture = pbr.metallicRoughnessTexture ?? new TextureInfo();
                        ormImageExport.SetMetalGlossTexture(mrTex2d);
                        if (HasMetallicGlossMap(uMaterial))
                            pbr.metallicFactor = 1.0f;
                        ExportTextureTransform(pbr.metallicRoughnessTexture, uMaterial, k_MetallicGlossMap, gltf);
                    }
                    else
                    {
                        logger?.Error(LogCode.TextureInvalidType, "metallic/gloss", uMaterial.name);
                        success = false;
                    }
                }
            }

            if (uMaterial.IsKeywordEnabled(k_KeywordSmoothnessTextureAlbedoChannelA))
            {
                var smoothnessTex = uMaterial.GetTexture(mainTexProperty) as Texture2D;
                if (smoothnessTex != null)
                {
                    pbr.metallicRoughnessTexture = pbr.metallicRoughnessTexture ?? new TextureInfo();
                    ormImageExport.SetSmoothnessTexture(smoothnessTex);
                    ExportTextureTransform(pbr.metallicRoughnessTexture, uMaterial, mainTexProperty, gltf);
                }
            }

            material.pbrMetallicRoughness = pbr;
            return success;
        }

        static bool HasMetallicGlossMap(UnityEngine.Material uMaterial)
        {
            return uMaterial.IsKeywordEnabled(k_KeywordMetallicGlossMap) // Built-In Standard
#if USING_URP || USING_HDRP
                || uMaterial.IsKeywordEnabled(k_KeywordMetallicSpecGlossMap) // URP Lit
#endif
                ;
        }

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
            if (AddImageExport(gltf, imageExport, out var textureId))
            {
                return new OcclusionTextureInfo
                {
                    index = textureId
                };
            }
            return null;
        }
    }
}
