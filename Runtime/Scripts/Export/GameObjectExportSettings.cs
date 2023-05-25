// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast.Export
{

    /// <summary>
    /// GameObject hierarchies related glTF export settings
    /// </summary>
    public class GameObjectExportSettings
    {
        /// <summary>
        /// When true, only GameObjects that are active (in a hierarchy) are exported
        /// </summary>
        public bool OnlyActiveInHierarchy { get; set; } = true;

        /// <summary>
        /// When true, components will get exported regardless whether they're
        /// enabled or not.
        /// </summary>
        public bool DisabledComponents { get; set; }

        /// <summary>
        /// Only GameObjects on layers contained in this mask are going to get exported.
        /// </summary>
        public LayerMask LayerMask { get; set; } = ~0;
    }
}
