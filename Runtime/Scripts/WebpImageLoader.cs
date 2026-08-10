// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if WEBP_IS_INSTALLED

using System;
using System.Threading;
using System.Threading.Tasks;
using GLTFast.Logging;
using Unity.Collections;
using UnityEngine;
using WebP;

namespace GLTFast
{
    /// <summary>
    /// Decodes WebP image data into a <see cref="Texture2D"/> using the unity.webp package.
    /// </summary>
    static class WebpImageLoader
    {
        public static async Task<ImageResult> LoadAsync(
            ImportContext context,
            ImportSettings settings,
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken
            )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bytes = data.ToArray();
            int width = 0;
            int height = 0;
            byte[] rawData = null;
            Error error = Error.Success;

            // Offload heavy decompression to a background thread
            await Task.Run(() =>
            {
                try
                {
                    Texture2DExt.GetWebPDimensions(bytes, out width, out height);
                    // Always decode WITHOUT mipmaps — unity.webp only fills the base level.
                    // We generate mipmaps natively on the GPU later.
                    rawData = Texture2DExt.LoadRGBAFromWebP(bytes, ref width, ref height, false, out error, null);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[glTFast] WebP background decode exception: {e.Message}");
                    error = Error.DecodingError;
                }
            }, cancellationToken);

            if (error != Error.Success || rawData == null)
            {
                context.Logger?.Error(LogCode.EmbedImageLoadFailed);
                return ImageResult.Null;
            }

            // Back on the main thread: create texture and upload pixels instantly
            var texture = Texture2DExt.CreateWebpTexture2D(width, height, false, linear);
            texture.LoadRawTextureData(rawData);
            
            // Generate mipmaps if requested, then optionally mark non-readable
            texture.Apply(generateMipMaps, !readable);

            // unity.webp returns textures in Unity's standard orientation (bottom-up),
            // so isYFlipped is false.
            return new ImageResult(texture, false);
        }
    }
}

#endif // WEBP_IS_INSTALLED
