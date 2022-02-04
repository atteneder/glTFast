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
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GLTFast.Export {
    public class ImageExport : ImageExportBase {

        protected Texture2D m_Texture;
        protected Format m_Format;
        
#if UNITY_EDITOR
        protected string m_AssetPath;
        protected bool validAssetPath => !string.IsNullOrEmpty(m_AssetPath) && File.Exists(m_AssetPath);
#endif
        
        public ImageExport(Texture2D texture, Format format = Format.Unknown) {
            m_Texture = texture;
            m_Format = format;
#if UNITY_EDITOR
            m_AssetPath = AssetDatabase.GetAssetPath(texture);
#endif
        }
        
        protected virtual Format format {
            get {
                if (m_Format != Format.Unknown) return m_Format;
                return HasAlpha(m_Texture) ? Format.Png : Format.Jpg;
            }
        }

        public override string fileName {
            get {
#if UNITY_EDITOR
                if (validAssetPath) {
                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(m_AssetPath);
                    return $"{nameWithoutExtension}.{fileExtension}";
                }
#endif
                return $"{m_Texture.name}.{fileExtension}";
            }
        }

        public override FilterMode filterMode => m_Texture != null ? m_Texture.filterMode : FilterMode.Bilinear;
        public override TextureWrapMode wrapModeU => m_Texture != null ? m_Texture.wrapModeU : TextureWrapMode.Repeat;
        public override TextureWrapMode wrapModeV => m_Texture != null ? m_Texture.wrapModeV : TextureWrapMode.Repeat;

        public override string mimeType {
            get {
                return format switch {
                    Format.Jpg => Constants.mimeTypeJPG,
                    Format.Png => Constants.mimeTypePNG,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        protected string fileExtension {
            get {
                return format switch {
                    Format.Jpg => "jpg",
                    Format.Png => "png",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        protected virtual bool GenerateTexture(out byte[] imageData) {
            if (m_Texture != null) {
                imageData = EncodeTexture(m_Texture, format);
                return true;
            }
            imageData = null;
            return false;
        }

        public override void Write(string filePath, bool overwrite) {
#if UNITY_EDITOR
            if (validAssetPath && GetFormatFromExtension(m_AssetPath)==format) {
                File.Copy(m_AssetPath, filePath, overwrite);
            } else
#endif
            if(GenerateTexture(out var imageData)) {
                File.WriteAllBytes(filePath,imageData);
            }
        }

        public override byte[] GetData() {
#if UNITY_EDITOR
            if (validAssetPath) {
                return File.ReadAllBytes(m_AssetPath);
            }
#endif
            GenerateTexture(out var imageData);
            return imageData;
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

        protected static Material LoadBlitMaterial(string shaderName) {
            var shader = Shader.Find($"Hidden/{shaderName}");
            if (shader == null) {
                Debug.LogError($"Missing Shader {shaderName}");
                return null;
            }
            return new Material(shader);
        }
        
        static bool HasAlpha(Texture2D texture) {
            return GraphicsFormatUtility.HasAlphaChannel(GraphicsFormatUtility.GetGraphicsFormat(texture.format, false));
        }
        
#if UNITY_EDITOR
        static Format GetFormatFromExtension(string assetPath) {
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