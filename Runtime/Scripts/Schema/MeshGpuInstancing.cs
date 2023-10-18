// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace GLTFast.Schema
{

    /// <summary>
    /// Extension for enabling GPU instancing, rendering many copies of a
    /// single mesh at once using a small number of draw calls.
    /// </summary>
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Vendor/EXT_mesh_gpu_instancing"/>
    [System.Serializable]
    public class MeshGpuInstancing
    {

        /// <summary>
        /// Per-instance attributes collection
        /// </summary>
        [System.Serializable]
        public class Attributes
        {

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// Instance positions accessor index
            /// </summary>
            public int TRANSLATION = -1;

            /// <summary>
            /// Instance rotations accessor index
            /// </summary>
            public int ROTATION = -1;

            /// <summary>
            /// Instance scales accessor index
            /// </summary>
            public int SCALE = -1;

            // ReSharper restore InconsistentNaming
        }

        /// <inheritdoc cref="Attributes"/>
        public Attributes attributes;

        internal void GltfSerialize(JsonWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}
