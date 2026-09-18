// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Wraps a <see cref="NativeArray{T}.ReadOnly"/> and provides a <see cref="ReadOnlyNativeArray{T}"/> for accessing it.
    /// </summary>
    readonly unsafe struct ReadOnlyNativeArrayFromNativeArray<T> where T : unmanaged
    {
        readonly ReadOnlyNativeArray<T> m_Array;
        public ReadOnlyNativeArray<T> Array
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                // Making sure the source was not disposed already.
                // This indirectly triggers a check of the original's safety handle as in
                // `AtomicSafetyHandle.CheckReadAndThrow(m_Source.m_Safety);`
                m_Source.AsReadOnlySpan();
#endif
                return m_Array;
            }
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        readonly NativeArray<T>.ReadOnly m_Source;
#endif

        public ReadOnlyNativeArrayFromNativeArray(NativeArray<T>.ReadOnly data)
        {
            var bufferAddress = data.GetUnsafeReadOnlyPtr();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Source = data;
            var safety = AtomicSafetyHandle.Create();
            m_Array = new ReadOnlyNativeArray<T>(bufferAddress, data.Length, ref safety);
#else
            m_Array = new ReadOnlyNativeArray<T>(bufferAddress, data.Length);
#endif
        }
    }
}
