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
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Assert = UnityEngine.Assertions.Assert;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        protected virtual byte[] GenerateTexture() {
            return EncodeTexture(m_Texture, format);
        }

        public override void Write(string filePath, bool overwrite) {
#if UNITY_EDITOR
            if (validAssetPath && GetFormatFromExtension(m_AssetPath)==format) {
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
            if (validAssetPath) {
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

    public class NormalImageExport : ImageExport {
        
        static Material s_NormalBlitMaterial;
        
        public NormalImageExport(Texture2D texture)
            : base(texture) { }

        static Material GetNormalBlitMaterial() {
            if (s_NormalBlitMaterial == null) {
                s_NormalBlitMaterial = LoadBlitMaterial("glTFExportNormal");
            }
            return s_NormalBlitMaterial;
        }

        protected override byte[] GenerateTexture() {
            return EncodeTexture(m_Texture, format, hasAlpha:false, blitMaterial:GetNormalBlitMaterial());
        }
    }
    
    public class OrmImageExport : ImageExport {
        
        static Material s_MetalGlossBlitMaterial;
        static Material s_OcclusionBlitMaterial;
        static Material s_GlossBlitMaterial;

        Texture2D m_OccTexture;
        Texture2D m_SmoothnessTexture;

        public OrmImageExport(
            Texture2D metalGlossTexture = null,
            Texture2D occlusionTexture = null,
            Texture2D smoothnessTexture = null)
            : base(metalGlossTexture) 
        {
            m_OccTexture = occlusionTexture;
            m_SmoothnessTexture = smoothnessTexture;
        }
        
        public override string fileName {
            get {
                if (m_Texture != null) return base.fileName;
                if (m_OccTexture != null) return $"{m_OccTexture.name}.{fileExtension}";
                return $"{m_SmoothnessTexture.name}ORM.{fileExtension}";
            }
        }
        
        protected override Format format => m_Format != Format.Unknown ? m_Format : Format.Jpg;

        public bool hasOcclusion => m_OccTexture != null;

        public void SetMetalGlossTexture(Texture2D texture) {
            m_Texture = texture;
        }
        
        public void SetOcclusionTexture(Texture2D texture) {
            m_OccTexture = texture;
        }
        
        public void SetSmoothnessTexture(Texture2D texture) {
            m_SmoothnessTexture = texture;
        }
        
        static Material GetMetalGlossBlitMaterial() {
            if (s_MetalGlossBlitMaterial == null) {
                s_MetalGlossBlitMaterial = LoadBlitMaterial("glTFExportMetalGloss");
            }
            return s_MetalGlossBlitMaterial;
        }
        
        static Material GetOcclusionBlitMaterial() {
            if (s_OcclusionBlitMaterial == null) {
                s_OcclusionBlitMaterial = LoadBlitMaterial("glTFExportOcclusion");
            }
            return s_OcclusionBlitMaterial;
        }

        
        static Material GetGlossBlitMaterial() {
            if (s_GlossBlitMaterial == null) {
                s_GlossBlitMaterial = LoadBlitMaterial("glTFExportSmoothness");
            }
            return s_GlossBlitMaterial;
        }

        public override void Write(string filePath, bool overwrite) {
            if (m_Texture != null || m_OccTexture!=null || m_SmoothnessTexture!=null) {
                var imageData = GenerateTexture();
                File.WriteAllBytes(filePath,imageData);
            }
        }
        
        protected override byte[] GenerateTexture() {
            return EncodeOrmTexture(m_Texture, m_OccTexture, m_SmoothnessTexture, format);
        }
        
        protected static byte[] EncodeOrmTexture(
            Texture2D metalGlossTexture,
            Texture2D occlusionTexture,
            Texture2D smoothnessTexture,
            Format format
            )
        {
            Assert.IsTrue(metalGlossTexture!=null || occlusionTexture!=null || smoothnessTexture!=null);
            var blitMaterial = GetMetalGlossBlitMaterial();

            var width = int.MinValue;
            var height = int.MinValue;

            if (metalGlossTexture != null) {
                width = math.max( width, metalGlossTexture.width);
                height = math.max(height, metalGlossTexture.height);
            }
            
            if (occlusionTexture != null) {
                width = math.max( width, occlusionTexture.width);
                height = math.max(height, occlusionTexture.height);
            }
            
            if (smoothnessTexture != null) {
                width = math.max( width, smoothnessTexture.width);
                height = math.max(height, smoothnessTexture.height);
            }
            
            var destRenderTexture = RenderTexture.GetTemporary(
                width,
                height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear
            );

            if (metalGlossTexture == null) {
                var rt = RenderTexture.active;
                RenderTexture.active = destRenderTexture;
                GL.Clear(true, true, Color.white);
                RenderTexture.active = rt;
            }
            else {
                Graphics.Blit(metalGlossTexture, destRenderTexture, blitMaterial);
            }
            if (occlusionTexture != null) {
                blitMaterial = GetOcclusionBlitMaterial();
                Graphics.Blit(occlusionTexture, destRenderTexture, blitMaterial);
            }
            if (smoothnessTexture != null) {
                blitMaterial = GetGlossBlitMaterial();
                Graphics.Blit(smoothnessTexture, destRenderTexture, blitMaterial);
            }
            
            var exportTexture = new Texture2D(
                width,
                height,
                TextureFormat.RGB24,
                false,
                true);
            exportTexture.ReadPixels(new Rect(0, 0, destRenderTexture.width, destRenderTexture.height), 0, 0);
            exportTexture.Apply();
            
            var imageData = format == Format.Png 
                ? exportTexture.EncodeToPNG()
                : exportTexture.EncodeToJPG(60);

            return imageData;
        }
        
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            var hash = 14;
            if (m_Texture != null) {
                hash = hash * 7 + m_Texture.GetHashCode();
            }
            if (m_OccTexture != null) {
                hash = hash * 7 + m_OccTexture.GetHashCode();
            }
            if (m_SmoothnessTexture != null) {
                hash = hash * 7 + m_SmoothnessTexture.GetHashCode();
            }
            return hash;
        }

        public override bool Equals(object obj) {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            return Equals((OrmImageExport)obj);
        }
        
        bool Equals(OrmImageExport other) {
            return m_Texture == other.m_Texture 
                && m_OccTexture == other.m_OccTexture 
                && m_SmoothnessTexture == other.m_SmoothnessTexture;
        }
    }
}