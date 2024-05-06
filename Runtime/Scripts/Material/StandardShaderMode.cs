// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast.Materials
{
    /// <summary>
    /// Built-In render pipeline Standard shader modes
    /// </summary>
    public enum StandardShaderMode
    {
        /// <summary>
        /// Opaque mode
        /// </summary>
        Opaque = 0,
        /// <summary>
        /// Cutout mode (alpha test)
        /// </summary>
        Cutout = 1,
        /// <summary>
        /// Fade mode (alpha blended opacity)
        /// </summary>
        Fade = 2,
        /// <summary>
        /// Transparent mode (alpha blended transmission; e.g. glass)
        /// </summary>
        Transparent = 3
    }
}
