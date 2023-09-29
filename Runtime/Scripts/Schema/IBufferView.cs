// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast.Schema
{
    /// <summary>
    /// A view into a buffer generally representing a subset of the buffer.
    /// </summary>
    public interface IBufferView
    {
        /// <summary>
        /// The index of the buffer.
        /// </summary>
        int Buffer { get; }

        /// <summary>
        /// The offset into the buffer in bytes.
        /// </summary>
        int ByteOffset { get; }

        /// <summary>
        /// The length of the bufferView in bytes.
        /// </summary>
        int ByteLength { get; }
    }
}
