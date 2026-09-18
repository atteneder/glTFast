// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Objects;
using UnityEngine;
using Texture = Unity.Cloud.Gltfast.Objects.Texture;

namespace Unity.Cloud.Gltfast.Export
{
    static class TextureComparer
    {
        /// <summary>
        /// Compares two textures based on their source and sampler only.
        /// </summary>
        /// <param name="x">First texture.</param>
        /// <param name="y">Second texture.</param>
        /// <returns>True if textures have identical source and sampler, false otherwise.</returns>
        public static bool Equals(Texture x, Texture y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null) return false;
            if (y is null) return false;
            return x.Sampler == y.Sampler
                && x.Source == y.Source;
        }
    }
}
