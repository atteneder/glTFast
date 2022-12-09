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

using UnityEngine;

namespace GLTFast.Schema
{

    /// <summary>
    /// This extension defines a sheen that can be layered on top of an
    /// existing glTF material definition. A sheen layer is a common technique
    /// used in Physically-Based Rendering to represent cloth and fabric
    /// materials, for example.
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_sheen"/>
    /// </summary>
    [System.Serializable]
    public class Sheen
    {

        /// <summary>
        /// The sheen color red, green and blue components in linear space.
        /// </summary>
        public float[] sheenColorFactor = { 1, 1, 1 };

        /// <summary>
        /// The sheen color in linear space.
        /// </summary>
        public Color SheenColor =>
            new Color(
                sheenColorFactor[0],
                sheenColorFactor[1],
                sheenColorFactor[2]
            );

        /// <summary>
        /// The sheen color texture.
        /// </summary>
        public TextureInfo sheenColorTexture;

        /// <summary>
        /// The sheen roughness.
        /// </summary>
        public float sheenRoughnessFactor;

        /// <summary>
        /// The sheen roughness (Alpha) texture.
        /// </summary>
        public TextureInfo sheenRoughnessTexture;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.Close();
            throw new System.NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }
}
