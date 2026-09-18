// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using Unity.Collections;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Hands out buffer data and holds a lease on it for as long as it is not disposed.
    /// </summary>
    sealed class BufferDataLease : IGltfBufferData
    {
        BufferStore m_Store;

        internal BufferDataLease(BufferStore store)
        {
            m_Store = store;
        }

        public BufferAccessStatus GetBufferView(
            int bufferViewIndex,
            out NativeArray<byte>.ReadOnly data,
            out int? byteStride
            )
        {
            if (m_Store == null)
            {
                data = default;
                byteStride = null;
                return BufferAccessStatus.BufferUnavailable;
            }
            return m_Store.ReadBufferView(bufferViewIndex, out data, out byteStride);
        }

        public BufferAccessStatus GetAccessorData<T>(int accessorIndex, out NativeArray<T>.ReadOnly data)
            where T : unmanaged
        {
            if (m_Store == null)
            {
                data = default;
                return BufferAccessStatus.BufferUnavailable;
            }
            return m_Store.ReadAccessorData(accessorIndex, out data);
        }

        public BufferAccessStatus GetStridedAccessorData<T>(
            int accessorIndex,
            out ReadOnlyNativeStridedArray<T> data
            )
            where T : unmanaged
        {
            if (m_Store == null)
            {
                data = default;
                return BufferAccessStatus.BufferUnavailable;
            }
            return m_Store.ReadStridedAccessorData(accessorIndex, out data);
        }

        public void Dispose()
        {
            var store = m_Store;
            m_Store = null;
            store?.ReleaseLease();
        }
    }
}
