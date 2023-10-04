// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if MESHOPT
using System;
using Meshoptimizer;

namespace GLTFast.Schema
{
    [Serializable]
    public class BufferViewMeshoptExtension : IBufferView {

        /// <summary>
        /// The index of the buffer.
        /// </summary>
        public int buffer;

        /// <summary>
        /// The offset into the buffer in bytes.
        /// </summary>
        public int byteOffset;

        /// <summary>
        /// The length of the bufferView in bytes.
        /// </summary>
        public int byteLength;

        /// <summary>
        /// The stride, in bytes, between vertex attributes or other interleaved data.
        /// When this is zero, data is tightly packed.
        /// </summary>
        public int byteStride = -1;

        public int count;

        // Field is public for unified serialization only. Warn via Obsolete attribute.
        [Obsolete("Use GetMode for access.")]
        public string mode;

        // Field is public for unified serialization only. Warn via Obsolete attribute.
        [Obsolete("Use GetFilter for access.")]
        public string filter;

        Mode m_ModeEnum = Mode.Undefined;
        Filter m_FilterEnum = Filter.Undefined;

        public int Buffer => buffer;
        public int ByteOffset => byteOffset;
        public int ByteLength => byteLength;

        public Mode GetMode() {
            if (m_ModeEnum != Mode.Undefined) {
                return m_ModeEnum;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (!Enum.TryParse(mode, true, out m_ModeEnum))
            {
                m_ModeEnum = Mode.Undefined;
            }

            mode = null;
#pragma warning restore CS0618 // Type or member is obsolete
            return m_ModeEnum;
        }

        public Filter GetFilter() {
            if (m_FilterEnum != Filter.Undefined) {
                return m_FilterEnum;
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (!Enum.TryParse(filter, true, out m_FilterEnum))
            {
                m_FilterEnum = Filter.None;
            }

            filter = null;
#pragma warning restore CS0618 // Type or member is obsolete
            return m_FilterEnum;
        }
    }
}

#endif
