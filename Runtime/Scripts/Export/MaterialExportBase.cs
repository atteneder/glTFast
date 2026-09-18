// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast.Export
{

    using Logging;
    using Objects;

    /// <inheritdoc cref="IMaterialExport"/>
    [MovedFrom(true, sourceNamespace: "GLTFast.Export", sourceAssembly: "glTFast.Export")]
    public abstract class MaterialExportBase : IMaterialExport
    {
        // These property IDs might be useful for developing custom IMaterialExport implementations,
        // thus they are public.
        // ReSharper disable MemberCanBeProtected.Global
        // ReSharper disable MemberCanBePrivate.Global

        /// <summary>
        /// _BaseColor shader property identifier
        /// </summary>
        public static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        /// <summary>
        /// _MainTex shader property identifier
        /// </summary>
        public static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");

        /// <summary>
        /// _Color shader property identifier
        /// </summary>
        public static readonly int ColorProperty = Shader.PropertyToID("_Color");

        /// <summary>
        /// _Metallic shader property identifier
        /// </summary>
        public static readonly int MetallicProperty = Shader.PropertyToID("_Metallic");

        /// <summary>
        /// _Smoothness shader property identifier
        /// </summary>
        public static readonly int SmoothnessProperty = Shader.PropertyToID("_Smoothness");

        /// <summary>
        /// _Cutoff shader property identifier
        /// </summary>
        public static readonly int CutoffProperty = Shader.PropertyToID("_Cutoff");

        // ReSharper restore MemberCanBeProtected.Global
        // ReSharper restore MemberCanBePrivate.Global

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
        protected static void SetAlphaModeAndCutoff(UnityEngine.Material uMaterial, Material material)
        {
            switch (uMaterial.GetTag("RenderType", false, ""))
            {
                case "TransparentCutout":
                    if (uMaterial.HasProperty(CutoffProperty))
                    {
                        material.AlphaCutoff = uMaterial.GetFloat(CutoffProperty);
                    }
                    material.AlphaMode = AlphaMode.Mask;
                    break;
                case "Transparent":
                case "Fade":
                    material.AlphaMode = AlphaMode.Blend;
                    break;
                default:
                    material.AlphaMode = AlphaMode.Opaque;
                    break;
            }
        }

        /// <summary>
        /// Retrieves whether material is double-sided.
        /// </summary>
        /// <param name="uMaterial">Material to analyze.</param>
        /// <param name="cullPropId">CullMode property id.</param>
        /// <returns>True if material is double-sided, false otherwise.</returns>
        protected static bool IsDoubleSided(UnityEngine.Material uMaterial, int cullPropId)
        {
            return uMaterial.HasProperty(cullPropId) &&
                uMaterial.GetInt(cullPropId) == (int)CullMode.Off;
        }

        /// <summary>
        /// Retrieves whether material is unlit
        /// </summary>
        /// <param name="material">Material to analyze</param>
        /// <returns>True if material uses unlit shader, false otherwise</returns>
        protected static bool IsUnlit(UnityEngine.Material material)
        {
            return material.shader.name.ToLowerInvariant().Contains("unlit");
        }

        /// <summary>
        /// Converts an unlit Unity material into a glTF material
        /// </summary>
        /// <param name="material">Destination glTF material</param>
        /// <param name="uMaterial">Source Unity material</param>
        /// <param name="mainTexProperty">Main texture property ID</param>
        /// <param name="gltf">Context glTF to export to</param>
        /// <param name="logger">Custom logger</param>
        protected void ExportUnlit(
            Material material,
            UnityEngine.Material uMaterial,
            int mainTexProperty,
            IGltfWritable gltf,
            ICodeLogger logger
            )
        {

            gltf.RegisterExtensionUsage(Extension.MaterialsUnlit);
            material.Extensions = material.Extensions ?? new MaterialExtensions();
            material.Extensions.Unlit = new MaterialUnlit();

            var pbr = material.PbrMetallicRoughness ?? new PbrMetallicRoughness();

            if (GetUnlitColor(uMaterial, out var baseColor))
            {
                pbr.BaseColorFactor = baseColor.linear;
            }

            if (uMaterial.HasProperty(mainTexProperty))
            {
                var mainTex = uMaterial.GetTexture(mainTexProperty);
                if (mainTex != null)
                {
                    if (mainTex is Texture2D)
                    {
                        pbr.BaseColorTexture = ExportTextureInfo(mainTex, gltf);
                        if (pbr.BaseColorTexture != null)
                        {
                            ExportTextureTransform(pbr.BaseColorTexture, uMaterial, mainTexProperty, gltf);
                        }
                    }
                    else
                    {
                        logger?.Error(LogCode.TextureInvalidType, "main", material.Name);
                    }
                }
            }

            material.PbrMetallicRoughness = pbr;
        }

        /// <summary>
        /// Returns the color of an unlit material
        /// </summary>
        /// <param name="uMaterial">Unity material</param>
        /// <param name="baseColor">Resulting unlit color</param>
        /// <returns>True if the unlit color was retrieved, false otherwise</returns>
        protected virtual bool GetUnlitColor(UnityEngine.Material uMaterial, out UnityEngine.Color baseColor)
        {
            if (uMaterial.HasProperty(BaseColorProperty))
            {
                baseColor = uMaterial.GetColor(BaseColorProperty);
                return true;
            }
            if (uMaterial.HasProperty(ColorProperty))
            {
                baseColor = uMaterial.GetColor(ColorProperty);
                return true;
            }
            baseColor = UnityEngine.Color.magenta;
            return false;
        }

        /// <summary>
        /// Export a Unity texture to a glTF.
        /// </summary>
        /// <param name="texture">Texture to export.</param>
        /// <param name="gltf">Context glTF to export to</param>
        /// <param name="format">Desired image format</param>
        /// <returns>glTF texture info</returns>
        protected static TextureInfo ExportTextureInfo(
            UnityEngine.Texture texture,
            IGltfWritable gltf,
            ImageFormat format = ImageFormat.Unknown
            )
        {
            var texture2d = texture as Texture2D;
            if (texture2d == null)
            {
                return null;
            }
            var imageExport = new ImageExport(texture2d, format);
            if (MaterialExport.TryAddImageExport(gltf, imageExport, out var textureId))
            {
                return new TextureInfo
                {
                    Index = textureId,
                    // TexCoord = 0 // TODO: figure out which UV set was used
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
        /// <param name="normalScalePropId">Normal scale property ID</param>
        /// <returns>glTF texture info</returns>
        protected static NormalTextureInfo ExportNormalTextureInfo(
            UnityEngine.Texture texture,
            UnityEngine.Material material,
            IGltfWritable gltf,
            int normalScalePropId
        )
        {
            var texture2d = texture as Texture2D;
            if (texture2d == null)
            {
                return null;
            }
            var imageExport = new NormalImageExport(texture2d);
            if (MaterialExport.TryAddImageExport(gltf, imageExport, out var textureId))
            {
                var info = new NormalTextureInfo
                {
                    Index = textureId,
                    // TexCoord = 0 // TODO: figure out which UV set was used
                };

                if (material.HasProperty(normalScalePropId))
                {
                    info.Scale = material.GetFloat(normalScalePropId);
                }
                return info;
            }
            return null;
        }

        /// <summary>
        /// Calculates a texture's transform and adds a KHR_texture_transform glTF extension, if required
        /// </summary>
        /// <param name="def">glTF TextureInfo to edit</param>
        /// <param name="mat">Source Material</param>
        /// <param name="texPropertyId">Texture property to fetch transformation from</param>
        /// <param name="gltf">Context glTF to export to (for registering extension usage)</param>
        protected static void ExportTextureTransform(TextureInfo def, UnityEngine.Material mat, int texPropertyId, IGltfWritable gltf)
        {
            var offset = mat.GetTextureOffset(texPropertyId);
            var scale = mat.GetTextureScale(texPropertyId);

            if (offset != Vector2.zero || scale != Vector2.one)
            {
                gltf.RegisterExtensionUsage(Extension.TextureTransform);
                def.SetTextureTransform(new TextureTransform
                {
                    Scale = new float2(scale.x, scale.y),
                    Offset = new float2(offset.x, 1 - offset.y - scale.y)
                });
            }
        }
    }
}
