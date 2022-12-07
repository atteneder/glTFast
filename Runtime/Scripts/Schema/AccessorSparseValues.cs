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
    /// Sparse values property of a glTF <seealso cref="AccessorSparse"/>
    /// </summary>
    [System.Serializable]
    public class AccessorSparseValues
    {
        /// <summary>
        /// The index of the bufferView with sparse values.
        /// Referenced bufferView can't have ARRAY_BUFFER or ELEMENT_ARRAY_BUFFER target.
        /// </summary>
        public uint bufferView;

        /// <summary>
        /// The offset relative to the start of the bufferView in bytes. Must be aligned.
        /// <minimum>0</minimum>
        /// </summary>
        public int byteOffset;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.AddProperty("bufferView", bufferView);
            if (byteOffset >= 0)
            {
                writer.AddProperty("byteOffset", byteOffset);
            }
            writer.Close();
        }
    }
}
