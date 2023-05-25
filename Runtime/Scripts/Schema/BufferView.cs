// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if MESHOPT
using Meshoptimizer;
#endif

namespace GLTFast.Schema
{

    // public enum BufferViewTarget
    // {
    //     None = 0,
    //     ArrayBuffer = 34962,
    //     ElementArrayBuffer = 34963,
    // }

    /// <summary>
    /// Base class; Consists of a byte size and offset
    /// </summary>
    [System.Serializable]
    public abstract class BufferSlice
    {
        /// <summary>
        /// The offset into the buffer in bytes.
        /// <minimum>0</minimum>
        /// </summary>
        public int byteOffset;

        /// <summary>
        /// The length of the bufferView in bytes.
        /// <minimum>0</minimum>
        /// </summary>
        public int byteLength;
    }

    /// <summary>
    /// Adds buffer index and byte stride to <seealso cref="BufferSlice"/>
    /// </summary>
    [System.Serializable]
    public class BufferViewBase : BufferSlice
    {
        /// <summary>
        /// The index of the buffer.
        /// </summary>
        public int buffer;

        /// <summary>
        /// The stride, in bytes, between vertex attributes or other interleaved data.
        /// When this is zero, data is tightly packed.
        /// <minimum>0</minimum>
        /// <maximum>255</maximum>
        /// </summary>
        public int byteStride = -1;
    }

    /// <summary>
    /// A view into a buffer generally representing a subset of the buffer.
    /// </summary>
    [System.Serializable]
    public class BufferView : BufferViewBase
    {
        /// <summary>
        /// The target that the WebGL buffer should be bound to.
        /// All valid values correspond to WebGL enums.
        /// When this is not provided, the bufferView contains animation or skin data.
        /// </summary>
        public int target;

        internal void GltfSerialize(JsonWriter writer)
        {
            writer.AddObject();
            writer.AddProperty("buffer", buffer);
            writer.AddProperty("byteLength", byteLength);
            if (byteOffset > 0)
            {
                writer.AddProperty("byteOffset", byteOffset);
            }
            if (byteStride > 0)
            {
                writer.AddProperty("byteStride", byteStride);
            }
            if (target > 0)
            {
                writer.AddProperty("target", target);
            }
            writer.Close();
        }

#if MESHOPT
        public BufferViewExtensions extensions;
#endif
    }

#if MESHOPT
    [System.Serializable]
    public class BufferViewExtensions {
        // ReSharper disable InconsistentNaming
        public BufferViewMeshoptExtension EXT_meshopt_compression;
        // ReSharper restore InconsistentNaming
    }

    [System.Serializable]
    public class BufferViewMeshoptExtension : BufferViewBase {

        public int count;
        public string mode;
        public string filter;

        Mode m_ModeEnum = Mode.Undefined;
        Filter m_FilterEnum = Filter.Undefined;

        public Mode GetMode() {
            if (m_ModeEnum != Mode.Undefined) {
                return m_ModeEnum;
            }

            if (!string.IsNullOrEmpty(mode)) {
                m_ModeEnum = (Mode)System.Enum.Parse(typeof(Mode), mode, true);
                mode = null;
                return m_ModeEnum;
            }

            return Mode.Undefined;
        }

        public Filter GetFilter() {
            if (m_FilterEnum != Filter.Undefined) {
                return m_FilterEnum;
            }

            if (!string.IsNullOrEmpty(filter)) {
                m_FilterEnum = (Filter)System.Enum.Parse(typeof(Filter), filter, true);
                filter = null;
                return m_FilterEnum;
            }

            return Filter.None;
        }
    }
#endif
}
