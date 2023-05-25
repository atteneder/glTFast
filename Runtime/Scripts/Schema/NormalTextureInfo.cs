// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Mathematics;

namespace GLTFast.Schema
{

    /// <summary>
    /// Normal map specific texture info
    /// </summary>
    [System.Serializable]
    public class NormalTextureInfo : TextureInfo
    {

        /// <summary>
        /// The scalar multiplier applied to each normal vector of the texture.
        /// This value is ignored if normalTexture is not specified.
        /// This value is linear.
        /// </summary>
        public float scale = 1.0f;

        internal override void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeTextureInfo(writer);
            if (math.abs(scale - 1f) > Constants.epsilon)
            {
                writer.AddProperty("scale", scale);
            }
            writer.Close();
        }
    }
}
