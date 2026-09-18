// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace GLTFast.Documentation.Examples
{
    static class WebP
    {
        public static async Task<Texture2D> Decode(
            NativeArray<byte>.ReadOnly data,
            bool linear,
            bool readable,
            CancellationToken cancellationToken
            )
        {
            if (!TryGetInfo(data, out var width, out var height))
            {
                return null;
            }
            Profiler.BeginSample("WebPCreateTexture2D");
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, linear);
            var textureData = texture.GetRawTextureData<byte>();
            using var result = new NativeArray<IntPtr>(1, Allocator.Persistent);
            var job = new WebPDecodeJob
            {
                data = data,
                textureData = textureData,
                width = width,
                result = result
            }.Schedule();
            Profiler.EndSample();

            while (!job.IsCompleted)
            {
                await Task.Yield();
            }
            job.Complete();

            if (result[0] == IntPtr.Zero)
            {
                UnityEngine.Object.Destroy(texture);
                return null;
            }
            Profiler.BeginSample("WebPApply");
            texture.Apply(false, !readable);
            Profiler.EndSample();
            return texture;
        }

        static unsafe bool TryGetInfo(NativeArray<byte>.ReadOnly data, out int width, out int height)
        {
            width = 0;
            height = 0;
            try
            {
                var returnValue = WebPGetInfo((byte*)data.GetUnsafeReadOnlyPtr(), (uint)data.Length, ref width, ref height);
                return returnValue != 0;
            }
            catch (DllNotFoundException)
            {
                Debug.LogError("For this example to work, you need to compile <a href=\"https://chromium.googlesource.com/webm/libwebp\">libwebp</a> as a native plugin and name it 'webp-unity'.");
            }

            return false;
        }

        unsafe struct WebPDecodeJob : IJob
        {
            [WriteOnly]
            public NativeArray<IntPtr> result;

            [ReadOnly]
            public NativeArray<byte>.ReadOnly data;

            [WriteOnly]
            public NativeArray<byte> textureData;

            public int width;

            public void Execute()
            {
                var decodeResult = WebPDecodeRGBAInto(
                    (byte*)data.GetUnsafeReadOnlyPtr(), (uint)data.Length,
                    (byte*)textureData.GetUnsafePtr(), (uint)textureData.Length,
                    sizeof(Color32) * width
                );

                result[0] = decodeResult;
            }
        }

        [DllImport("webp-unity")]
        public static extern unsafe int WebPGetInfo(byte* data, uint size, ref int width, ref int height);

        [DllImport("webp-unity")]
        public static extern unsafe IntPtr WebPDecodeRGBAInto(
            byte* data, uint size,
            byte* outputBuffer, uint outputBufferSize,
            int outputStride);
    }
}
