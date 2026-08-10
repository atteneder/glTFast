// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if WEBP_IS_INSTALLED

using System;
using System.Threading;
using System.Threading.Tasks;
using GLTFast.Addons;
using GLTFast.Schema;
using Unity.Collections;
using UnityEngine;
using WebP;

namespace GLTFast
{
    /// <summary>
    /// Import add-on that registers native WebP texture support.
    /// Auto-registered by <see cref="ImportAddonRegistry"/> when the unity.webp package is installed.
    /// </summary>
    class WebpTextureImportAddon : ImportAddon<WebpTextureImportAddonInstance> { }

    /// <summary>
    /// Handles the EXT_texture_webp glTF extension and WebP image format detection.
    /// Decodes WebP texture data using the unity.webp package.
    /// </summary>
    class WebpTextureImportAddonInstance : ImportAddonInstance, ITextureImageLoader
    {
        /// <inheritdoc />
        public override void Inject(GltfImportBase gltfImport)
        {
            gltfImport.AddImportAddonInstance(this);
        }

        /// <inheritdoc />
        public override void Inject(IInstantiator instantiator) { }

        /// <inheritdoc />
        public override void Dispose() { }

        /// <inheritdoc />
        public override bool SupportsGltfExtension(string extensionName)
        {
            return extensionName == ExtensionName.TextureWebP;
        }

        /// <summary>
        /// Checks if the texture has the EXT_texture_webp extension and returns the WebP image index.
        /// </summary>
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

        /// <summary>
        /// Detects WebP format from raw byte data (RIFF????WEBP header).
        /// </summary>
        public bool IsAbleToLoad(ReadOnlySpan<byte> data)
        {
            return ImageFormatDetection.IsWebP(data);
        }

        /// <summary>
        /// Decodes WebP image data into a Unity Texture2D.
        /// </summary>
        public async Task<ImageResult> LoadImage(
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
                Debug.LogError($"[glTFast] WebP texture decode failed: {error}");
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
