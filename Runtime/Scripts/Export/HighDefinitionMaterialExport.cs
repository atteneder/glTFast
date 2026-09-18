// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if USING_HDRP

using System;
using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Materials;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Export
{

    using Objects;

    [MovedFrom(true, sourceNamespace: "GLTFast.Export", sourceAssembly: "glTFast.Export")]
    public class HighDefinitionMaterialExport : MaterialExportBase
    {

        const string k_KeywordNormalMapTangentSpace = "_NORMALMAP_TANGENT_SPACE";
        const string k_KeywordMaskMap = "_MASKMAP"; // HDRP Lit

        static readonly int k_AORemapMax = Shader.PropertyToID("_AORemapMax");
        static readonly int k_AORemapMin = Shader.PropertyToID("_AORemapMin");
        static readonly int k_EmissiveColor = Shader.PropertyToID("_EmissiveColor");
        static readonly int k_EmissionColorMap = Shader.PropertyToID("_EmissiveColorMap");
        static readonly int k_NormalMap = Shader.PropertyToID("_NormalMap");
        static readonly int k_NormalScale = Shader.PropertyToID("_NormalScale");
        static readonly int k_BaseColorMap = Shader.PropertyToID("_BaseColorMap");
        static readonly int k_MaskMap = Shader.PropertyToID("_MaskMap");
        static readonly int k_SmoothnessRemapMax = Shader.PropertyToID("_SmoothnessRemapMax");
        static readonly int k_SmoothnessRemapMin = Shader.PropertyToID("_SmoothnessRemapMin");
        static readonly int k_UnlitColor = Shader.PropertyToID("_UnlitColor");
        static readonly int k_CoatMask = Shader.PropertyToID("_CoatMask");
        static readonly int k_CoatMaskMap = Shader.PropertyToID("_CoatMaskMap");

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
                Name = uMaterial.name,
                PbrMetallicRoughness = new PbrMetallicRoughness
                {
                    MetallicFactor = 0,
                    RoughnessFactor = 1.0f
                }
            };

            var mainTexProperty = uMaterial.HasProperty(k_BaseColorMap) ? k_BaseColorMap : MainTexProperty;

            SetAlphaModeAndCutoff(uMaterial, material);
            material.DoubleSided = IsDoubleSided(uMaterial, MaterialProperty.CullMode);

            //
            // Emission
            //
            if (uMaterial.HasProperty(k_EmissiveColor))
            {
                var emissionColor = uMaterial.GetColor(k_EmissiveColor);

                // Clamp emissiveColor to 0..1
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

            if (uMaterial.HasProperty(k_EmissionColorMap))
            {
                var emissionTex = uMaterial.GetTexture(k_EmissionColorMap);

                if (emissionTex != null)
                {
                    if (emissionTex is Texture2D)
                    {
                        material.EmissiveTexture = ExportTextureInfo(emissionTex, gltf);
                        ExportTextureTransform(material.EmissiveTexture, uMaterial, mainTexProperty, gltf);
                    }
                    else
                    {
                        logger?.Error(LogCode.TextureInvalidType, "emission", material.Name);
                        success = false;
                    }
                }
            }

            //
            // Normal Map
            //
            if (
                uMaterial.HasProperty(k_NormalMap)
                && uMaterial.IsKeywordEnabled(k_KeywordNormalMapTangentSpace)
            )
            {
                var normalTex = uMaterial.GetTexture(k_NormalMap);

                if (normalTex != null)
                {
                    if (normalTex is Texture2D)
                    {
                        material.NormalTexture = ExportNormalTextureInfo(normalTex, uMaterial, gltf, k_NormalScale);
                        ExportTextureTransform(material.NormalTexture, uMaterial, mainTexProperty, gltf);
                    }
                    else
                    {
                        logger?.Error(LogCode.TextureInvalidType, "normal", uMaterial.name);
                        success = false;
                    }
                }
            }

            //
            // Clearcoat
            //
            if (uMaterial.HasProperty(k_CoatMask) && uMaterial.GetFloat(k_CoatMask) > 0)
            {
                gltf.RegisterExtensionUsage(Extension.MaterialsClearcoat);
                material.Extensions ??= new MaterialExtensions();
                material.Extensions.Clearcoat = new ClearCoat();
                material.Extensions.Clearcoat.ClearcoatFactor = uMaterial.GetFloat(k_CoatMask);

                if (uMaterial.HasProperty(k_CoatMaskMap))
                {
                    var coatMaskTex = uMaterial.GetTexture(k_CoatMaskMap);
                    if (coatMaskTex != null)
                    {
                        if (coatMaskTex is Texture2D)
                        {
                            material.Extensions.Clearcoat.ClearcoatTexture = ExportTextureInfo(coatMaskTex, gltf);
                            ExportTextureTransform(material.Extensions.Clearcoat.ClearcoatTexture, uMaterial, mainTexProperty, gltf);
                        }
                        else
                        {
                            logger?.Error(LogCode.TextureInvalidType, "clearcoat", material.Name);
                            success = false;
                        }
                    }
                }
            }

            if (IsUnlit(uMaterial))
            {
                ExportUnlit(material, uMaterial, mainTexProperty, gltf, logger);
            }
            else
            {
                success &= ExportPbrMetallicRoughness(
                    uMaterial,
                    material,
                    gltf,
                    logger
                );
            }



            return success;
        }

        static bool ExportPbrMetallicRoughness(
            UnityEngine.Material uMaterial,
            Material material,
            IGltfWritable gltf,
            ICodeLogger logger
        )
        {
            var success = true;
            var pbr = new PbrMetallicRoughness { MetallicFactor = 0, RoughnessFactor = 1.0f };

            var metallicUsed = false;
            if (uMaterial.HasProperty(MetallicProperty))
            {
                pbr.MetallicFactor = uMaterial.GetFloat(MetallicProperty);
                metallicUsed = pbr.MetallicFactor > 0;
            }

            if (uMaterial.HasProperty(k_BaseColorMap))
            {
                // TODO if additive particle, render black into alpha
                // TODO use private Material.GetFirstPropertyNameIdByAttribute here, supported from 2020.1+
                var mainTex = uMaterial.GetTexture(k_BaseColorMap);

                if (mainTex)
                {
                    if (mainTex is Texture2D)
                    {
                        pbr.BaseColorTexture = ExportTextureInfo(mainTex, gltf,
                            material.AlphaMode == AlphaMode.Opaque
                                ? ImageFormat.Jpeg
                                : ImageFormat.Unknown
                            );
                        ExportTextureTransform(pbr.BaseColorTexture, uMaterial, k_BaseColorMap, gltf);
                    }
                    else
                    {
                        logger?.Error(LogCode.TextureInvalidType, "main", uMaterial.name);
                        success = false;
                    }
                }
            }

            MaskMapImageExport ormImageExport = null;
            if (uMaterial.IsKeywordEnabled(k_KeywordMaskMap) && uMaterial.HasProperty(k_MaskMap))
            {
                var maskMap = uMaterial.GetTexture(k_MaskMap) as Texture2D;
                if (maskMap != null)
                {

                    var smoothnessUsed = false;
                    if (uMaterial.HasProperty(k_SmoothnessRemapMin))
                    {
                        var smoothnessRemapMin = uMaterial.GetFloat(k_SmoothnessRemapMin);
                        pbr.RoughnessFactor = 1 - smoothnessRemapMin;
                        if (uMaterial.HasProperty(k_SmoothnessRemapMax))
                        {
                            var smoothnessRemapMax = uMaterial.GetFloat(k_SmoothnessRemapMax);
                            smoothnessUsed = math.abs(smoothnessRemapMin - smoothnessRemapMax) > math.EPSILON;
                            if (smoothnessRemapMax < 1 && smoothnessUsed)
                            {
                                logger?.Warning(LogCode.RemapUnsupported, "Smoothness");
                            }
                        }
                    }

                    var occStrength = 1f;
                    if (uMaterial.HasProperty(k_AORemapMin))
                    {
                        var occMin = uMaterial.GetFloat(k_AORemapMin);
                        occStrength = math.clamp(1f - occMin, 0, 1);
                        if (uMaterial.HasProperty(k_AORemapMax))
                        {
                            var occMax = uMaterial.GetFloat(k_AORemapMax);
                            if (occMax < 1f && occStrength > 0)
                            {
                                logger?.Warning(LogCode.RemapUnsupported, "AO");
                            }
                        }
                    }

                    var occUsed = occStrength > 0;

                    // TODO: Detect if metallic/smoothness/occlusion channels
                    // are used based on pixel values (i.e. have non-white
                    // pixels) on top of parameter evaluation

                    if (metallicUsed || occUsed || smoothnessUsed)
                    {
                        ormImageExport = new MaskMapImageExport(maskMap);
                        if (MaterialExport.TryAddImageExport(gltf, ormImageExport, out var ormTextureId))
                        {

                            if (metallicUsed || smoothnessUsed)
                            {
                                pbr.MetallicRoughnessTexture = new TextureInfo
                                {
                                    Index = ormTextureId
                                };
                                ExportTextureTransform(pbr.MetallicRoughnessTexture, uMaterial, k_MaskMap, gltf);
                            }

                            if (occStrength > 0)
                            {
                                // TODO: Detect if occlusion channel is used based
                                // on pixel values
                                // (i.e. have non-white pixels) and not assign the
                                // texture info if not.
                                material.OcclusionTexture = new OcclusionTextureInfo
                                {
                                    Index = ormTextureId,
                                    Strength = occStrength
                                };
                                ExportTextureTransform(
                                    material.OcclusionTexture,
                                    uMaterial,
                                    k_BaseColorMap, // HDRP Lit always re-uses baseColorMap transform
                                    gltf
                                );
                            }
                        }
                    }
                }
            }

            if (uMaterial.HasProperty(BaseColorProperty))
            {
                pbr.BaseColorFactor = uMaterial.GetColor(BaseColorProperty).linear;
            }
            else if (uMaterial.HasProperty(ColorProperty))
            {
                pbr.BaseColorFactor = uMaterial.GetColor(ColorProperty).linear;
            }

            if (ormImageExport == null && uMaterial.HasProperty(SmoothnessProperty))
            {
                pbr.RoughnessFactor = 1f - uMaterial.GetFloat(SmoothnessProperty);
            }

            material.PbrMetallicRoughness = pbr;
            return success;
        }

        protected override bool GetUnlitColor(UnityEngine.Material uMaterial, out UnityEngine.Color baseColor)
        {
            if (uMaterial.HasProperty(k_UnlitColor))
            {
                baseColor = uMaterial.GetColor(k_UnlitColor);
                return true;
            }
            return base.GetUnlitColor(uMaterial, out baseColor);
        }
    }
}

#endif
