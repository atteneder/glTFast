// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Resolves glTF indices against the collections they point into. A glTF index may be absent, and a malformed
    /// document may reference an element that does not exist, so absent, negative and out-of-range indices as well as
    /// an absent collection all resolve to a failure.
    /// </summary>
    static class GltfIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetElement<T>(List<T> list, int? index, out T element)
        {
            if (list != null && TryGetIndex(index, list.Count, out var i))
            {
                element = list[i];
                return true;
            }

            element = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetIndex(int? index, int count, out int value)
        {
            if (index is { } i && i >= 0 && i < count)
            {
                value = i;
                return true;
            }

            value = default;
            return false;
        }

        public static string Describe(int? index) => index?.ToString() ?? "null";
    }
}
