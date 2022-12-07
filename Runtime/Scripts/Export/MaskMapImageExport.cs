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

namespace GLTFast.Export
{

    /// <summary>
    /// Exports a glTF ORM (occlusion/roughness/metallic) image map
    /// </summary>
    public class MaskMapImageExport : ImageExport
    {

        static Material s_BlitMaterial;

        /// <inheritdoc />
        public MaskMapImageExport(
            Texture2D maskMap = null,
            Format format = Format.Unknown)
            : base(maskMap, format) { }

        /// <inheritdoc />
        protected override Format format => m_Format != Format.Unknown ? m_Format : Format.Jpg;

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
                imageData = EncodeTexture(m_Texture, format, false, GetMaskMapBlitMaterial());
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
