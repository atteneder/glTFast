// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// This extension defines a clear coating that can be layered on top of an
    /// existing glTF material definition.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_materials_clearcoat/README.md"/>
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
        public NormalTextureInfo clearcoatNormalTexture;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();

            if (clearcoatFactor > 0)
            {
                writer.AddProperty("clearcoatFactor", clearcoatFactor);
            }
            if (clearcoatTexture != null)
            {
                writer.AddProperty("clearcoatTexture");
                clearcoatTexture.GltfSerialize(writer);
            }
            if (clearcoatRoughnessFactor > 0)
            {
                writer.AddProperty("clearcoatRoughnessFactor", clearcoatRoughnessFactor);
            }
            if (clearcoatRoughnessTexture != null)
            {
                writer.AddProperty("clearcoatRoughnessTexture");
                clearcoatRoughnessTexture.GltfSerialize(writer);
            }
            if (clearcoatNormalTexture != null)
            {
                writer.AddProperty("clearcoatNormalTexture");
                clearcoatNormalTexture.GltfSerialize(writer);
            }

            writer.Close();
        }

    }
}
