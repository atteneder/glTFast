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
    public abstract class ImageExportBase {

        public enum Format {
            Unknown,
            Jpg,
            Png
        }

        protected enum Channels {
            RGB,
            RGBA
        }
        
        public abstract string fileName { get; }
        public abstract string mimeType { get; }
        public abstract FilterMode filterMode { get; }
        public abstract TextureWrapMode wrapModeU { get; }
        public abstract TextureWrapMode wrapModeV { get; }
        
        public abstract void Write(string filePath, bool overwrite);
        public abstract byte[] GetData();

        protected static byte[] EncodeTexture(Texture2D texture, Format format, bool hasAlpha = true, Material blitMaterial = null) {

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
        }
    }
}
