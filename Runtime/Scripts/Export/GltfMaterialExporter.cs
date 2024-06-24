// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Logging;
using GLTFast.Materials;
using GLTFast.Schema;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Material = UnityEngine.Material;

namespace GLTFast.Export
{
    /// <summary>
    /// Converts Unity Materials that use a glTFast shader to glTF materials
    /// </summary>
    public abstract class GltfMaterialExporter : MaterialExportBase
    {
        /// <inheritdoc />
        public override bool ConvertMaterial(
            Material unityMaterial,
            out GLTFast.Schema.Material material,
            IGltfWritable gltf,
            ICodeLogger logger)
        {
            material = new GLTFast.Schema.Material
            {
                name = unityMaterial.name,
                pbrMetallicRoughness = new PbrMetallicRoughness(),
                doubleSided = IsDoubleSided(unityMaterial)
            };

            var alphaMode = GetAlphaMode(unityMaterial);
            material.SetAlphaMode(alphaMode);
            if (alphaMode == MaterialBase.AlphaMode.Mask)
            {
                material.alphaCutoff = GetAlphaCutoff(unityMaterial);
            }

            material = HandlePbrMetallicRoughness(gltf, material, unityMaterial);
            material = HandleNormal(gltf, material, unityMaterial);
            material = HandleOcclusion(gltf, material, unityMaterial);
            material = HandleEmission(gltf, material, unityMaterial);

            return true;
        }

        /// <summary>
        /// Extracts the glTF alpha mode from a Unity material.
        /// </summary>
        /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#_material_alphamode"/>
        /// <param name="material">Unity material.</param>
        /// <returns>glTF alpha mode.</returns>
        protected abstract MaterialBase.AlphaMode GetAlphaMode(Material material);

        /// <summary>
        /// Returns that material's alpha cutoff threshold.
        /// </summary>
        /// <param name="material">Unity material.</param>
        /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#_material_alphacutoff"/>
        /// <returns>Alpha cutoff threshold value.</returns>
        protected abstract float GetAlphaCutoff(Material material);

        /// <summary>
        /// Indicates whether (back-face) culling should be disabled.
        /// </summary>
        /// <seealso href="https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#_material_doublesided"/>
        /// <param name="material">Unity material.</param>
        /// <returns>True if material does not do back-face culling. False otherwise.</returns>
        protected abstract bool IsDoubleSided(Material material);

        static GLTFast.Schema.Material HandlePbrMetallicRoughness(
            IGltfWritable gltf,
            GLTFast.Schema.Material material,
            Material unityMaterial)
        {
            if (TryGetValue(unityMaterial, MaterialProperty.BaseColorTexture, out Texture2D texture2D))
            {
                if (MaterialExport.AddImageExport(gltf, new ImageExport(texture2D), out var textureId))
                {
                    var textureInfo = new TextureInfo
                    {
                        index = textureId,
                        texCoord = GetValue(unityMaterial, MaterialProperty.BaseColorTextureTexCoord)
                    };

                    material.pbrMetallicRoughness.baseColorTexture = textureInfo;

                    if (TryCreateTextureTransform(
                            gltf,
                            unityMaterial,
                            MaterialProperty.BaseColorTextureScaleTransform,
                            MaterialProperty.BaseColorTextureRotation,
                            out var textureTransform
                        ))
                    {
                        material.pbrMetallicRoughness.baseColorTexture.extensions = new TextureInfoExtensions
                        {
                            KHR_texture_transform = textureTransform
                        };
                    }
                }
            }

            if (TryGetValue(unityMaterial, MaterialProperty.BaseColor, out Color baseColor))
            {
                material.pbrMetallicRoughness.BaseColor = baseColor.linear;
            }

            material = HandleMetallicRoughness(gltf, material, unityMaterial);

            return material;
        }

        static GLTFast.Schema.Material HandleMetallicRoughness(
            IGltfWritable gltf,
            GLTFast.Schema.Material material,
            Material unityMaterial)
        {
            if (TryGetValue(unityMaterial, MaterialProperty.MetallicRoughnessMap, out Texture2D texture2D)
                && MaterialExport.AddImageExport(gltf, new ImageExport(texture2D), out var textureId))
            {
                var textureInfo = new TextureInfo
                {
                    index = textureId,
                    texCoord = GetValue(unityMaterial, MaterialProperty.MetallicRoughnessMapTexCoord)
                };

                if (TryCreateTextureTransform(
                        gltf,
                        unityMaterial,
                        MaterialProperty.MetallicRoughnessMapScaleTransform,
                        MaterialProperty.MetallicRoughnessMapRotation,
                        out var textureTransform)
                    )
                {
                    textureInfo.extensions = new TextureInfoExtensions
                    {
                        KHR_texture_transform = textureTransform
                    };
                }

                material.pbrMetallicRoughness.metallicRoughnessTexture = textureInfo;
            }

            if (TryGetValue(unityMaterial, MaterialProperty.Metallic, out float metallicFactor))
            {
                material.pbrMetallicRoughness.metallicFactor = metallicFactor;
            }

            if (TryGetValue(unityMaterial, MaterialProperty.RoughnessFactor, out float roughnessFactor))
            {
                material.pbrMetallicRoughness.roughnessFactor = roughnessFactor;
            }

            return material;
        }

        static GLTFast.Schema.Material HandleNormal(
            IGltfWritable gltf,
            GLTFast.Schema.Material material,
            Material unityMaterial)
        {
            if (!TryGetValue(unityMaterial, MaterialProperty.NormalTexture, out Texture2D texture2D))
            {
                return material;
            }

            if (!MaterialExport.AddImageExport(gltf, new NormalImageExport(texture2D), out var textureId))
            {
                return material;
            }

            TryGetValue(unityMaterial, MaterialProperty.NormalTextureScale, out float normalScale);
            var textureInfo = new NormalTextureInfo
            {
                index = textureId,
                texCoord = GetValue(unityMaterial, MaterialProperty.NormalTextureTexCoord),
                scale = normalScale
            };

            material.normalTexture = textureInfo;

            if (TryCreateTextureTransform(
                    gltf,
                    unityMaterial,
                    MaterialProperty.NormalTextureScaleTransform,
                    MaterialProperty.NormalTextureRotation,
                    out var textureTransform
                ))
            {
                material.normalTexture.extensions = new TextureInfoExtensions
                {
                    KHR_texture_transform = textureTransform
                };
            }

            return material;
        }

        static GLTFast.Schema.Material HandleOcclusion(
            IGltfWritable gltf,
            GLTFast.Schema.Material material,
            Material unityMaterial)
        {
            if (!TryGetValue(unityMaterial, MaterialProperty.OcclusionTexture, out Texture2D texture2D))
            {
                return material;
            }

            if (!MaterialExport.AddImageExport(gltf, new ImageExport(texture2D), out var textureId))
            {
                return material;
            }

            TryGetValue(unityMaterial, MaterialProperty.OcclusionTextureStrength, out float occlusionStrength);
            var info = new OcclusionTextureInfo
            {
                index = textureId,
                texCoord = GetValue(unityMaterial, MaterialProperty.OcclusionTextureTexCoord),
                strength = occlusionStrength
            };

            material.occlusionTexture = info;

            if (TryCreateTextureTransform(
                    gltf,
                    unityMaterial,
                    MaterialProperty.OcclusionTextureScaleTransform,
                    MaterialProperty.OcclusionTextureRotation,
                    out var textureTransform
                ))
            {
                material.occlusionTexture.extensions = new TextureInfoExtensions
                {
                    KHR_texture_transform = textureTransform
                };
            }

            return material;
        }

        static GLTFast.Schema.Material HandleEmission(
            IGltfWritable gltf,
            GLTFast.Schema.Material material,
            Material unityMaterial)
        {
            if (TryGetValue(unityMaterial, MaterialProperty.EmissiveTexture, out Texture2D texture2D))
            {
                if (MaterialExport.AddImageExport(gltf, new ImageExport(texture2D), out var textureId))
                {
                    var info = new TextureInfo
                    {
                        index = textureId,
                        texCoord = GetValue(unityMaterial, MaterialProperty.EmissiveTextureTexCoord)
                    };

                    material.emissiveTexture = info;

                    if (TryCreateTextureTransform(
                            gltf,
                            unityMaterial,
                            MaterialProperty.EmissiveTextureScaleTransform,
                            MaterialProperty.EmissiveTextureRotation,
                            out var textureTransform
                        ))
                    {
                        material.emissiveTexture.extensions = new TextureInfoExtensions
                        {
                            KHR_texture_transform = textureTransform
                        };
                    }
                }
            }

            if (TryGetValue(unityMaterial, MaterialProperty.EmissiveFactor, out Color emissiveFactor))
            {
                material.Emissive = emissiveFactor;
            }

            return material;
        }

        internal static bool TryCreateTextureTransform(
            IGltfWritable gltf,
            Material uMaterial,
            int scaleTransformPropertyId,
            int rotationPropertyId,
            out TextureTransform result
            )
        {
            result = null;
            if (!uMaterial.IsKeywordEnabled("_TEXTURE_TRANSFORM"))
            {
                return false;
            }

            var st = uMaterial.GetVector(scaleTransformPropertyId);
            var r = uMaterial.GetVector(rotationPropertyId);

            if (st.z != 0 || st.w != 0)
            {
                result ??= new TextureTransform();
                result.offset = new[] { st.z, st.w };
            }

            var uvTransform = UvTransform.FromMatrix(new float2x2(st.x, st.y, r.x, r.y));

            if (uvTransform.rotation != 0)
            {
                result ??= new TextureTransform();
                result.rotation = uvTransform.rotation;
            }

            if (math.abs(uvTransform.scale.x - 1) > math.EPSILON
                || math.abs(uvTransform.scale.x - 1) > math.EPSILON)
            {
                result ??= new TextureTransform();
                result.scale = new[] { uvTransform.scale[0], uvTransform.scale[1] };
            }

            if (result != null)
            {
                gltf.RegisterExtensionUsage(Extension.TextureTransform);
                return true;
            }

            return false;
        }

        internal static bool TryGetValue(Material material, int propertyId, out float value)
        {
            if (!material.HasProperty(propertyId))
            {
                value = default;
                return false;
            }
            value = material.GetFloat(propertyId);
            return true;
        }

        internal static bool TryGetValue(Material material, int propertyId, out int value)
        {
            if (TryGetValue(material, propertyId, out float floatValue))
            {
                value = (int)floatValue;
                return true;
            }

            value = default;
            return false;
        }

        internal static int GetValue(Material material, int propertyId)
        {
            return !TryGetValue(material, propertyId, out int value) ? default : value;
        }

        internal static bool TryGetValue(Material material, int propertyId, out Color value)
        {
            if (!material.HasProperty(propertyId))
            {
                value = default;
                return false;
            }
            value = material.GetColor(propertyId);
            return true;
        }

        internal static bool TryGetValue(Material material, int propertyId, out Texture2D value)
        {
            if (!material.HasProperty(propertyId))
            {
                value = default;
                return false;
            }
            value = (Texture2D)material.GetTexture(propertyId);
            return !(value is null);
        }
    }
}
