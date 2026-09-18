// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Outcome of a buffer data request on an <see cref="IGltfBufferData"/>.
    /// </summary>
    public enum BufferAccessStatus
    {
        /// <summary>
        /// The requested data was provided.
        /// </summary>
        Success,

        /// <summary>
        /// There is no buffer view or accessor at the requested index, or the one there references
        /// a buffer that does not exist.
        /// </summary>
        ObjectIndexOutOfRange,

        /// <summary>
        /// The buffer's memory is not available. Either the lease was disposed, or the glTF
        /// import it belongs to released its buffers.
        /// </summary>
        BufferUnavailable,

        /// <summary>
        /// The requested element type does not match the accessor's component type and accessor
        /// type combination.
        /// </summary>
        TypeMismatch,

        /// <summary>
        /// The requested offset + length exceeds the capacity of the accessor or bufferView or buffer.
        /// </summary>
        DataIndexOutOfRange,

        /// <summary>
        /// The accessor is sparse. Sparse accessor data is not provided by this API.
        /// </summary>
        SparseUnsupported,

        /// <summary>
        /// The accessor's data is interleaved, because its buffer view declares a byte stride that
        /// differs from the element size. Only tightly packed data can be provided as a plain
        /// array; request interleaved data via
        /// <see cref="IGltfBufferData.GetStridedAccessorData{T}"/> instead.
        /// </summary>
        StridedUnsupported
    }
}
