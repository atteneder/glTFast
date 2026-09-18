// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Image format.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public enum ImageFormat
    {
        /// <summary>Unknown image format</summary>
        Unknown,
        /// <summary>Portable Network Graphics</summary>
        Png,
        /// <summary>JPEG File Interchange Format</summary>
        Jpeg,
        /// <summary>KTX 2.0 GPU Texture Container Format</summary>
        Ktx,
        /// <summary>WebP</summary>
        /// <seealso href="https://developers.google.com/speed/webp"/>
        WebP,

        /// <summary>JPEG File Interchange Format</summary>
        [Obsolete("Use Jpeg instead.")]
        Jpg = Jpeg,
    }
}
