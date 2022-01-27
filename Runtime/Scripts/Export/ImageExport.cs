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
using System.IO;
using UnityEngine;

namespace GLTFast.Export {
    public abstract class ImageExportBase {

        protected enum Format {
            Unknown,
            Jpg,
            Png
        }

        public abstract string fileName { get; }
        public abstract string mimeType { get; }

        public abstract string GetAssetPath();
        public abstract void Write(string filePath, bool overwrite);
        public abstract byte[] GetData();

        protected static byte[] EncodeTexture(Texture2D texture, Format format) {

            Texture2D exportTexture;
            if (texture.isReadable) {
                exportTexture = texture as Texture2D;
                if (exportTexture == null) {
                    // m_Logger?.Error(LogCode.ImageFormatUnknown,texture.name,"n/a");
                    return null;
                }
            } else {
                var destRenderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(texture, destRenderTexture);
                exportTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false, true);
                exportTexture.ReadPixels(new Rect(0, 0, destRenderTexture.width, destRenderTexture.height), 0, 0);
                exportTexture.Apply();
            }
            
            var imageData = format == Format.Png 
                ? exportTexture.EncodeToPNG()
                : exportTexture.EncodeToJPG(60);

            return imageData;
        }
    }

    public class ImageExport : ImageExportBase {

        Texture2D texture;

        public ImageExport(Texture2D texture) {
            this.texture = texture;
        }
        
#if UNITY_EDITOR
        string assetPath;
        public ImageExport(string assetPath) {
            this.assetPath = assetPath;
        }
#endif
        
        protected Format format {
            get {
#if UNITY_EDITOR
                if (assetPath != null) {
                    return GetFormat(assetPath);
                }
#endif
                // TODO: smart PNG vs Jpeg decision
                return Format.Png;
            }
        }

        public override string GetAssetPath() {
            return assetPath;
        }

        public override string fileName {
            get {
#if UNITY_EDITOR
                if (assetPath != null) {
                    return Path.GetFileName(assetPath);
                }
#endif
                return $"{texture.name}.{fileExtension}";
            }
        }

        public override string mimeType {
            get {
                return format switch {
                    Format.Jpg => Constants.mimeTypeJPG,
                    Format.Png => Constants.mimeTypePNG,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        string fileExtension {
            get {
                return format switch {
                    Format.Jpg => "jpg",
                    Format.Png => "png",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public override void Write(string filePath, bool overwrite) {
#if UNITY_EDITOR
            if (assetPath!=null && File.Exists(assetPath)) {
                File.Copy(assetPath, filePath, overwrite);
            } else
#endif
            if (texture != null) {
                var imageData = EncodeTexture(texture, format);
                File.WriteAllBytes(filePath,imageData);
            }
        }

        public override byte[] GetData() {
#if UNITY_EDITOR
            if (assetPath!=null && File.Exists(assetPath)) {
                return File.ReadAllBytes(assetPath);
            } else
#endif
            if (texture != null) {
                var imageData = EncodeTexture(texture, format);
                return imageData;
            }

            return null;
        }

        public override int GetHashCode() {
            var hash = 13;
            if (texture != null) {
                hash = hash * 7 + texture.GetHashCode();
            }
            if (assetPath != null) {
                hash = hash * 7 + assetPath.GetHashCode();
            }
            return hash;
        }

        public override bool Equals(object obj) {
            //Check for null and compare run-time types.
            if (obj == null || ! GetType().Equals(obj.GetType())) {
                return false;
            }
            return Equals((ImageExport)obj);
        }
        
        bool Equals(ImageExport other) {
            return texture == other.texture
                && (
                    assetPath == null && other.assetPath == null
                    || (assetPath != null && assetPath.Equals(other.assetPath))
                );
        }
        
#if UNITY_EDITOR
        static Format GetFormat(string assetPath) {
            if (assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) {
                return Format.Png;
            }
            if (assetPath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                assetPath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                return Format.Jpg;
            }
            return Format.Unknown;
        }
#endif
    }
}