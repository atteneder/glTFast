// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using GLTFast.Addons;
using GLTFast.Schema;
using Unity.Collections;
using UnityEngine;

namespace GLTFast.Documentation.Examples
{
    class WebpTextureAddon : ImportAddon<WebpTextureAddonInstance> { }

    class WebpTextureAddonInstance : ImageLoaderAddonInstance, ITextureImageLoader
    {
        public override void Inject(GltfImportBase gltfImport)
        {
            gltfImport.AddImportAddonInstance(this);
        }

        public override bool SupportsGltfExtension(string extensionName)
        {
            return extensionName == "EXT_texture_webp";
        }

        public bool IsAbleToLoad(TextureBase texture, out int imageIndex)
        {
            if (texture.Extensions?.EXT_texture_webp is { source: >= 0 } webp)
            {
                imageIndex = webp.source;
                return true;
            }
            imageIndex = -1;
            return false;
        }

        public bool IsAbleToLoad(ReadOnlySpan<byte> data)
        {
            return ImageFormatDetection.IsWebP(data);
        }

        public async Task<ImageResult> LoadImage(
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
}

