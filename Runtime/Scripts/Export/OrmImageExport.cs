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
using UnityEngine.Assertions;

namespace GLTFast.Export {
    
    /// <summary>
    /// Exports a glTF ORM (occlusion/roughness/metallic) image map
    /// </summary>
    public class OrmImageExport : ImageExport {
        
        static Material s_MetalGlossBlitMaterial;
        static Material s_OcclusionBlitMaterial;
        static Material s_GlossBlitMaterial;

        Texture2D m_OccTexture;
        Texture2D m_SmoothnessTexture;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="metalGlossTexture">Metal/Gloss texture, as used by Unity Lit/Standard materials</param>
        /// <param name="occlusionTexture">Occlusion texture, as used by Unity Lit/Standard materials</param>
        /// <param name="smoothnessTexture">Smoothness texture, as used by Unity Lit/Standard materials</param>
        /// <param name="format">Export image format</param>
        public OrmImageExport(
            Texture2D metalGlossTexture = null,
            Texture2D occlusionTexture = null,
            Texture2D smoothnessTexture = null,
            Format format = Format.Unknown)
            : base(metalGlossTexture,format) 
        {
            m_OccTexture = occlusionTexture;
            m_SmoothnessTexture = smoothnessTexture;
        }
        
        /// <inheritdoc />
        public override string fileName {
            get {
                if (m_Texture != null) return base.fileName;
                if (m_OccTexture != null) return $"{m_OccTexture.name}.{fileExtension}";
                return $"{m_SmoothnessTexture.name}ORM.{fileExtension}";
            }
        }
        
        /// <inheritdoc />
        protected override Format format => m_Format != Format.Unknown ? m_Format : Format.Jpg;

        /// <inheritdoc />
        public override FilterMode filterMode {
            get {
                if (m_Texture != null) {
                    return m_Texture.filterMode;
                }
                if (m_OccTexture != null) {
                    return m_OccTexture.filterMode;
                }
                if (m_SmoothnessTexture != null) {
                    return m_SmoothnessTexture.filterMode;
                }
                return FilterMode.Bilinear;
            }
        }

        /// <inheritdoc />
        public override TextureWrapMode wrapModeU {
            get {
                if (m_Texture != null) {
                    return m_Texture.wrapModeU;
                }
                if (m_OccTexture != null) {
                    return m_OccTexture.wrapModeU;
                }
                if (m_SmoothnessTexture != null) {
                    return m_SmoothnessTexture.wrapModeU;
                }
                return TextureWrapMode.Repeat;
            }
        }

        /// <inheritdoc />
        public override TextureWrapMode wrapModeV {
            get {
                if (m_Texture != null) {
                    return m_Texture.wrapModeV;
                }
                if (m_OccTexture != null) {
                    return m_OccTexture.wrapModeV;
                }
                if (m_SmoothnessTexture != null) {
                    return m_SmoothnessTexture.wrapModeV;
                }
                return TextureWrapMode.Repeat;
            }
        }

        /// <summary>
        /// True if occlusion texture was set
        /// </summary>
        public bool hasOcclusion => m_OccTexture != null;

        /// <summary>
        /// Assigns a Metal/Gloss source texture
        /// </summary>
        /// <param name="texture">Metal/Gloss texture</param>
        public void SetMetalGlossTexture(Texture2D texture) {
            m_Texture = texture;
        }
        
        /// <summary>
        /// Assigns an occlusion source texture
        /// </summary>
        /// <param name="texture">Occlusion texture</param>
        public void SetOcclusionTexture(Texture2D texture) {
            m_OccTexture = texture;
        }
        
        /// <summary>
        /// Assigns a smoothness source texture
        /// </summary>
        /// <param name="texture">Smoothness texture</param>
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

        /// <inheritdoc />
        public override bool Write(string filePath, bool overwrite) {
            if (GenerateTexture(out var imageData)) {
                File.WriteAllBytes(filePath,imageData);
                return true;
            }
            return false;
        }
        
        /// <inheritdoc />
        protected override bool GenerateTexture(out byte[] imageData) {
            if (m_Texture != null || m_OccTexture!=null || m_SmoothnessTexture!=null) {
                imageData = EncodeOrmTexture(m_Texture, m_OccTexture, m_SmoothnessTexture, format);
                return true;
            }
            imageData = null;
            return false;
        }
        
        /// <summary>
        /// Encodes ORM texture
        /// </summary>
        /// <param name="metalGlossTexture">Metal/Gloss texture</param>
        /// <param name="occlusionTexture">Occlusion texture</param>
        /// <param name="smoothnessTexture">Smoothness texture</param>
        /// <param name="format">Export image format</param>
        /// <returns></returns>
        protected static byte[] EncodeOrmTexture(
            Texture2D metalGlossTexture,
            Texture2D occlusionTexture,
            Texture2D smoothnessTexture,
            Format format
        )
        {
#if UNITY_IMAGECONVERSION
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
#else
            return null;
#endif
        }
        
        /// <inheritdoc />
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

        /// <inheritdoc />
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
