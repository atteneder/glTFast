// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using Unity.Cloud.Gltfast.Objects;
using Unity.Cloud.Gltfast.Text.Json;
using UnityEngine;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Extension methods for <see cref="Root"/>
    /// </summary>
    public static class RootExtension
    {
        /// <summary>
        /// Serialization to JSON
        /// </summary>
        /// <param name="root">Root glTF object to be serialized.</param>
        /// <param name="stream"><see cref="Stream"/> the JSON string is being written to.</param>
        public static void Serialize(this Root root, Stream stream)
        {
            JsonSerializer.Serialize(stream, root, GltfJsonContext.Default.Root);
        }

        /// <summary>
        /// Figures if any skins' skeleton property is not set.
        /// </summary>
        /// <param name="root">glTF Root object</param>
        /// <returns>True if the skeleton property on any skin is not set, false otherwise.</returns>
        internal static bool IsASkeletonMissing(this Root root)
        {
            if (root.Skins != null)
            {
                foreach (var skin in root.Skins)
                {
                    if (skin.Skeleton < 0) return true;
                }
            }
            return false;
        }
    }
}
