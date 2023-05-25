// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

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
