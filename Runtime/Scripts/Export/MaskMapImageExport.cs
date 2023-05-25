// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if USING_HDRP

using System;
using System.IO;
using UnityEngine;

namespace GLTFast.Export
{

    /// <summary>
    /// Exports a glTF ORM (occlusion/roughness/metallic) image map
    /// </summary>
    class MaskMapImageExport : ImageExport
    {

        static Material s_BlitMaterial;

        /// <inheritdoc />
        public MaskMapImageExport(
            Texture2D maskMap = null,
            ImageFormat imageFormat = ImageFormat.Unknown)
            : base(maskMap, imageFormat) { }

        /// <inheritdoc />
        protected override ImageFormat ImageFormat => m_ImageFormat != ImageFormat.Unknown ? m_ImageFormat : ImageFormat.Jpg;

        static Material GetMaskMapBlitMaterial()
        {
            if (s_BlitMaterial == null)
            {
                s_BlitMaterial = LoadBlitMaterial("glTFExportMaskMap");
            }
            return s_BlitMaterial;
        }

        /// <inheritdoc />
        protected override bool GenerateTexture(out byte[] imageData)
        {
            if (m_Texture != null)
            {
                imageData = EncodeTexture(m_Texture, ImageFormat, false, GetMaskMapBlitMaterial());
                return true;
            }
            imageData = null;
            return false;
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
    }
}
#endif // USING_HDRP
