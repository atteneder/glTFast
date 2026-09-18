// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Magnification filter mode.
    /// </summary>
    public enum MagFilterMode
    {
        /// <summary>Undefined</summary>
        Undefined = 0,
        /// <summary>Nearest pixel sampling</summary>
        Nearest = 9728,
        /// <summary>Linear pixel interpolation sampling</summary>
        Linear = 9729,
    }
}
