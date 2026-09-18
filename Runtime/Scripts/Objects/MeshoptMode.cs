// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// Mode defines the type of buffer to decode
    /// </summary>
    public enum MeshoptMode
    {
        /// <summary>
        /// Don't use this value as parameter directly!
        /// It's for deserialization purpose only.
        /// </summary>
        Undefined,
        /// <summary>
        /// Vertex attributes
        /// </summary>
        Attributes,
        /// <summary>
        /// Triangle indices buffer
        /// </summary>
        Triangles,
        /// <summary>
        /// Index sequence
        /// </summary>
        Indices,
    }
}
