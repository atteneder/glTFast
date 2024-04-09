// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// A buffer points to binary geometry, animation, or skins.
    /// </summary>
    [System.Serializable]
    public class Buffer : NamedObject
    {

        /// <summary>
        /// The length of the buffer in bytes.
        /// </summary>
        public uint byteLength;

        /// <summary>
        /// The URI (or IRI) of the buffer.
        /// </summary>
        public string uri;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            if (!string.IsNullOrEmpty(uri))
            {
                writer.AddPropertySafe("uri", uri);
            }
            writer.AddProperty("byteLength", byteLength);
            writer.Close();
        }
    }
}
