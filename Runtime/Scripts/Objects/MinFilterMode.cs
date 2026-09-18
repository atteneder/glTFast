// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Minification filter mode.
    /// </summary>
    public enum MinFilterMode
    {
        /// <summary>Undefined</summary>
        Undefined = 0,
        /// <summary>Nearest pixel sampling</summary>
        Nearest = 9728,
        /// <summary>Linear pixel interpolation sampling</summary>
        Linear = 9729,
        /// <summary>Nearest pixel and nearest mipmap sampling</summary>
        NearestMipmapNearest = 9984,
        /// <summary>Linear pixel interpolation and nearest mipmap sampling</summary>
        LinearMipmapNearest = 9985,
        /// <summary>Nearest pixel and linear mipmap interpolation sampling</summary>
        NearestMipmapLinear = 9986,
        /// <summary>Linear pixel interpolation and linear mipmap interpolation sampling</summary>
        LinearMipmapLinear = 9987
    }
}
