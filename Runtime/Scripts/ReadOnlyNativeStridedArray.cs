// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting.APIUpdating;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Read-only view into a buffer whose elements are spaced by a byte stride, as glTF's
    /// interleaved vertex data is.
    /// </summary>
    /// <remarks>
    /// Instances are obtained from the import API and stay valid for as long as the memory they
    /// view does. They can be used as fields in Burst compiled jobs.
    /// </remarks>
    /// <typeparam name="T">Element type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    [NativeContainerIsReadOnly]
    [DebuggerDisplay("Length = {m_Count}")]
    public unsafe struct ReadOnlyNativeStridedArray<T> where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        void* m_Buffer;
        readonly int m_Count;
        readonly int m_ByteStride;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;

        internal ReadOnlyNativeStridedArray(
            void* buffer,
            int byteLength,
            int offset,
            int count,
            int byteStride,
            ref AtomicSafetyHandle safety
            )
        {
            CheckConstructorArguments(byteLength, offset, count, byteStride);
            m_Buffer = (byte*)buffer + offset;
            m_Count = count;
            m_ByteStride = byteStride;
            m_Safety = safety;
        }
#else
        internal ReadOnlyNativeStridedArray(
            void* buffer,
            int byteLength,
            int offset,
            int count,
            int byteStride
            )
        {
            m_Buffer = (byte*)buffer + offset;
            m_Count = count;
            m_ByteStride = byteStride;
        }
#endif

        /// <summary>
        /// Number of elements.
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Count;
        }

        /// <summary>
        /// Byte distance between the starts of two consecutive elements.
        /// </summary>
        public int ByteStride
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_ByteStride;
        }

        /// <summary>
        /// True if this instance views memory. False for a default-initialized instance.
        /// </summary>
        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Buffer != null;
        }

        /// <summary>
        /// Reads the element at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Zero-based element index.</param>
        /// <value>The element at <paramref name="index"/>.</value>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/> is negative or not less than <see cref="Length"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">The viewed memory was released.</exception>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckReadIndex(index);
                return UnsafeUtility.ReadArrayElementWithStride<T>(m_Buffer, index, m_ByteStride);
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        static void CheckConstructorArguments(int byteLength, int offset, int count, int byteStride)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "offset must be >= 0");
            if (offset + (count - 1) * byteStride + sizeof(T) > byteLength)
                throw new ArgumentOutOfRangeException(nameof(count), $"accessor range is outside the range of the native array 0-{(object)(byteLength - 1)}");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckReadIndex(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            if (index < 0 || index >= m_Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range (must be between 0 and {m_Count - 1}).");
        }
    }
}
