// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast
{

    /// <summary>
    /// Abstract glTF component type used for including/excluding specific
    /// features from import/export.
    /// </summary>
    [Flags]
    public enum ComponentType
    {
        /// <summary>
        /// No components
        /// </summary>
        None = 0,
        /// <summary>
        /// Meshes (Primitives)
        /// </summary>
        Mesh = 1 << 1,
        /// <summary>
        /// Animation
        /// </summary>
        Animation = 1 << 2,
        /// <summary>
        /// Cameras
        /// </summary>
        Camera = 1 << 3,
        /// <summary>
        /// Lights
        /// </summary>
        Light = 1 << 4,
        /// <summary>
        /// All component types
        /// </summary>
        All = ~0,
    }
}
