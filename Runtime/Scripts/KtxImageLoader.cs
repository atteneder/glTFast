// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if KTX_IS_RECENT
#define KTX_IS_ENABLED
#endif

#if KTX_IS_ENABLED

using System;
using System.Threading;
using System.Threading.Tasks;
using KtxUnity;
using Unity.Cloud.Gltfast.Logging;
using Unity.Collections;
using UnityEngine;

namespace Unity.Cloud.Gltfast
{
    static class KtxImageLoader
    {

        public static async Task<ImageResult> LoadAsync(
            ImportContext context,
            ImportSettings settings,
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            CancellationToken cancellationToken
            )
        {
            var ktx = new KtxTexture();
            var errorCode = ktx.Open(data);
            if (errorCode != ErrorCode.Success)
            {
                context.Logger?.Error(LogCode.EmbedImageLoadFailed);
                ktx.Dispose();
                return default;
            }

            // TODO implement cancellation in KTX package
            var result = await ktx.LoadTexture2D(linear, readable);

            if (result.errorCode == ErrorCode.Success)
            {
                if (settings.GenerateMipMaps && result.texture.mipmapCount <= 1)
                {
                    Debug.LogWarning("KTX texture does not contain mipmaps.");
                }
            }
            ktx.Dispose();
            return new ImageResult(result.texture, result.orientation.IsYFlipped());
        }
    }
}
#endif // KTX_IS_ENABLED
