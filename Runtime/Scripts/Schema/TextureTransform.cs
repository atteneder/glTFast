// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <inheritdoc cref="Extension.TextureTransform"/>
    [System.Serializable]
    public class TextureTransform
    {

        /// <summary>
        /// The offset of the UV coordinate origin as a factor of the texture dimensions.
        /// </summary>
        public float[] offset = { 0, 0 };

        /// <summary>
        /// Rotate the UVs by this many radians counter-clockwise around the origin. This is equivalent to a similar rotation of the image clockwise.
        /// </summary>
        public float rotation;

        /// <summary>
        /// The scale factor applied to the components of the UV coordinates.
        /// </summary>
        public float[] scale = { 1, 1 };

        /// <summary>
        /// Overrides the textureInfo texCoord value if supplied, and if this extension is supported.
        /// </summary>
        public int texCoord = -1;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (offset != null)
            {
                writer.AddArrayProperty("offset", offset);
            }
            if (scale != null)
            {
                writer.AddArrayProperty("scale", scale);
            }
            if (rotation != 0)
            {
                writer.AddProperty("rotation", rotation);
            }
            if (texCoord >= 0)
            {
                writer.AddProperty("texCoord", texCoord);
            }
            writer.Close();
        }
    }
}
