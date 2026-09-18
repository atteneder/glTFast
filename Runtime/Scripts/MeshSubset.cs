// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Unity.Cloud.Gltfast
{
    readonly struct MeshSubset
    {
        /// <summary>glTF mesh index.</summary>
        public readonly int meshIndex;

        /// <summary>
        /// glTF mesh subset numeration.
        /// glTF mesh primitives are clustered into one or more consecutively numbered MeshSubsets.
        /// </summary>
        public readonly int meshNumeration;

        /// <summary>
        /// Connects Unity sub-mesh indices to glTF mesh primitive indices.
        /// Key: Unity sub-mesh index.
        /// Value: glTF primitive index.
        /// </summary>
        public readonly int[] primitives;

        public MeshSubset(int meshIndex, int meshNumeration, int[] primitives)
        {
            this.meshIndex = meshIndex;
            this.meshNumeration = meshNumeration;
            this.primitives = primitives;
        }
    }
}
