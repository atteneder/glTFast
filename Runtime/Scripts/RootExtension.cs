// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using GLTFast.Schema;
using UnityEngine;

namespace GLTFast
{

    static class RootExtension
    {

        /// <summary>
        /// Figures if any skins' skeleton property is not set.
        /// </summary>
        /// <param name="root">glTF Root object</param>
        /// <returns>True if the skeleton property on any skin is not set, false otherwise.</returns>
        internal static bool IsASkeletonMissing(this Root root)
        {
            if (root.skins != null)
            {
                foreach (var skin in root.skins)
                {
                    if (skin.skeleton < 0) return true;
                }
            }
            return false;
        }
    }
}
