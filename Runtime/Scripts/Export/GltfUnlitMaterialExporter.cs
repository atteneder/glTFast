// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Materials;
using Unity.Cloud.Gltfast.Objects;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using Color = UnityEngine.Color;
using Material = UnityEngine.Material;

namespace Unity.Cloud.Gltfast.Export
{
    /// <summary>
    /// Converts Unity Materials that use the glTFast shader `glTF/Unlit` to glTF materials
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast.Export", sourceAssembly: "glTFast.Export")]
    public class GltfUnlitMaterialExporter : IMaterialExport
    {
        /// <inheritdoc />
        public bool ConvertMaterial(
            Material unityMaterial,
            out Unity.Cloud.Gltfast.Objects.Material material,
            IGltfWritable gltf,
            ICodeLogger logger)
        {
            gltf.RegisterExtensionUsage(Extension.MaterialsUnlit);

            material = new Unity.Cloud.Gltfast.Objects.Material
            {
                Name = unityMaterial.name,
                Extensions = new MaterialExtensions
                {
                    Unlit = new MaterialUnlit()
                }
            };

            if (GltfMaterialExporter.TryGetValue(unityMaterial, MaterialProperty.Cull, out int cull))
            {
                material.DoubleSided = cull.Equals((int)CullMode.Off);
            }

            material = HandlePbrMetallicRoughness(gltf, material, unityMaterial);

            return true;
        }

        static Unity.Cloud.Gltfast.Objects.Material HandlePbrMetallicRoughness(
            IGltfWritable gltf,
            Unity.Cloud.Gltfast.Objects.Material material,
            Material unityMaterial)
        {
            if (GltfMaterialExporter.TryGetValue(unityMaterial, MaterialProperty.BaseColorTexture, out Texture2D texture2D))
            {
                if (MaterialExport.TryAddImageExport(gltf, new ImageExport(texture2D), out var textureId))
                {
                    var textureInfo = new TextureInfo
                    {
                        Index = textureId,
                        TexCoord = GltfMaterialExporter.GetValue(unityMaterial, MaterialProperty.BaseColorTextureTexCoord)
                    };

                    material.PbrMetallicRoughness ??= new PbrMetallicRoughness();
                    material.PbrMetallicRoughness.BaseColorTexture = textureInfo;

                    if (GltfMaterialExporter.TryCreateTextureTransform(
                            gltf,
                            unityMaterial,
                            MaterialProperty.BaseColorTextureScaleTransform,
                            MaterialProperty.BaseColorTextureRotation,
                            out var textureTransform
                        ))
                    {
                        material.PbrMetallicRoughness.BaseColorTexture.Extensions = new TextureInfoExtensions
                        {
                            TextureTransform = textureTransform
                        };
                    }
                }
            }

            if (GltfMaterialExporter.TryGetValue(unityMaterial, MaterialProperty.BaseColor, out Color baseColor)
                && baseColor != Color.white)
            {
                material.PbrMetallicRoughness ??= new PbrMetallicRoughness();
                material.PbrMetallicRoughness.BaseColorFactor = baseColor.linear;
            }

            return material;
        }
    }
}
