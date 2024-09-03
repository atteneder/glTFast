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
    /// Converts Unity Materials that use the glTFast shader `glTF/Unlit` to glTF materials
    /// </summary>
    public class GltfUnlitMaterialExporter : IMaterialExport
    {
        /// <inheritdoc />
        public bool ConvertMaterial(
            Material unityMaterial,
            out GLTFast.Schema.Material material,
            IGltfWritable gltf,
            ICodeLogger logger)
        {
            gltf.RegisterExtensionUsage(Extension.MaterialsUnlit);

            material = new GLTFast.Schema.Material
            {
                name = unityMaterial.name,
                extensions = new MaterialExtensions
                {
                    KHR_materials_unlit = new MaterialUnlit()
                }
            };

            if (GltfMaterialExporter.TryGetValue(unityMaterial, MaterialProperty.Cull, out int cull))
            {
                material.doubleSided = cull.Equals((int)CullMode.Off);
            }

            material = HandlePbrMetallicRoughness(gltf, material, unityMaterial);

            return true;
        }

        static GLTFast.Schema.Material HandlePbrMetallicRoughness(
            IGltfWritable gltf,
            GLTFast.Schema.Material material,
            Material unityMaterial)
        {
            if (GltfMaterialExporter.TryGetValue(unityMaterial, MaterialProperty.BaseColorTexture, out Texture2D texture2D))
            {
                if (MaterialExport.AddImageExport(gltf, new ImageExport(texture2D), out var textureId))
                {
                    var textureInfo = new TextureInfo
                    {
                        index = textureId,
                        texCoord = GltfMaterialExporter.GetValue(unityMaterial, MaterialProperty.BaseColorTextureTexCoord)
                    };

                    material.pbrMetallicRoughness ??= new PbrMetallicRoughness();
                    material.pbrMetallicRoughness.baseColorTexture = textureInfo;

                    if (GltfMaterialExporter.TryCreateTextureTransform(
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

            if (GltfMaterialExporter.TryGetValue(unityMaterial, MaterialProperty.BaseColor, out Color baseColor)
                && baseColor != Color.white)
            {
                material.pbrMetallicRoughness ??= new PbrMetallicRoughness();
                material.pbrMetallicRoughness.BaseColor = baseColor.linear;
            }

            return material;
        }
    }
}
