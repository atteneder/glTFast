// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast.Export
{

    /// <summary>
    /// Wrapper to export a glTF image normal map
    /// </summary>
    class NormalImageExport : ImageExport
    {

        static Material s_NormalBlitMaterial;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="texture">Main source texture</param>
        public NormalImageExport(Texture2D texture)
            : base(texture) { }

        protected override ImageFormat ImageFormat
        {
            get
            {
                if (m_ImageFormat != ImageFormat.Unknown) return m_ImageFormat;
                return ImageFormat.Png;
            }
        }

        static Material GetNormalBlitMaterial()
        {
            if (s_NormalBlitMaterial == null)
            {
                s_NormalBlitMaterial = LoadBlitMaterial("glTFExportNormal");
            }
            return s_NormalBlitMaterial;
        }

        /// <inheritdoc />
        protected override bool GenerateTexture(out byte[] imageData)
        {
            if (m_Texture != null)
            {
                imageData = EncodeTexture(m_Texture, ImageFormat, hasAlpha: false, blitMaterial: GetNormalBlitMaterial());
                return true;
            }
            imageData = null;
            return false;
        }
    }
}
