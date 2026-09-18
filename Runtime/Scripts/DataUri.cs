// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Text;
using Unity.Mathematics;

namespace Unity.Cloud.Gltfast
{
    static class DataUri
    {
        static ReadOnlySpan<byte> DataPrefix => new[]
        {
            (byte)'d', (byte)'a', (byte)'t', (byte)'a', (byte)':'
        };

        static ReadOnlySpan<byte> Base64Marker => new[]
        {
            (byte)'b', (byte)'a', (byte)'s', (byte)'e', (byte)'6', (byte)'4', (byte)','
        };

        public static bool IsDataUri(ReadOnlySpan<byte> utf8)
        {
            return utf8.StartsWith(DataPrefix);
        }

        /// <summary>
        /// Parses a <c>data:&lt;mime&gt;;base64,&lt;payload&gt;</c> URI's descriptor portion.
        /// </summary>
        /// <param name="utf8">UTF-8 bytes of the full data URI.</param>
        /// <param name="mimeType">MIME type substring (decoded as ASCII).</param>
        /// <param name="payloadStartIndex">Byte index of the start of the base-64 payload.</param>
        /// <param name="decodedByteLength">Predicted decoded byte length of the payload.</param>
        /// <returns>True if the descriptor is well-formed and the encoding is <c>base64</c>.</returns>
        public static bool TryGetDataUriDescriptor(
            ReadOnlySpan<byte> utf8,
            out string mimeType,
            out int payloadStartIndex,
            out int decodedByteLength)
        {
            const int prefixLength = 5; // "data:"
            // Cap the MIME type segment search to a reasonable size to avoid scanning huge payloads.
            var searchLength = math.min(utf8.Length - prefixLength, 1000);
            var mediaTypeEnd = utf8.Slice(prefixLength, searchLength).IndexOf((byte)';');
            if (mediaTypeEnd < 0)
            {
                mimeType = null;
                payloadStartIndex = 0;
                decodedByteLength = -1;
                return false;
            }
            mediaTypeEnd += prefixLength;
            var mimeBytes = utf8.Slice(prefixLength, mediaTypeEnd - prefixLength);
            var encodingStart = mediaTypeEnd + 1;
            if (utf8.Length < encodingStart + Base64Marker.Length
                || !utf8.Slice(encodingStart, Base64Marker.Length).SequenceEqual(Base64Marker))
            {
                mimeType = null;
                payloadStartIndex = 0;
                decodedByteLength = -1;
                return false;
            }

            mimeType = Encoding.ASCII.GetString(mimeBytes);
            payloadStartIndex = encodingStart + Base64Marker.Length;

            var padding = 0;
            if (utf8.Length > 0 && utf8[utf8.Length - 1] == (byte)'=')
            {
                padding = utf8.Length > 1 && utf8[utf8.Length - 2] == (byte)'=' ? 2 : 1;
            }
            decodedByteLength = (int)(((long)(utf8.Length - payloadStartIndex) * 3 + 3) / 4 - padding);
            return true;
        }
    }
}
