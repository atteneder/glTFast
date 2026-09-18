// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast.Objects
{
    /// <summary>
    /// The topology type of primitives to render.
    /// </summary>
    /// <seealso href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#_mesh_primitive_mode"/>
    public enum PrimitiveMode
    {
        /// <summary>Points</summary>
        Points = 0,
        /// <summary>Lines</summary>
        Lines = 1,
        /// <summary>Line loop</summary>
        LineLoop = 2,
        /// <summary>Line strip</summary>
        LineStrip = 3,
        /// <summary>Triangles</summary>
        Triangles = 4,
        /// <summary>Triangle strip</summary>
        TriangleStrip = 5,
        /// <summary>Triangle fan</summary>
        TriangleFan = 6
    }
}
