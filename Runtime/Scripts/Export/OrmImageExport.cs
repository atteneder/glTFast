// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace Unity.Cloud.Gltfast.Export
{

    /// <summary>
    /// Exports a glTF ORM (occlusion/roughness/metallic) image map
    /// </summary>
    class OrmImageExport : ImageExport
    {
        static readonly int smoothnessFactorProperty = Shader.PropertyToID("_SmoothnessFactor");

        static Material s_MetalGlossBlitMaterial;
        static Material s_OcclusionBlitMaterial;
        static Material s_GlossBlitMaterial;

        readonly Texture2D m_OccTexture;
        readonly Texture2D m_SmoothnessTexture;

        readonly float m_SmoothnessFactor;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="metalGlossTexture">Metal/Gloss texture, as used by Unity Lit/Standard materials</param>
        /// <param name="occlusionTexture">Occlusion texture, as used by Unity Lit/Standard materials</param>
        /// <param name="smoothnessTexture">Smoothness texture, as used by Unity Lit/Standard materials</param>
        /// <param name="smoothnessFactor">Smoothness factor</param>
        /// <param name="imageFormat">Export image format</param>
        public OrmImageExport(
            Texture2D metalGlossTexture = null,
            Texture2D occlusionTexture = null,
            Texture2D smoothnessTexture = null,
            float smoothnessFactor = 1.0f,
            ImageFormat imageFormat = ImageFormat.Unknown)
            : base(metalGlossTexture, imageFormat)
        {
            m_OccTexture = occlusionTexture;
            m_SmoothnessTexture = smoothnessTexture;
            m_SmoothnessFactor = smoothnessFactor;
        }

        /// <inheritdoc />
        public override string FileName
        {
            get
            {
                if (m_Texture != null) return base.FileName;
                if (m_OccTexture != null && !string.IsNullOrEmpty(m_OccTexture.name))
                {
                    return $"{m_OccTexture.name}.{FileExtension}";
                }
                if (m_SmoothnessTexture != null && !string.IsNullOrEmpty(m_SmoothnessTexture.name))
                {
                    return $"{m_SmoothnessTexture.name}ORM.{FileExtension}";
                }
                return $"ORM.{FileExtension}";
            }
        }

        /// <inheritdoc />
        protected override ImageFormat ImageFormat => m_ImageFormat != ImageFormat.Unknown ? m_ImageFormat : ImageFormat.Jpeg;

        /// <inheritdoc />
        public override FilterMode FilterMode
        {
            get
            {
                if (m_Texture != null)
                {
                    return m_Texture.filterMode;
                }
                if (m_OccTexture != null)
                {
                    return m_OccTexture.filterMode;
                }
                if (m_SmoothnessTexture != null)
                {
                    return m_SmoothnessTexture.filterMode;
                }
                return FilterMode.Bilinear;
            }
        }

        /// <inheritdoc />
        public override TextureWrapMode WrapModeU
        {
            get
            {
                if (m_Texture != null)
                {
                    return m_Texture.wrapModeU;
                }
                if (m_OccTexture != null)
                {
                    return m_OccTexture.wrapModeU;
                }
                if (m_SmoothnessTexture != null)
                {
                    return m_SmoothnessTexture.wrapModeU;
                }
                return TextureWrapMode.Repeat;
            }
        }

        /// <inheritdoc />
        public override TextureWrapMode WrapModeV
        {
            get
            {
                if (m_Texture != null)
                {
                    return m_Texture.wrapModeV;
                }
                if (m_OccTexture != null)
                {
                    return m_OccTexture.wrapModeV;
                }
                if (m_SmoothnessTexture != null)
                {
                    return m_SmoothnessTexture.wrapModeV;
                }
                return TextureWrapMode.Repeat;
            }
        }

        /// <summary>
        /// True if occlusion texture was set
        /// </summary>
        public bool HasOcclusion => m_OccTexture != null;

        static Material GetMetalGlossBlitMaterial()
        {
            if (s_MetalGlossBlitMaterial == null)
            {
                s_MetalGlossBlitMaterial = LoadBlitMaterial("glTFExportMetalGloss");
            }
            return s_MetalGlossBlitMaterial;
        }

        static Material GetOcclusionBlitMaterial()
        {
            if (s_OcclusionBlitMaterial == null)
            {
                s_OcclusionBlitMaterial = LoadBlitMaterial("glTFExportOcclusion");
            }
            return s_OcclusionBlitMaterial;
        }


        static Material GetGlossBlitMaterial()
        {
            if (s_GlossBlitMaterial == null)
            {
                s_GlossBlitMaterial = LoadBlitMaterial("glTFExportSmoothness");
            }
            return s_GlossBlitMaterial;
        }

        /// <inheritdoc />
        public override bool Write(string filePath, bool overwrite)
        {
            if (GenerateTexture(out var imageData))
            {
                File.WriteAllBytes(filePath, imageData);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        protected override bool GenerateTexture(out byte[] imageData)
        {
            if (m_Texture != null || m_OccTexture != null || m_SmoothnessTexture != null)
            {
                imageData = EncodeOrmTexture(
                    m_Texture, m_OccTexture, m_SmoothnessTexture, m_SmoothnessFactor, ImageFormat, JpgQuality);
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
        /// <param name="smoothnessFactor">Smoothenss factor</param>
        /// <param name="format">Export image format</param>
        /// <param name="jpgQuality">[1-100] quality for JPG images</param>
        /// <returns></returns>
        static byte[] EncodeOrmTexture(
            Texture2D metalGlossTexture,
            Texture2D occlusionTexture,
            Texture2D smoothnessTexture,
            float smoothnessFactor,
            ImageFormat format,
            int jpgQuality
        )
        {
#if UNITY_IMAGECONVERSION
            Assert.IsTrue(metalGlossTexture != null || occlusionTexture != null || smoothnessTexture != null);
            var blitMaterial = GetMetalGlossBlitMaterial();

            var width = int.MinValue;
            var height = int.MinValue;

            if (metalGlossTexture != null)
            {
                width = math.max(width, metalGlossTexture.width);
                height = math.max(height, metalGlossTexture.height);
            }

            if (occlusionTexture != null)
            {
                width = math.max(width, occlusionTexture.width);
                height = math.max(height, occlusionTexture.height);
            }

            if (smoothnessTexture != null)
            {
                width = math.max(width, smoothnessTexture.width);
                height = math.max(height, smoothnessTexture.height);
            }

            var destRenderTexture = RenderTexture.GetTemporary(
                width,
                height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear,
                1,
                RenderTextureMemoryless.Depth
            );

            if (metalGlossTexture == null)
            {
                var rt = RenderTexture.active;
                RenderTexture.active = destRenderTexture;
                GL.Clear(true, true, Color.white);
                RenderTexture.active = rt;
            }
            else
            {
                blitMaterial.SetFloat(smoothnessFactorProperty, smoothnessFactor);
                Graphics.Blit(metalGlossTexture, destRenderTexture, blitMaterial);
            }
            if (occlusionTexture != null)
            {
                blitMaterial = GetOcclusionBlitMaterial();
                Graphics.Blit(occlusionTexture, destRenderTexture, blitMaterial);
            }
            if (smoothnessTexture != null)
            {
                blitMaterial = GetGlossBlitMaterial();
                blitMaterial.SetFloat(smoothnessFactorProperty, smoothnessFactor);
                Graphics.Blit(smoothnessTexture, destRenderTexture, blitMaterial);
            }

            var exportTexture = new Texture2D(
                width,
                height,
                SystemInfo.IsFormatSupported(GraphicsFormat.R8G8B8_UNorm, GraphicsFormatUsage.Sample)
                    ? GraphicsFormat.R8G8B8_UNorm
                    : GraphicsFormat.R8G8B8A8_UNorm,
                TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate
            );
            exportTexture.ReadPixels(new Rect(0, 0, destRenderTexture.width, destRenderTexture.height), 0, 0);
            RenderTexture.ReleaseTemporary(destRenderTexture);
            exportTexture.Apply();

            var imageData = format == ImageFormat.Png
                ? exportTexture.EncodeToPNG()
                : exportTexture.EncodeToJPG(jpgQuality);

            // Release temporary texture
#if UNITY_EDITOR
            Object.DestroyImmediate(exportTexture);
#else
            Object.Destroy(exportTexture);
#endif

            return imageData;
#else
            return null;
#endif
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 14;
            if (m_Texture != null)
            {
                hash = hash * 7 + m_Texture.GetHashCode();
            }
            if (m_OccTexture != null)
            {
                hash = hash * 7 + m_OccTexture.GetHashCode();
            }
            if (m_SmoothnessTexture != null)
            {
                hash = hash * 7 + m_SmoothnessTexture.GetHashCode();
            }
            return hash;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((OrmImageExport)obj);
        }

        bool Equals(OrmImageExport other)
        {
            return m_Texture == other.m_Texture
                && m_OccTexture == other.m_OccTexture
                && m_SmoothnessTexture == other.m_SmoothnessTexture;
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticsOnLoad()
        {
            // Reset static state
            s_MetalGlossBlitMaterial = null;
            s_OcclusionBlitMaterial = null;
            s_GlossBlitMaterial = null;
        }
#endif
    }
}
