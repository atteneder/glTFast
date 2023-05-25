// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// Reference to a texture.
    /// </summary>
    [System.Serializable]
    public class TextureInfo
    {

        /// <summary>
        /// The index of the texture.
        /// </summary>
        public int index = -1;

        /// <summary>
        /// This integer value is used to construct a string in the format
        /// TEXCOORD_&lt;set index&gt; which is a reference to a key in
        /// mesh.primitives.attributes (e.g. A value of 0 corresponds to TEXCOORD_0).
        /// </summary>
        public int texCoord;

        /// <inheritdoc cref="TextureInfoExtension"/>
        public TextureInfoExtension extensions;

        internal void GltfSerializeTextureInfo(JsonWriter writer)
        {
            if (index >= 0)
            {
                writer.AddProperty("index", index);
            }
            if (texCoord > 0)
            {
                writer.AddProperty("texCoord", texCoord);
            }

            if (extensions != null)
            {
                writer.AddProperty("extensions");
                extensions.GltfSerialize(writer);
            }
        }

        internal virtual void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeTextureInfo(writer);
            writer.Close();
        }
    }
}
