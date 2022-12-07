// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using UnityEngine;

namespace GLTFast.Schema
{

    /// <summary>
    /// Extension for enabling GPU instancing, rendering many copies of a
    /// single mesh at once using a small number of draw calls.
    /// <seealso href="https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Vendor/EXT_mesh_gpu_instancing"/>
    /// </summary>
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
