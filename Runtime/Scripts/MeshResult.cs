// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast
{

    /// <summary>
    /// This struct holds the result of a glTF to Unity mesh conversion.
    /// During an import, glTF meshes (composed of primitives) will be converted to Unity Meshes (with sub-meshes).
    /// glTF meshes and Unity Meshes do not necessarily relate one-to-one. One glTF mesh (with multiple primitives)
    /// might be converted to multiple Unity Meshes (e.g. because of incompatible vertex buffer structure).
    /// </summary>
    public readonly struct MeshResult
    {
        /// <summary>Original glTF mesh index</summary>
        public readonly int meshIndex;

        /// <summary>Original glTF mesh primitive index per sub-mesh</summary>
        public readonly int[] primitiveIndices;

        /// <summary>glTF material index per sub-mesh</summary>
        public readonly int[] materialIndices;

        /// <summary>Converted Unity Mesh</summary>
        public readonly UnityEngine.Mesh mesh;

        // public readonly Dictionary<Extension, object> extensionData;

        /// <summary>
        /// MeshResult Constructor
        /// </summary>
        /// <param name="meshIndex">Original glTF mesh index</param>
        /// <param name="primitiveIndices">Original glTF mesh primitive index per sub-mesh</param>
        /// <param name="materialIndices">glTF material index per sub-mesh</param>
        /// <param name="mesh">Converted Unity Mesh</param>
        public MeshResult(
            int meshIndex,
            int[] primitiveIndices,
            int[] materialIndices,
            UnityEngine.Mesh mesh
            )
        {
            this.meshIndex = meshIndex;
            this.primitiveIndices = primitiveIndices;
            this.materialIndices = materialIndices;
            this.mesh = mesh;
        }
    }
}
