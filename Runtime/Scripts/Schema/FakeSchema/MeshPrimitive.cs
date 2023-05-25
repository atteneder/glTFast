// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if DRACO_UNITY

namespace GLTFast.FakeSchema {
    // ReSharper disable InconsistentNaming
    [System.Serializable]
    class MeshPrimitive {
        public MeshPrimitiveExtensions extensions;
    }

    [System.Serializable]
    class MeshPrimitiveExtensions {
        public string KHR_draco_mesh_compression;
    }
    // ReSharper restore InconsistentNaming
}

#endif
