// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine.Serialization;

namespace GLTFast.Schema
{

    /// <inheritdoc />
    [System.Serializable]
    public class TextureInfo : TextureInfoBase<TextureInfoExtensions> { }

    /// <inheritdoc />
    /// <typeparam name="TExtensions">textureInfo extensions type</typeparam>
    [System.Serializable]
    public abstract class TextureInfoBase<TExtensions> : TextureInfoBase
        where TExtensions : TextureInfoExtensions, new()
    {
        /// <inheritdoc cref="Extensions"/>
        public TExtensions extensions;

        /// <inheritdoc />
        public override TextureInfoExtensions Extensions => extensions;

        internal override void SetTextureTransform(TextureTransform textureTransform)
        {
            extensions = extensions ?? new TExtensions();
            extensions.KHR_texture_transform = textureTransform;
        }
    }

    /// <summary>
    /// Reference to a texture.
    /// </summary>
    [System.Serializable]
    public abstract class TextureInfoBase
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

        /// <inheritdoc cref="TextureInfoExtensions"/>
        public abstract TextureInfoExtensions Extensions { get; }

        /// <summary>
        /// Applies a texture transform by initializing <see cref="Extensions" /> (if required) and setting its
        /// <see cref="TextureInfoExtensions.KHR_texture_transform" /> field.
        /// </summary>
        /// <param name="textureTransform">Texture transform to apply.</param>
        internal abstract void SetTextureTransform(TextureTransform textureTransform);

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

            if (Extensions != null)
            {
                writer.AddProperty("extensions");
                Extensions.GltfSerialize(writer);
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
