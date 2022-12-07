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

namespace GLTFast.Schema
{

    /// <summary>
    /// This extension defines a clear coating that can be layered on top of an
    /// existing glTF material definition.
    /// <seealso href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md"/>
    /// </summary>
    [System.Serializable]
    public class ClearCoat
    {

        /// <summary>
        /// The clearcoat layer intensity.
        /// </summary>
        public float clearcoatFactor;

        /// <summary>
        /// The clearcoat layer intensity texture.
        /// </summary>
        public TextureInfo clearcoatTexture;

        /// <summary>
        /// The clearcoat layer roughness.
        /// </summary>
        public float clearcoatRoughnessFactor;

        /// <summary>
        /// The clearcoat layer roughness texture.
        /// </summary>
        public TextureInfo clearcoatRoughnessTexture;

        /// <summary>
        /// The clearcoat normal map texture.
        /// </summary>
        public TextureInfo clearcoatNormalTexture;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.Close();
            throw new System.NotImplementedException($"GltfSerialize missing on {GetType()}");
        }

    }
}
