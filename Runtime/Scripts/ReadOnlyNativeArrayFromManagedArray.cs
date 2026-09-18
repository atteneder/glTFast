// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Wraps a managed array and provides a <see cref="ReadOnlyNativeArray{T}"/> for accessing it.
    /// </summary>
    sealed class ReadOnlyNativeArrayFromManagedArray<T> : IDisposable
        where T : unmanaged
    {
        public ReadOnlyNativeArray<T> Array { get; }

        GCHandle m_BufferHandle;
        bool m_Pinned;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;
#endif

        public unsafe ReadOnlyNativeArrayFromManagedArray(T[] original)
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));

            m_BufferHandle = GCHandle.Alloc(original, GCHandleType.Pinned);
            fixed (void* bufferAddress = &original[0])
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                m_Safety = AtomicSafetyHandle.Create();
                Array = new ReadOnlyNativeArray<T>(bufferAddress, original.Length, ref m_Safety);
#else
                Array = new ReadOnlyNativeArray<T>(bufferAddress, original.Length);
#endif
            }

            m_Pinned = true;
        }

        /// <summary>
        /// Releases the pin on the managed array and invalidates every
        /// <see cref="ReadOnlyNativeArray{T}" /> derived from it. Repeated calls are a no-op.
        /// </summary>
        public void Dispose()
        {
            if (!m_Pinned)
            {
                return;
            }
            m_Pinned = false;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckDeallocateAndThrow(m_Safety);
            AtomicSafetyHandle.Release(m_Safety);
#endif
            m_BufferHandle.Free();
        }
    }
}
