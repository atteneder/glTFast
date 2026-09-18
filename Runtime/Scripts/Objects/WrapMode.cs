// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Texture wrap mode.
    /// </summary>
    public enum WrapMode
    {
        /// <summary>Undefined</summary>
        Undefined = 0,
        /// <summary>Clamp to edge</summary>
        ClampToEdge = 33071,
        /// <summary>Mirrored repeat</summary>
        MirroredRepeat = 33648,
        /// <summary>Repeat</summary>
        Repeat = 10497
    }
}
