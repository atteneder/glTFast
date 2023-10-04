// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{

    /// <summary>
    /// Sparse values property of a glTF
    /// </summary>
    /// <seealso cref="AccessorSparse"/>
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
