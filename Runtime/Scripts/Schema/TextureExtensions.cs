// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// Texture extensions
    /// </summary>
    [System.Serializable]
    public class TextureExtensions
    {

        /// <inheritdoc cref="Extension.TextureBasisUniversal"/>
        // ReSharper disable once InconsistentNaming
        public TextureBasisUniversal KHR_texture_basisu;

        internal void GltfSerialize(JsonWriter writer)
        {
            throw new System.NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }

    /// <summary>
    /// Basis Universal texture extension
    /// </summary>
    /// <seealso cref="Extension.TextureBasisUniversal"/>
    [System.Serializable]
    public class TextureBasisUniversal
    {

        /// <summary>
        /// Index of the image which defines a reference to the KTX v2 image
        /// with Basis Universal super-compression.
        /// </summary>
        public int source = -1;
    }
}
