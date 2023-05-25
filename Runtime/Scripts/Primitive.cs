// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast
{

    struct Primitive
    {
        public UnityEngine.Mesh mesh;
        public int[] materialIndices;

        public Primitive(UnityEngine.Mesh mesh, int[] materialIndex)
        {
            this.mesh = mesh;
            this.materialIndices = materialIndex;
        }
    }
}
