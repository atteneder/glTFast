// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Mathematics;

namespace GLTFast.Schema
{
    /// <inheritdoc />
    [System.Serializable]
    public class OcclusionTextureInfo : OcclusionTextureInfoBase<TextureInfoExtensions> { }

    /// <inheritdoc />
    /// <typeparam name="TExtensions">occlusionTextureInfo extensions type</typeparam>
    [System.Serializable]
    public abstract class OcclusionTextureInfoBase<TExtensions> : OcclusionTextureInfoBase
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
    /// Occlusion map specific texture info
    /// </summary>
    [System.Serializable]
    public abstract class OcclusionTextureInfoBase : TextureInfoBase
    {
        /// <summary>
        /// A scalar multiplier controlling the amount of occlusion applied.
        /// A value of 0.0 means no occlusion.
        /// A value of 1.0 means full occlusion.
        /// This value is ignored if the corresponding texture is not specified.
        /// This value is linear.
        /// </summary>
        public float strength = 1.0f;

        internal override void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeTextureInfo(writer);
            if (math.abs(strength - 1f) > Constants.epsilon)
            {
                writer.AddProperty("strength", strength);
            }
            writer.Close();
        }
    }
}
