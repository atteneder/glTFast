// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using GLTFast.Schema;
using Unity.Collections;
using UnityEngine;

namespace GLTFast
{
    /// <summary>
    /// Creates a <see cref="UnityEngine.Texture2D"/> from raw, usually compressed image data.
    /// </summary>
    public interface ITextureImageLoader
    {
        /// <summary>
        /// Creates an image from the input data.
        /// </summary>
        /// <param name="data">Raw, compressed image data.</param>
        /// <param name="linear">If true, the texture being created is in linear space. If false, it is in sRGB space.</param>
        /// <param name="readable">If true, the resulting texture should remain readable (<see cref="UnityEngine.Texture2D.isReadable"/>).</param>
        /// <param name="generateMipMaps">If true, mipmap levels should get generated.</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <returns>An image texture result</returns>
        Task<ImageResult> LoadImage(
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken
        );

        /// <summary>
        /// Channel-aware variant of <see cref="LoadImage(NativeArray{byte}.ReadOnly,bool,bool,bool,CancellationToken)"/>.
        /// <paramref name="channelMask"/> is a bitmask of the channels the referencing materials actually consume, so a
        /// loader can pick the most efficient format. The default implementation ignores the mask and forwards to the
        /// channel-unaware overload, so existing loaders keep working unchanged.
        /// </summary>
        /// <param name="data">Raw, compressed image data.</param>
        /// <param name="linear">If true, the texture being created is in linear space. If false, it is in sRGB space.</param>
        /// <param name="readable">If true, the resulting texture should remain readable (<see cref="UnityEngine.Texture2D.isReadable"/>).</param>
        /// <param name="generateMipMaps">If true, mipmap levels should get generated.</param>
        /// <param name="cancellationToken">Token to submit cancellation requests. The default value is None.</param>
        /// <param name="channelMask">Bitmask of channels consumed by referencing materials (bit0=R..bit3=A).</param>
        /// <returns>An image texture result</returns>
        Task<ImageResult> LoadImage(
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken,
            int channelMask
        ) => LoadImage(data, linear, readable, generateMipMaps, cancellationToken);

        /// <summary>
        /// Determines if this loader can load the given texture, and if so, returns the corresponding image index.
        /// The typical use-case is a glTF texture extension that adds support for a new image format,
        /// e.g. EXT_texture_webp.
        /// The loader may also update the image index to point to a different image, e.g. if the extension adds support
        /// for a new image format that is stored in a different image than the one referenced by the texture.
        /// </summary>
        /// <param name="texture">glTF texture.</param>
        /// <param name="imageIndex">Corresponding, potentially updated glTF image index.</param>
        /// <returns>True if the texture image loader supports loading that texture, false otherwise.</returns>
        bool IsAbleToLoad(TextureBase texture, out int imageIndex);

        /// <summary>
        /// Implement this to support content-based image format detection.
        /// This is optional, but fixes loading of glTF images
        /// where the type could not be derived from the mime type or file extension.
        /// </summary>
        /// <param name="data">Image data.</param>
        /// <returns>True if the image loader supports loading that data, false otherwise.</returns>
        bool IsAbleToLoad(ReadOnlySpan<byte> data) => false;
    }
}
