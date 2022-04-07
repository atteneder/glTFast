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

namespace GLTFast.Export {
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

        protected override bool GenerateTexture(out byte[] imageData) {
            //UnityEngine.Debug.Log("Exporting Normal Image");
            if (m_Texture != null) {
                imageData = EncodeTexture(m_Texture, format, hasAlpha:false, blitMaterial:GetNormalBlitMaterial());
                return true;
            }
            imageData = null;
            return false;
        }
    }
}
