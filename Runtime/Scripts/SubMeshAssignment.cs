// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Cloud.Gltfast.Objects;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Usually one glTF primitive relates to one Unity sub-mesh.
    /// Sometimes the primitives of one mesh share the same vertex buffer accessors. To avoid duplicate import of those
    /// vertex buffers this struct reassigns the vertex buffer of one primitive (at VertexBufferIndex)
    /// to another (Primitive).
    /// </summary>
    readonly struct SubMeshAssignment
    {
        public MeshPrimitive Primitive { get; }
        public int VertexBufferIndex { get; }

        public SubMeshAssignment(MeshPrimitive primitive, int vertexBufferIndex)
        {
            Primitive = primitive;
            VertexBufferIndex = vertexBufferIndex;
        }
    }
}
