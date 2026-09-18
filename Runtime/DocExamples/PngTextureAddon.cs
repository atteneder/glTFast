// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Threading.Tasks;
using GLTFast.Addons;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;

namespace GLTFast.Documentation.Examples
{
    class PngTextureAddon : ImportAddon<PngTextureAddonInstance> { }

    class PngTextureAddonInstance : ImageLoaderAddonInstance, IDefaultImageFormatLoader
    {
        public override void Inject(GltfImportBase gltfImport)
        {
            gltfImport.AddImportAddonInstance(this);
        }

        public bool IsAbleToLoad(ImageFormat format)
        {
#if UNITY_IMAGECONVERSION
            return format == ImageFormat.Png;
#else
            return false;
#endif
        }

        public bool IsAbleToLoad(ReadOnlySpan<byte> data)
        {
            return ImageFormatDetection.IsPng(data);
        }

        public Task<ImageResult> LoadImage(
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            bool generateMipMaps,
            CancellationToken cancellationToken
            )
        {
#if UNITY_IMAGECONVERSION
            Profiler.BeginSample("LoadPNG");
            var texture = CreateEmptyTexture(linear, generateMipMaps);
            var success = texture.LoadImage(data.AsReadOnlySpan(), !readable);
            Profiler.EndSample();
            if (success)
            {
                return Task.FromResult(new ImageResult(texture));
            }
#endif // UNITY_IMAGECONVERSION
            return Task.FromResult(ImageResult.Null);
        }

        static Texture2D CreateEmptyTexture(
            bool forceSampleLinear,
            bool generateMipMaps
        )
        {
            var textureCreationFlags = TextureCreationFlags.DontUploadUponCreate | TextureCreationFlags.DontInitializePixels;
            if (generateMipMaps)
            {
                textureCreationFlags |= TextureCreationFlags.MipChain;
            }
            var txt = new Texture2D(
                4, 4,
                forceSampleLinear
                    ? GraphicsFormat.R8G8B8A8_UNorm
                    : GraphicsFormat.R8G8B8A8_SRGB,
                textureCreationFlags
            );
            return txt;
        }
    }
}
