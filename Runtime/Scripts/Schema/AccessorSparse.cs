// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace GLTFast.Schema
{
    /// <inheritdoc/>
    [System.Serializable]
    public class AccessorSparse : AccessorSparseBase<AccessorSparseIndices, AccessorSparseValues> { }

    /// <inheritdoc/>
    [System.Serializable]
    public abstract class AccessorSparseBase<TIndices, TValues> : AccessorSparseBase
    where TIndices : AccessorSparseIndices
    where TValues : AccessorSparseValues
    {
        /// <inheritdoc cref="Indices"/>
        public TIndices indices;

        /// <inheritdoc cref="Values"/>
        public TValues values;

        /// <inheritdoc cref="AccessorSparseBase.Indices"/>
        public override AccessorSparseIndices Indices => indices;

        /// <inheritdoc cref="AccessorSparseBase.Values"/>
        public override AccessorSparseValues Values => values;
    }

    /// <summary>
    /// Sparse property of a glTF
    /// </summary>
    /// <seealso cref="Accessor"/>
    [System.Serializable]
    public abstract class AccessorSparseBase
    {
        /// <summary>
        /// Number of entries stored in the sparse array.
        /// </summary>
        public int count;

        /// <summary>
        /// Index array of size `count` that points to those accessor attributes that
        /// deviate from their initialization value. Indices must strictly increase.
        /// </summary>
        public abstract AccessorSparseIndices Indices { get; }

        /// <summary>
        /// "Array of size `count` times number of components, storing the displaced
        /// accessor attributes pointed by `indices`. Substituted values must have
        /// the same `componentType` and number of components as the base accessor.
        /// </summary>
        public abstract AccessorSparseValues Values { get; }

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.AddProperty("count", count);
            if (Indices != null)
            {
                writer.AddProperty("indices");
                Indices.GltfSerialize(writer);
            }
            if (Values != null)
            {
                writer.AddProperty("values");
                Values.GltfSerialize(writer);
            }
            writer.Close();
        }

    }
}
