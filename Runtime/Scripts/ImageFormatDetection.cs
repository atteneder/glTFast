// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Methods for detecting image formats from byte data.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public static class ImageFormatDetection
    {
        static readonly byte[] k_PNGHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        static readonly byte[] k_JpegHeader = { 0xff, 0xd8, 0xff };
        static readonly byte[] k_KtxHeaderPart1 = { 0xAB, 0x4B, 0x54, 0x58, 0x20 };
        static readonly byte[] k_KtxHeaderPart2 = { 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
        static readonly byte[] k_WebPHeaderPart1 = { 0x52, 0x49, 0x46, 0x46 };
        static readonly byte[] k_WebPHeaderPart2 = { 0x57, 0x45, 0x42, 0x50 };

        /// <summary>
        /// Determines if the data is in PNG or Jpeg format.
        /// </summary>
        /// <param name="data">Image data. Can be the first 8 bytes only.</param>
        /// <returns>True if the data is PNG or Jpeg, false otherwise.</returns>
        public static bool IsPngOrJpeg(ReadOnlySpan<byte> data)
        {
            return IsPng(data) || IsJpeg(data);
        }

        /// <summary>
        /// Determines if the data is in PNG format.
        /// </summary>
        /// <param name="data">Image data. Can be the first 8 bytes only.</param>
        /// <returns>True if the data is PNG, false otherwise.</returns>
        public static bool IsPng(ReadOnlySpan<byte> data)
        {
            return data.StartsWith(k_PNGHeader);
        }

        /// <summary>
        /// Determines if the data is in Jpeg format.
        /// </summary>
        /// <param name="data">Image data. Can be the first 3 bytes only.</param>
        /// <returns>True if the data is Jpeg, false otherwise.</returns>
        public static bool IsJpeg(ReadOnlySpan<byte> data)
        {
            return data.StartsWith(k_JpegHeader);
        }

        /// <summary>
        /// Determines if the data is in KTX format.
        /// </summary>
        /// <param name="data">Image data. Can be the first 12 bytes only.</param>
        /// <returns>True if the data is KTX, false otherwise.</returns>
        public static bool IsKtx(ReadOnlySpan<byte> data)
        {
            // Header is
            // "«KTX XX»\r\n\x1A\n"
            // where XX is the version number, e.g., "20" for KTX2
            return data.Length >= 12
                && data[..5].SequenceEqual(k_KtxHeaderPart1)
                && data.Slice(7, 5).SequenceEqual(k_KtxHeaderPart2);
        }

        /// <summary>
        /// Determines if the data is in WebP format.
        /// </summary>
        /// <param name="data">Image data. Can be the first 12 bytes only.</param>
        /// <returns>True if the data is WebP, false otherwise.</returns>
        public static bool IsWebP(ReadOnlySpan<byte> data)
        {
            // Header is "RIFF????WEBP" where ? is the file size.
            return data.Length >= 12
                && data[..4].SequenceEqual(k_WebPHeaderPart1)
                && data.Slice(8, 4).SequenceEqual(k_WebPHeaderPart2);
        }
    }
}
