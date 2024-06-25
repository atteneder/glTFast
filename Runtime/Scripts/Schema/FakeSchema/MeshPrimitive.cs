// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.FakeSchema
{
    // ReSharper disable InconsistentNaming
    [System.Serializable]
    class MeshPrimitive
    {
        public MeshPrimitiveExtensions extensions;
    }

    [System.Serializable]
    class MeshPrimitiveExtensions
    {
#if DRACO_UNITY
        public string KHR_draco_mesh_compression;
#endif
        public string KHR_materials_variants;
    }
    // ReSharper restore InconsistentNaming
}
