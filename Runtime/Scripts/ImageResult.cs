// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Imported glTF image containing a <see cref="Texture2D" /> and metadata.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "GLTFast", sourceAssembly: "glTFast")]
    public struct ImageResult
    {
        /// <summary>
        /// Imported texture.
        /// </summary>
        public Texture2D Texture { get; }

        /// <summary>
        /// If true, the image is flipped vertically.
        /// </summary>
        public bool IsYFlipped { get; }

        /// <summary>
        /// Empty image. Indicates a failed load.
        /// </summary>
        public static ImageResult Null => new(null);

        /// <summary>
        /// Default Image constructor.
        /// </summary>
        /// <param name="texture">Imported texture.</param>
        /// <param name="isYFlipped">Must be true if texture is flipped vertically.</param>
        public ImageResult(Texture2D texture, bool isYFlipped = false)
        {
            Texture = texture;
            IsYFlipped = isYFlipped;
        }
    }
}
