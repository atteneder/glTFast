// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
#if UNITY_2022_1_OR_NEWER
using UnityEngine.Experimental.Rendering;
#endif
using Object = UnityEngine.Object;

namespace GLTFast.Export
{

    /// <summary>
    /// Wrapper to export a glTF image from one or more Unity textures
    /// </summary>
    public abstract class ImageExportBase
    {
        /// <summary>
        /// Exported texture's file name
        /// </summary>
        public abstract string FileName { get; }

        /// <summary>
        /// Exported texture's mime type
        /// </summary>
        public abstract string MimeType { get; }

        /// <summary>
        /// Source texture's filter mode
        /// </summary>
        public abstract FilterMode FilterMode { get; }

        /// <summary>
        /// Source texture's wrap mode (U direction)
        /// </summary>
        public abstract TextureWrapMode WrapModeU { get; }

        /// <summary>
        /// Source texture's wrap mode (V direction)
        /// </summary>
        public abstract TextureWrapMode WrapModeV { get; }

        /// <summary>
        /// Writes image file
        /// </summary>
        /// <param name="filePath">Destination file path</param>
        /// <param name="overwrite">If true, existing files will be overwritten</param>
        /// <returns>True if writing succeeded, false otherwise</returns>
        public abstract bool Write(string filePath, bool overwrite);

        /// <summary>
        /// Returns the exported and encoded texture data
        /// </summary>
        /// <returns>Encoded texture data.</returns>
        public abstract byte[] GetData();

        /// <summary>
        /// Encodes the export texture
        /// </summary>
        /// <param name="texture">Main texture to encode</param>
        /// <param name="format">Image format</param>
        /// <param name="hasAlpha">True if the texture has an alpha channel</param>
        /// <param name="blitMaterial">Custom blit material</param>
        /// <returns>Encoded texture data</returns>
        protected static byte[] EncodeTexture(Texture2D texture, ImageFormat format, bool hasAlpha = true, Material blitMaterial = null)
        {

#if UNITY_IMAGECONVERSION
            Texture2D exportTexture;
            var tmpTexture = false;

            if (texture.isReadable && blitMaterial==null) {
                exportTexture = texture;
                if (exportTexture == null) {
                    // m_Logger?.Error(LogCode.ImageFormatUnknown,texture.name,"n/a");
                    return null;
                }
            } else {
                var destRenderTexture = RenderTexture.GetTemporary(
                    texture.width,
                    texture.height,
                    0,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear,
                    1,
                    RenderTextureMemoryless.Depth
                );
                if (blitMaterial == null) {
                    Graphics.Blit(texture, destRenderTexture);
                } else {
                    Graphics.Blit(texture, destRenderTexture, blitMaterial);
                }
                exportTexture = new Texture2D(
                    texture.width,
                    texture.height,
#if UNITY_2023_2_OR_NEWER
                    // ~20 times faster texture construction
                    !hasAlpha && SystemInfo.IsFormatSupported(GraphicsFormat.R8G8B8_UNorm, GraphicsFormatUsage.Sample) ?  GraphicsFormat.R8G8B8_UNorm : GraphicsFormat.R8G8B8A8_UNorm,
                    TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate
#elif UNITY_2022_1_OR_NEWER
                    // ~20 times faster texture construction
                    !hasAlpha && SystemInfo.IsFormatSupported(GraphicsFormat.R8G8B8_UNorm, FormatUsage.Sample) ?  GraphicsFormat.R8G8B8_UNorm : GraphicsFormat.R8G8B8A8_UNorm,
                    TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate
#else
                    hasAlpha ? TextureFormat.ARGB32 : TextureFormat.RGB24,
                    false,
                    true
#endif
                );
                exportTexture.ReadPixels(new Rect(0, 0, destRenderTexture.width, destRenderTexture.height), 0, 0);
                RenderTexture.ReleaseTemporary(destRenderTexture);
                exportTexture.Apply();
                tmpTexture = true;
            }

            var imageData = format == ImageFormat.Png
                ? exportTexture.EncodeToPNG()
                : exportTexture.EncodeToJPG(60);

            if (tmpTexture) {
                // Release temporary texture
#if UNITY_EDITOR
                Object.DestroyImmediate(exportTexture);
#else
                Object.Destroy(exportTexture);
#endif
            }
            return imageData;
#else
            return null;
#endif
        }
    }
}
