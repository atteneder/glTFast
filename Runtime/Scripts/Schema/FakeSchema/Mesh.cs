// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.FakeSchema
{

    [System.Serializable]
    class Mesh : NamedObject
    {
        public MeshPrimitive[] primitives;
    }
}
