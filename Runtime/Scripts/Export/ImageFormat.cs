// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast.Export
{
    /// <summary>
    /// Exported image file format
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>
        /// Unknown, no preferred file format
        /// </summary>
        Unknown,
        /// <summary>
        /// Jpeg file format
        /// </summary>
        Jpg,
        /// <summary>
        /// PNG (Portable Network Graphics) file format
        /// </summary>
        Png
    }
}
