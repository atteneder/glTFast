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

using System;
using GLTFast.Materials;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace GLTFast.Export {
	
    using Logging;
    using Schema;

    /// <inheritdoc cref="IMaterialExport"/>
    public abstract class MaterialExportBase : IMaterialExport {
        
        /// <summary>
        /// _BaseColor property
        /// </summary>
        protected static readonly int k_BaseColor = Shader.PropertyToID("_BaseColor");
        
        /// <summary>
        /// _MainTex property
        /// </summary>
        protected static readonly int k_MainTex = Shader.PropertyToID("_MainTex");
        
        /// <summary>
        /// _Color property
        /// </summary>
        protected static readonly int k_Color = Shader.PropertyToID("_Color");
        
        /// <summary>
        /// _Metallic property
        /// </summary>
        protected static readonly int k_Metallic = Shader.PropertyToID("_Metallic");
        
        /// <summary>
        /// _Smoothness property
        /// </summary>
        protected static readonly int k_Smoothness = Shader.PropertyToID("_Smoothness");

        static readonly int k_Cutoff = Shader.PropertyToID("_Cutoff");
        static readonly int k_Cull = Shader.PropertyToID("_Cull");

        /// <summary>
        /// Converts a Unity material into a glTF material
        /// </summary>
        /// <param name="uMaterial">Source material</param>
        /// <param name="material">Resulting glTF material</param>
        /// <param name="gltf">glTF to export material to. Will be used to add required texture images</param>
        /// <param name="logger">Custom logger</param>
        /// <returns>True if material was converted successfully, false otherwise</returns>
        public abstract bool ConvertMaterial(UnityEngine.Material uMaterial, out Material material, IGltfWritable gltf, ICodeLogger logger);
        
        /// <summary>
        /// Applies alpha mode and cutoff
        /// </summary>
        /// <param name="uMaterial">Source Unity Material</param>
        /// <param name="material">glTF material to apply settings on</param>
        protected static void SetAlphaModeAndCutoff(UnityEngine.Material uMaterial, Material material) {
            switch (uMaterial.GetTag("RenderType", false, ""))
            {
                case "TransparentCutout":
                    if (uMaterial.HasProperty(k_Cutoff))
                    {
                        material.alphaCutoff = uMaterial.GetFloat(k_Cutoff);
                    }
                    material.alphaModeEnum = Material.AlphaMode.MASK;
                    break;
                case "Transparent":
                case "Fade":
                    material.alphaModeEnum = Material.AlphaMode.BLEND;
                    break;
                default:
                    material.alphaModeEnum = Material.AlphaMode.OPAQUE;
                    break;
            }
        }

        /// <summary>
        /// Retrieves whether material is double-sided.
        /// </summary>
        /// <param name="uMaterial">Material to analyze.</param>
        /// <returns>True if material is double-sided, false otherwise.</returns>
        protected static bool IsDoubleSided(UnityEngine.Material uMaterial) {
            return uMaterial.HasProperty(k_Cull) &&
                uMaterial.GetInt(k_Cull) == (int) CullMode.Off;
        }
        
        /// <summary>
        /// Retrieves whether material is unlit
        /// </summary>
        /// <param name="material">Material to analyze</param>
        /// <returns>True if material uses unlit shader, false otherwise</returns>
        protected static bool IsUnlit(UnityEngine.Material material) {
            return material.shader.name.ToLower().Contains("unlit");
        }
        
        /// <summary>
        /// Converts an unlit Unity material into a glTF material 
        /// </summary>
        /// <param name="material">Destination glTF material</param>
        /// <param name="uMaterial">Source Unity material</param>
        /// <param name="mainTexProperty">Main texture property ID</param>
        /// <param name="gltf">Context glTF to export to</param>
        /// <param name="logger">Custom logger</param>
        protected static void ExportUnlit(
            Material material,
            UnityEngine.Material uMaterial,
            int mainTexProperty,
            IGltfWritable gltf,
            ICodeLogger logger
            )
        {

            gltf.RegisterExtensionUsage(Extension.MaterialsUnlit);
            material.extensions = material.extensions ?? new MaterialExtension();
            material.extensions.KHR_materials_unlit = new MaterialUnlit();
	        
            var pbr = material.pbrMetallicRoughness ?? new PbrMetallicRoughness();

            if (uMaterial.HasProperty(k_Color)) {
                pbr.baseColor = uMaterial.GetColor(k_Color);
            }

            if (uMaterial.HasProperty(mainTexProperty)) {
                var mainTex = uMaterial.GetTexture(mainTexProperty);
                if (mainTex != null) {
                    if(mainTex is Texture2D) {
                        pbr.baseColorTexture = ExportTextureInfo(mainTex, gltf);
                        if (pbr.baseColorTexture != null) {
                            ExportTextureTransform(pbr.baseColorTexture, uMaterial, mainTexProperty, gltf);
                        }
                    } else {
                        logger?.Error(LogCode.TextureInvalidType, "main", material.name );
                    }
                }
            }

            material.pbrMetallicRoughness = pbr;
        }
        
        /// <summary>
        /// Export a Unity texture to a glTF.
        /// </summary>
        /// <param name="texture">Texture to export.</param>
        /// <param name="gltf">Context glTF to expor to</param>
        /// <param name="format">Desired image format</param>
        /// <returns>glTF texture info</returns>
        protected static TextureInfo ExportTextureInfo(
            UnityEngine.Texture texture,
            IGltfWritable gltf,
            ImageExportBase.Format format = ImageExportBase.Format.Unknown
            )
        {
            var texture2d = texture as Texture2D;
            if (texture2d == null) {
                return null;
            }
            var imageExport = new ImageExport(texture2d, format);
            if (AddImageExport(gltf, imageExport, out var textureId)) {
                return new TextureInfo {
                    index = textureId,
                    // texCoord = 0 // TODO: figure out which UV set was used
                };
            }
            return null;
        }
        
        /// <summary>
        /// Export a normal texture from Unity to glTF.
        /// </summary>
        /// <param name="texture">Normal texture to export</param>
        /// <param name="material">Material the normal is used on</param>
        /// <param name="gltf">Context glTF to export to</param>
        /// <returns>glTF texture info</returns>
        protected static NormalTextureInfo ExportNormalTextureInfo(
            UnityEngine.Texture texture,
            UnityEngine.Material material,
            IGltfWritable gltf
        )
        {
            var texture2d = texture as Texture2D;
            if (texture2d == null) {
                return null;
            }
            var imageExport = new NormalImageExport(texture2d);
            if (AddImageExport(gltf, imageExport, out var textureId)) {
                var info = new NormalTextureInfo {
                    index = textureId,
                    // texCoord = 0 // TODO: figure out which UV set was used
                };

                if (material.HasProperty(MaterialGenerator.bumpScalePropId)) {
                    info.scale = material.GetFloat(MaterialGenerator.bumpScalePropId);
                }
                return info;
            }
            return null;
        }
        
        /// <summary>
        /// Adds an ImageExport to the glTF.
        /// No conversions or channel swizzling 
        /// </summary>
        /// <param name="gltf"></param>
        /// <param name="imageExport"></param>
        /// <param name="textureId"></param>
        /// <returns>glTF texture ID</returns>
        protected static bool AddImageExport(IGltfWritable gltf, ImageExportBase imageExport, out int textureId) {
            var imageId = gltf.AddImage(imageExport);
            if (imageId < 0) {
                textureId = -1;
                return false;
            }

            var samplerId = gltf.AddSampler(imageExport.filterMode, imageExport.wrapModeU, imageExport.wrapModeV);
            textureId = gltf.AddTexture(imageId,samplerId);
            return true;
        }
        
        /// <summary>
        /// Calculates a texture's transform and adds a KHR_texture_transform glTF extension, if required
        /// </summary>
        /// <param name="def">glTF TextureInfo to edit</param>
        /// <param name="mat">Source Material</param>
        /// <param name="texPropertyId">Texture property to fetch transformation from</param>
        /// <param name="gltf">Context glTF to export to (for registering extension usage)</param>
        protected static void ExportTextureTransform(TextureInfo def, UnityEngine.Material mat, int texPropertyId, IGltfWritable gltf) {
            var offset = mat.GetTextureOffset(texPropertyId);
            var scale = mat.GetTextureScale(texPropertyId);

            // Counter measure for Unity/glTF texture coordinate difference
            // TODO: Offer UV conversion as alternative
            offset.y = 1 - offset.y;
            scale.y *= -1;

            if (offset != Vector2.zero || scale != Vector2.one) {
                gltf.RegisterExtensionUsage(Extension.TextureTransform);
                def.extensions = def.extensions ?? new TextureInfoExtension();
                def.extensions.KHR_texture_transform = new TextureTransform {
                    scale = new[] { scale.x, scale.y },
                    offset = new[] { offset.x, offset.y }
                };
            }
        }
    }
}
