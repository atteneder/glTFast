// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Mathematics;
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
        public Color SheenColor
        {
            get =>
                new Color(
                    sheenColorFactor[0],
                    sheenColorFactor[1],
                    sheenColorFactor[2]
                );
            set
            {
                sheenColorFactor = new[] { value.r, value.g, value.b };
            }
        }

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
            if (sheenColorFactor != null && sheenColorFactor.Length > 2 && (
                    math.abs(sheenColorFactor[0] - 1f) > Constants.epsilon ||
                    math.abs(sheenColorFactor[1] - 1f) > Constants.epsilon ||
                    math.abs(sheenColorFactor[2] - 1f) > Constants.epsilon
                ))
            {
                writer.AddArrayProperty("sheenColorFactor", sheenColorFactor);
            }
            if (sheenColorTexture != null)
            {
                writer.AddProperty("sheenColorTexture");
                sheenColorTexture.GltfSerialize(writer);
            }
            if (sheenRoughnessFactor > 0)
            {
                writer.AddProperty("sheenRoughnessFactor", sheenRoughnessFactor);
            }
            if (sheenRoughnessTexture != null)
            {
                writer.AddProperty("sheenRoughnessTexture");
                sheenRoughnessTexture.GltfSerialize(writer);
            }
            writer.Close();
        }
    }
}
