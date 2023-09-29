// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// TextureInfo extensions
    /// </summary>
    [System.Serializable]
    public class TextureInfoExtensions
    {

        /// <inheritdoc cref="Extension.TextureTransform"/>
        // ReSharper disable once InconsistentNaming
        public TextureTransform KHR_texture_transform;

        internal void GltfSerialize(JsonWriter writer)
        {
            if (KHR_texture_transform != null)
            {
                writer.AddObject();
                writer.AddProperty("KHR_texture_transform");
                KHR_texture_transform.GltfSerialize(writer);
                writer.Close();
            }
        }
    }
}
