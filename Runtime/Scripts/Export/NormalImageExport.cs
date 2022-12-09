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
