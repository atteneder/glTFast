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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GLTFast.Export {
    public abstract class ImageExportBase {

        protected enum Format {
            Unknown,
            Jpg,
            Png
        }

        public abstract string fileName { get; }
        public abstract string mimeType { get; }

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

    public class ImageExport : ImageExportBase {

        protected Texture2D m_Texture;
#if UNITY_EDITOR
        protected string m_AssetPath;
#endif
        
        public ImageExport(Texture2D texture) {
            m_Texture = texture;
#if UNITY_EDITOR
            m_AssetPath = AssetDatabase.GetAssetPath(texture);
#endif
        }
        
        protected Format format {
            get {
#if UNITY_EDITOR
                if (m_AssetPath != null) {
                    return GetFormat(m_AssetPath);
                }
#endif
                // TODO: smart PNG vs Jpeg decision
                return Format.Png;
            }
        }

        public override string fileName {
            get {
#if UNITY_EDITOR
                if (m_AssetPath != null) {
                    return Path.GetFileName(m_AssetPath);
                }
#endif
                return $"{m_Texture.name}.{fileExtension}";
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

        protected virtual byte[] GenerateTexture() {
            return EncodeTexture(m_Texture, format);
        }

        public override void Write(string filePath, bool overwrite) {
#if UNITY_EDITOR
            if (m_AssetPath!=null) {
                File.Copy(m_AssetPath, filePath, overwrite);
            } else
#endif
            if (m_Texture != null) {
                var imageData = GenerateTexture();
                File.WriteAllBytes(filePath,imageData);
            }
        }

        public override byte[] GetData() {
#if UNITY_EDITOR
            if (m_AssetPath!=null) {
                return File.ReadAllBytes(m_AssetPath);
            }
#endif
            if (m_Texture != null) {
                var imageData = EncodeTexture(m_Texture, format);
                return imageData;
            }

            return null;
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            var hash = 13;
            if (m_Texture != null) {
                hash = hash * 7 + m_Texture.GetHashCode();
            }
            return hash;
        }

        public override bool Equals(object obj) {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            return Equals((ImageExport)obj);
        }
        
        bool Equals(ImageExport other) {
            return m_Texture == other.m_Texture;
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

    public class NormalImageExport : ImageExport {
        
        static Material s_NormalBlitMaterial;
        
        public NormalImageExport(Texture2D texture)
            : base(texture) { }

        static Material GetNormalBlitMaterial() {
            if (s_NormalBlitMaterial == null) {
                var normalBlitShader = Shader.Find("Hidden/glTFExportNormal");
                if (normalBlitShader == null) {
                    return null;
                }
                s_NormalBlitMaterial = new Material(normalBlitShader);
            }

            if (s_NormalBlitMaterial == null) {
                Debug.LogError("Missing Shader glTFExportNormal");
            }

            return s_NormalBlitMaterial;
        }

        protected override byte[] GenerateTexture() {
            return EncodeTexture(m_Texture, format, hasAlpha:false, blitMaterial:GetNormalBlitMaterial());
        }
    }
    
    public class OccRoughMetImageExport : ImageExport {
        
        static Material s_BlitMaterial;
        
        public OccRoughMetImageExport(Texture2D texture)
            : base(texture) { }

        static Material GetBlitMaterial() {
            if (s_BlitMaterial == null) {
                var normalBlitShader = Shader.Find("Hidden/glTFExportMetalGloss");
                if (normalBlitShader == null) {
                    return null;
                }
                s_BlitMaterial = new Material(normalBlitShader);
            }

            if (s_BlitMaterial == null) {
                Debug.LogError("Missing Shader glTFExportMetalGloss");
            }

            return s_BlitMaterial;
        }

        public override void Write(string filePath, bool overwrite) {
            if (m_Texture != null) {
                var imageData = GenerateTexture();
                File.WriteAllBytes(filePath,imageData);
            }
        }
        
        protected override byte[] GenerateTexture() {
            return EncodeTexture(m_Texture, format, hasAlpha:false, blitMaterial:GetBlitMaterial());
        }
    }
}