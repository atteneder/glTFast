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
        internal static bool IsASkeletonMissing(this RootBase root)
        {
            if (root.Skins != null)
            {
                foreach (var skin in root.Skins)
                {
                    if (skin.skeleton < 0) return true;
                }
            }
            return false;
        }
    }
}
