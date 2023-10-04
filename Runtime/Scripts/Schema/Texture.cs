// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast.Schema
{
    /// <inheritdoc />
    [Serializable]
    public class Texture : TextureBase<TextureExtensions> { }

    /// <inheritdoc />
    /// <typeparam name="TExtensions">Texture extensions type</typeparam>
    [Serializable]
    public abstract class TextureBase<TExtensions> : TextureBase
    where TExtensions : TextureExtensions
    {
        /// <inheritdoc cref="Extensions"/>
        public TExtensions extensions;

        /// <inheritdoc />
        public override TextureExtensions Extensions => extensions;
    }

    /// <summary>
    /// A texture is defined by an image and a sampler.
    /// </summary>
    [Serializable]
    public abstract class TextureBase : NamedObject
    {

        /// <summary>
        /// The index of the sampler used by this texture.
        /// </summary>
        public int sampler = -1;

        /// <summary>
        /// The index of the image used by this texture.
        /// </summary>
        public int source = -1;

        /// <inheritdoc cref="TextureExtensions"/>
        public abstract TextureExtensions Extensions { get; }

        /// <summary>
        /// Retrieves the final image index.
        /// </summary>
        /// <returns>Final image index</returns>
        public int GetImageIndex()
        {
            if (Extensions != null)
            {
                if (Extensions.KHR_texture_basisu != null && Extensions.KHR_texture_basisu.source >= 0)
                {
                    return Extensions.KHR_texture_basisu.source;
                }
            }
            return source;
        }

        /// <summary>
        /// True, if the texture is of the KTX format.
        /// </summary>
        public bool IsKtx => Extensions?.KHR_texture_basisu != null;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeName(writer);
            if (source >= 0)
            {
                writer.AddProperty("source", source);
            }
            if (sampler >= 0)
            {
                writer.AddProperty("sampler", sampler);
            }
            if (Extensions != null)
            {
                writer.AddProperty("extensions");
                Extensions.GltfSerialize(writer);
            }
            writer.Close();
        }

        /// <summary>
        /// Determines whether two object instances are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return Equals((Texture)obj);
        }

        bool Equals(Texture other)
        {
            return source == other.source
                && sampler == other.sampler
                && (
                    Extensions == null && other.Extensions == null
                    || (Extensions != null && Extensions.Equals(other.Extensions))
                );
        }

        /// <summary>
        /// Default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
#if NET_STANDARD
            return HashCode.Combine(source, sampler, Extensions.GetHashCode());
#else
            var hash = 17;
            hash = hash * 31 + source;
            hash = hash * 31 + sampler;
            hash = hash * 31 + Extensions.GetHashCode();
            return hash;
#endif
        }
    }
}
