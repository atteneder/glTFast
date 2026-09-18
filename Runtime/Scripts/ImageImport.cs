// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Gltfast.Addons;
using Unity.Cloud.Gltfast.Loading;
using Unity.Cloud.Gltfast.Logging;
using UnityEngine;

namespace Unity.Cloud.Gltfast
{
    static class ImageImport
    {
        internal static async Task<ImageResult> LoadImageAsync(
            ImportContext context,
            ImportSettings settings,
            int imageIndex,
            bool linear,
            bool readable,
            bool generateMipMaps,
            Task<IReadOnlyDisposableData> dataTask,
            ImportAddonInstanceCollection addons,
            CancellationToken cancellationToken
            )
        {
            using var data = await dataTask;
            if (data == null)
            {
                return ImageResult.Null;
            }

            while (context.DeferAgent.ShouldDefer())
            {
                cancellationToken.ThrowIfCancellationRequestedWithTracking();
                await Task.Yield();
            }

            var task = addons
                ?.First<ITextureImageLoader>(addon => addon.IsAbleToLoad(data.Data.AsReadOnlySpan()))
                ?.LoadImageAsync(
                    data.Data,
                    linear,
                    readable,
                    generateMipMaps,
                    cancellationToken);
            if (task != null)
            {
                return await task;
            }

            if (ImageFormatDetection.IsPngOrJpeg(data.Data.AsReadOnlySpan()))
            {
#if UNITY_IMAGECONVERSION
                return await ImageConversionImageLoader.LoadAsync(
                    context, settings, data.Data, linear, readable, cancellationToken);
#else
                context.Logger?.Error(LogCode.ImageConversionNotEnabled);
                return ImageResult.Null;
#endif
            }

            if (ImageFormatDetection.IsKtx(data.Data.AsReadOnlySpan()))
            {
#if KTX_IS_RECENT
                var result = await KtxImageLoader.LoadAsync(
                    context, settings, data.Data, linear, readable, cancellationToken);
                return result;
#else
                context.Logger?.Error(
                    LogCode.PackageMissing, "KTX for Unity", ExtensionName.TextureBasisUniversal);
                return ImageResult.Null;
#endif // KTX_IS_RECENT
            }

            if (ImageFormatDetection.IsWebP(data.Data.AsReadOnlySpan()))
            {
                context.Logger?.Error(
                    LogCode.ImageFormatUnsupported,
                    imageIndex.ToString(),
                    nameof(ImageFormat.WebP)
                    );
                return ImageResult.Null;
            }

            context.Logger?.Error(
                LogCode.ImageFormatUnknown,
                imageIndex.ToString());
            return ImageResult.Null;
        }

        internal static async Task<ImageResult> LoadImageAsync(
            Task<IReadOnlyDisposableData> dataTask,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken,
            ITextureImageLoader loader,
            IDeferAgent deferAgent
        )
        {
            using var data = await dataTask;
            if (data == null || !data.Data.IsCreated || data.Data.Length == 0)
            {
                return ImageResult.Null;
            }
            while (deferAgent.ShouldDefer())
            {
                cancellationToken.ThrowIfCancellationRequestedWithTracking();
                await Task.Yield();
            }
            return await loader.LoadImageAsync(data.Data, linear, readable, generateMipMaps, cancellationToken);
        }

        internal static async ValueTask<IReadOnlyDisposableData> LoadDataAsync(
            ImportContext context,
            Uri uri,
            CancellationToken cancellationToken
        )
        {
            var download = await context.DownloadProvider.RequestAsync(uri);
            if (download == null)
            {
                context.Logger?.Error(LogCode.TextureDownloadFailed, "?", uri.ToString());
                return null;
            }

            if (cancellationToken.IsCancellationRequested)
                return null;

            if (download.Success)
            {
                return new ReadOnlyData(download.Data, download);
            }

            context.Logger?.Error(LogCode.TextureDownloadFailed, download.Error, uri.ToString());
            return null;
        }
    }
}
