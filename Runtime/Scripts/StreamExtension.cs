// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading;
using UnityEngine;

using System.Threading.Tasks;

namespace GLTFast
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
            do
            {
                readBytes = await stream.ReadAsync(destination, offset, pendingBytes, cancellationToken);
                pendingBytes -= readBytes;
                offset += readBytes;
            } while (pendingBytes > 0 && readBytes > 0);

            return pendingBytes <= 0;
        }
    }
}
