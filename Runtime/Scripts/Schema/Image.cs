// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// Image data used to create a texture.
    /// </summary>
    [System.Serializable]
    public class Image : NamedObject
    {
        /// <summary>
        /// The uri of the image.  Relative paths are relative to the .gltf file.
        /// Instead of referencing an external file, the uri can also be a data-uri.
        /// The image format must be jpg, png, bmp, or gif.
        /// </summary>
        public string uri;

        /// <summary>
        /// The image's MIME type.
        /// </summary>
        public string mimeType;

        /// <summary>
        /// The index of the bufferView that contains the image.
        /// Use this instead of the image's uri property.
        /// </summary>
        public int bufferView = -1;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeName(writer);
            if (!string.IsNullOrEmpty(uri))
            {
                writer.AddPropertySafe("uri", uri);
            }
            if (!string.IsNullOrEmpty(mimeType))
            {
                writer.AddProperty("mimeType", mimeType);
            }
            if (bufferView >= 0)
            {
                writer.AddProperty("bufferView", bufferView);
            }
            writer.Close();
        }
    }
}
