// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

// currently no good way to detect exception support setting in WebGL, so disabling by default here
#if !UNITY_WEBGL || UNITY_EDITOR
#define GLTFAST_FULL_EXCEPTION_SUPPORT
#endif

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Cloud.Gltfast
{
    static class StreamExtension
    {
        public static async Task<bool> ReadToArrayAsync(
            this Stream stream,
            byte[] destination,
            int offset,
            int length,
            CancellationToken cancellationToken
            )
        {
            int readBytes;
            var pendingBytes = length;
            try
            {
                do
                {
                    readBytes = await stream.ReadAsync(destination, offset, pendingBytes
#if GLTFAST_FULL_EXCEPTION_SUPPORT
                        , cancellationToken
#endif
                        );
                    pendingBytes -= readBytes;
                    offset += readBytes;
                } while (pendingBytes > 0 && readBytes > 0);
            }
            catch (TaskCanceledException)
            {
                cancellationToken.ThrowIfCancellationRequestedWithTracking();
            }

            return pendingBytes <= 0;
        }
    }
}
