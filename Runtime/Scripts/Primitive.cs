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
