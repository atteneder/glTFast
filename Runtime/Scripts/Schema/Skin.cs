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

namespace GLTFast.Schema
{

    /// <summary>
    /// Joints and matrices defining a skinned mesh.
    /// </summary>
    [System.Serializable]
    public class Skin : NamedObject
    {

        /// <summary>
        /// The index of the accessor containing the
        /// floating-point 4x4 inverse-bind matrices.
        /// </summary>
        public int inverseBindMatrices = -1;

        /// <summary>
        /// The index of the node used as a skeleton root.
        /// </summary>
        public int skeleton = -1;

        /// <summary>
        /// Indices of skeleton nodes, used as joints in this skin.
        /// </summary>
        public uint[] joints;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            GltfSerializeRoot(writer);
            writer.Close();
            throw new System.NotImplementedException($"GltfSerialize missing on {GetType()}");
        }
    }
}
