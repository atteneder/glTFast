// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Gltfast.Addons;
using Unity.Cloud.Gltfast.Objects;
using Unity.Collections;
using UnityEngine;
using Texture = Unity.Cloud.Gltfast.Objects.Texture;

namespace Unity.Cloud.Gltfast.Documentation.Examples
{
    class WebpTextureAddon : ImportAddon<WebpTextureAddonInstance> { }

    class WebpTextureAddonInstance : ImageLoaderAddonInstance, ITextureImageLoader
    {
        public override void Inject(GltfImport gltfImport)
        {
            gltfImport?.AddImportAddonInstance(this);
        }

        public override bool SupportsGltfExtension(string extensionName)
        {
            return extensionName == "EXT_texture_webp";
        }

        public bool IsAbleToLoad(Texture texture, out int imageIndex)
        {
            if (texture?.Extensions != null
                && texture.Extensions.TryGetValue<TextureWebpExtension>(
                    "EXT_texture_webp", out var ext))
            {
                imageIndex = ext.source;
                return true;
            }

            imageIndex = -1;
            return false;
        }

        public bool IsAbleToLoad(ReadOnlySpan<byte> data)
        {
            return ImageFormatDetection.IsWebP(data);
        }

        public async Task<ImageResult> LoadImageAsync(
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken
            )
        {
            var texture = await WebP.Decode(data, linear, readable, cancellationToken);
            return new ImageResult(texture, true);
        }
    }

    struct TextureWebpExtension
    {
        public int source;
    }
}
