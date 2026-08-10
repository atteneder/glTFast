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

        /// <inheritdoc />
        internal override void UnsetExtensions()
        {
            extensions = null;
        }
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
        // TODO: Remove upon next major release. This serves no purpose anymore except keeping the API intact.
        public override bool Equals(object obj)
        {
            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            return base.Equals(obj);
        }

        /// <summary>
        /// Default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        // TODO: Remove upon next major release. This serves no purpose anymore except keeping the API intact.
        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }

        /// <summary>
        /// Sets <see cref="Extensions"/> to null.
        /// </summary>
        internal abstract void UnsetExtensions();

        /// <summary>
        /// Cleans up invalid parsing artifacts created by <see cref="GltfJsonUtilityParser"/>.
        /// </summary>
        internal void JsonUtilityCleanup()
        {
            var e = Extensions;
            if (e != null)
            {
                // Check if Basis Universal extension is valid
                if ((e.KHR_texture_basisu?.source ?? -1) < 0)
                {
                    e.KHR_texture_basisu = null;
                }

                // Check if WebP extension is valid
                if ((e.EXT_texture_webp?.source ?? -1) < 0)
                {
                    e.EXT_texture_webp = null;
                }

                // Unset extensions container if all extensions are null
                if (e.KHR_texture_basisu == null && e.EXT_texture_webp == null)
                {
                    UnsetExtensions();
                }
            }
        }
    }
}
