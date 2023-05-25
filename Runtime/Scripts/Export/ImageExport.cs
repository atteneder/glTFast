// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GLTFast.Export
{

    /// <inheritdoc />
    class ImageExport : ImageExportBase
    {

        static Material s_ColorBlitMaterial;

        /// <summary>
        /// Main source texture
        /// </summary>
        protected Texture2D m_Texture;

        /// <summary>
        /// Preferred image format
        /// </summary>
        protected ImageFormat m_ImageFormat;

#if UNITY_EDITOR
        /// <summary>
        /// Asset's path
        /// </summary>
        protected string m_AssetPath;

        /// <summary>
        /// True if <seealso cref="m_AssetPath"/> is a valid path
        /// </summary>
        protected bool validAssetPath => !string.IsNullOrEmpty(m_AssetPath) && File.Exists(m_AssetPath);
#endif

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="texture">Main source texture</param>
        /// <param name="imageFormat">Export image format</param>
        public ImageExport(Texture2D texture, ImageFormat imageFormat = ImageFormat.Unknown)
        {
            m_Texture = texture;
            m_ImageFormat = imageFormat;
#if UNITY_EDITOR
            m_AssetPath = AssetDatabase.GetAssetPath(texture);
#endif
        }

        /// <summary>
        /// Final export imageFormat
        /// </summary>
        protected virtual ImageFormat ImageFormat
        {
            get
            {
                if (m_ImageFormat != ImageFormat.Unknown) return m_ImageFormat;
                return HasAlpha(m_Texture) ? ImageFormat.Png : ImageFormat.Jpg;
            }
        }

        /// <inheritdoc />
        public override string FileName
        {
            get
            {
#if UNITY_EDITOR
                if (validAssetPath) {
                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(m_AssetPath);
                    return $"{nameWithoutExtension}.{FileExtension}";
                }
#endif
                var name = m_Texture.name;
                if (string.IsNullOrEmpty(name))
                {
                    name = "texture";
                }
                return $"{name}.{FileExtension}";
            }
        }

        /// <inheritdoc />
        public override FilterMode FilterMode => m_Texture != null ? m_Texture.filterMode : FilterMode.Bilinear;

        /// <inheritdoc />
        public override TextureWrapMode WrapModeU => m_Texture != null ? m_Texture.wrapModeU : TextureWrapMode.Repeat;

        /// <inheritdoc />
        public override TextureWrapMode WrapModeV => m_Texture != null ? m_Texture.wrapModeV : TextureWrapMode.Repeat;

        /// <inheritdoc />
        public override string MimeType
        {
            get
            {
                switch (ImageFormat)
                {
                    case ImageFormat.Jpg:
                        return Constants.mimeTypeJPG;
                    case ImageFormat.Png:
                        return Constants.mimeTypePNG;
                    case ImageFormat.Unknown:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// File extension according to image format
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected string FileExtension
        {
            get
            {
                switch (ImageFormat)
                {
                    case ImageFormat.Jpg:
                        return "jpg";
                    case ImageFormat.Png:
                        return "png";
                    case ImageFormat.Unknown:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Encodes the export texture
        /// </summary>
        /// <param name="imageData">Destination buffer</param>
        /// <returns>True if encoding succeeded, false otherwise</returns>
        protected virtual bool GenerateTexture(out byte[] imageData)
        {
            if (m_Texture != null)
            {
                imageData = EncodeTexture(m_Texture, ImageFormat, blitMaterial: GetColorBlitMaterial());
                return imageData != null;
            }
            imageData = null;
            return false;
        }

        /// <inheritdoc />
        public override bool Write(string filePath, bool overwrite)
        {
#if UNITY_EDITOR
            if (validAssetPath && GetFormatFromExtension(m_AssetPath)==ImageFormat) {
                File.Copy(m_AssetPath, filePath, overwrite);
                return true;
            }
#endif
            if (GenerateTexture(out var imageData))
            {
                File.WriteAllBytes(filePath, imageData);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public override byte[] GetData()
        {
#if UNITY_EDITOR
            if (validAssetPath && GetFormatFromExtension(m_AssetPath)==ImageFormat) {
                return File.ReadAllBytes(m_AssetPath);
            }
#endif
            GenerateTexture(out var imageData);
            return imageData;
        }

        /// <summary>
        /// Default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            var hash = 13;
            if (m_Texture != null)
            {
                hash = hash * 7 + m_Texture.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((ImageExport)obj);
        }

        bool Equals(ImageExport other)
        {
            return m_Texture == other.m_Texture;
        }

        /// <summary>
        /// Creates a blit material from a shader name
        /// </summary>
        /// <param name="shaderName">Name of the shader to be used (without the "Hidden/" prefix)</param>
        /// <returns>Blit material with requested Shader or null, if Shader wasn't found</returns>
        protected static Material LoadBlitMaterial(string shaderName)
        {
            var shader = Shader.Find($"Hidden/{shaderName}");
            if (shader == null)
            {
                Debug.LogError($"Missing Shader {shaderName}");
                return null;
            }
            return new Material(shader);
        }

        static bool HasAlpha(Texture2D texture)
        {
            return GraphicsFormatUtility.HasAlphaChannel(GraphicsFormatUtility.GetGraphicsFormat(texture.format, false));
        }

#if UNITY_EDITOR
        static ImageFormat GetFormatFromExtension(string assetPath) {
            if (assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) {
                return ImageFormat.Png;
            }
            if (assetPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                assetPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                return ImageFormat.Jpg;
            }
            return ImageFormat.Unknown;
        }
#endif

        static Material GetColorBlitMaterial()
        {
            if (s_ColorBlitMaterial == null)
            {
                s_ColorBlitMaterial = LoadBlitMaterial("glTFExportColor");
            }
            return s_ColorBlitMaterial;
        }
    }
}
