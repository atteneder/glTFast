// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace Unity.Cloud.Gltfast
{
    readonly struct MeshAssignment
    {
        public readonly Mesh mesh;

        /// <summary>
        /// Key: sub-mesh index
        /// Value: primitive index
        /// </summary>
        public readonly int[] primitives;

        public MeshAssignment(Mesh mesh, int[] primitives)
        {
            this.mesh = mesh;
            this.primitives = primitives;
        }
    }
}
