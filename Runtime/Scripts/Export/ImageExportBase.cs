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
using UnityEngine;

namespace GLTFast.Export {
    
    /// <summary>
    /// Wrapper to export a glTF image from one or more Unity textures
    /// </summary>
    public abstract class ImageExportBase {

        /// <summary>
        /// Exported image file format
        /// </summary>
        public enum Format {
            /// <summary>
            /// Unknown, no preferred file format
            /// </summary>
            Unknown,
            /// <summary>
            /// Jpeg file format
            /// </summary>
            Jpg,
            /// <summary>
            /// PNG (Portable Network Graphics) file format
            /// </summary>
            Png
        }

        /// <summary>
        /// Exported texture's file name
        /// </summary>
        public abstract string fileName { get; }
        
        /// <summary>
        /// Exported texture's mime type
        /// </summary>
        public abstract string mimeType { get; }
        
        /// <summary>
        /// Source texture's filter mode
        /// </summary>
        public abstract FilterMode filterMode { get; }
        
        /// <summary>
        /// Source texture's wrap mode (U direction)
        /// </summary>
        public abstract TextureWrapMode wrapModeU { get; }
        
        /// <summary>
        /// Source texture's wrap mode (V direction)
        /// </summary>
        public abstract TextureWrapMode wrapModeV { get; }
        
        /// <summary>
        /// Writes image file
        /// </summary>
        /// <param name="filePath">Destination file path</param>
        /// <param name="overwrite">If true, existing files will be overwritten</param>
        public abstract bool Write(string filePath, bool overwrite);
        
        /// <summary>
        /// Returns the exported and encoded texture data
        /// </summary>
        /// <returns></returns>
        public abstract byte[] GetData();

        /// <summary>
        /// Encodes the export texture
        /// </summary>
        /// <param name="texture">Main texture to encode</param>
        /// <param name="format">Image format</param>
        /// <param name="hasAlpha">True if the texture has an alpha channel</param>
        /// <param name="blitMaterial">Custom blit material</param>
        /// <returns>Encoded texture data</returns>
        protected static byte[] EncodeTexture(Texture2D texture, Format format, bool hasAlpha = true, Material blitMaterial = null) {

#if UNITY_IMAGECONVERSION
            Texture2D exportTexture;
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
                    RenderTextureReadWrite.Linear
                );
                if (blitMaterial == null) {
                    Graphics.Blit(texture, destRenderTexture);
                } else {
                    Graphics.Blit(texture, destRenderTexture, blitMaterial);
                }
                exportTexture = new Texture2D(
                    texture.width,
                    texture.height,
                    hasAlpha ? TextureFormat.ARGB32 : TextureFormat.RGB24,
                    false,
                    true);
                exportTexture.ReadPixels(new Rect(0, 0, destRenderTexture.width, destRenderTexture.height), 0, 0);
                exportTexture.Apply();
            }
            
            var imageData = format == Format.Png 
                ? exportTexture.EncodeToPNG()
                : exportTexture.EncodeToJPG(60);

            return imageData;
#else
            return null;
#endif
        }
    }
}
