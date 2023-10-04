// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// Extension for optical transparency (transmission)
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Khronos/KHR_materials_transmission"/>
    [System.Serializable]
    public class Transmission
    {

        /// <summary>
        /// The base fraction of light that is transmitted through the surface.
        /// </summary>
        public float transmissionFactor;

        /// <summary>
        /// A texture that defines the transmission fraction of the surface,
        /// stored in the R channel. This will be multiplied by
        /// transmissionFactor.
        /// </summary>
        public TextureInfo transmissionTexture;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.Close();
            throw new System.NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }
}
