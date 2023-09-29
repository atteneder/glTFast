// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace GLTFast.Schema
{

    /// <summary>
    /// This extension defines a sheen that can be layered on top of an
    /// existing glTF material definition. A sheen layer is a common technique
    /// used in Physically-Based Rendering to represent cloth and fabric
    /// materials, for example.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_sheen"/>
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
