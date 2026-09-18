// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Objects;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Texture image loader that is able to load glTF core specification image formats (PNG and/or JPEG).
    /// Can be used to override the default loading (performed by the ImageConversion module).
    /// To add support for texture extensions that add support for an image format that's not in the glTF specification
    /// (i.e. that override the image source) use <see cref="ITextureImageLoader"/> instead.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public interface IDefaultImageFormatLoader : ITextureImageLoader
    {
        /// <summary>
        /// Determines if this loader can load the image format
        /// detected from glTF JSON clues (mimeType and URI file extension).
        /// </summary>
        /// <param name="format">Source image format.</param>
        /// <returns>True if the implementation is able to load the given format, false otherwise.</returns>
        bool IsAbleToLoad(ImageFormat format);

        /// <summary>
        /// Determines if this loader can load the given texture, and if so, return the corresponding image index.
        /// In context of <see cref="IDefaultImageFormatLoader"/> this *SHOULD* always return false,
        /// otherwise it might block other <see cref="ITextureImageLoader"/> implementations from loading textures
        /// unknown to this one. Use <see cref="ITextureImageLoader"/> directly if you want to implement support for
        /// a glTF texture extension.
        /// </summary>
        /// <param name="texture">glTF texture.</param>
        /// <param name="imageIndex">Corresponding, potentially updated glTF image index.</param>
        /// <returns>True if the texture image loader supports loading that texture, false otherwise.</returns>
        /// <seealso cref="ITextureImageLoader"/>
        bool ITextureImageLoader.IsAbleToLoad(Texture texture, out int imageIndex)
        {
            imageIndex = -1;
            return false;
        }
    }
}
