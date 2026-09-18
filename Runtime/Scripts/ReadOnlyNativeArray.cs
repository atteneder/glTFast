// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// This is a stripped-down version of <see cref="NativeArray{T}.ReadOnly"/> that supports <see cref="GetSubArray(int,int)"/>.
    /// </summary>
    /// <typeparam name="T">Member type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    [NativeContainerIsReadOnly]
    [DebuggerDisplay("Length = {Length}")]
    unsafe struct ReadOnlyNativeArray<T> where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        internal void* m_Buffer;
        internal int m_Length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal AtomicSafetyHandle m_Safety;
#endif

        internal ReadOnlyNativeArray(NativeArray<T> nativeArray)
        {
            m_Buffer = nativeArray.GetUnsafeReadOnlyPtr();
            m_Length = nativeArray.Length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(nativeArray);
#endif
        }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        internal ReadOnlyNativeArray(void* buffer, int length, ref AtomicSafetyHandle safety)
        {
            m_Buffer = buffer;
            m_Length = length;
            m_Safety = safety;
        }
#else
        internal ReadOnlyNativeArray(void* buffer, int length)
        {
            m_Buffer = buffer;
            m_Length = length;
        }

#endif

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Length;
        }

        public ReadOnlyNativeArray<T> GetSubArray(int start, int length)
        {
            CheckGetSubArrayArguments(start, length);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            return new ReadOnlyNativeArray<T>(
                ((byte*)m_Buffer) + ((long)UnsafeUtility.SizeOf<T>()) * start, length, ref m_Safety);
#else
            return new ReadOnlyNativeArray<T>(
                ((byte*)m_Buffer) + ((long)UnsafeUtility.SizeOf<T>()) * start, length);
#endif
        }

        public NativeSlice<T> ToSlice()
        {
            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(m_Buffer, m_Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, m_Safety);
#endif
            return array.Slice();
        }

        public NativeArray<T>.ReadOnly AsNativeArrayReadOnly()
        {
            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(m_Buffer, m_Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, m_Safety);
#endif
            return array.AsReadOnly();
        }

        public ReadOnlyNativeStridedArray<TTarget> ToStrided<TTarget>(
            int offset,
            int count,
            int byteStride
            )
            where TTarget : unmanaged
        {
            return new ReadOnlyNativeStridedArray<TTarget>(
                m_Buffer,
                Length * UnsafeUtility.SizeOf<T>(),
                offset,
                count,
                byteStride
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                , ref m_Safety
#endif
                );
        }

        public void* GetUnsafeReadOnlyPtr()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            return m_Buffer;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckGetSubArrayArguments(int start, int length)
        {
            if (start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "start must be >= 0");
            }

            if (start + length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"sub array range {start}-{start + length - 1} is outside the range of the native array 0-{Length - 1}");
            }

            if (start + length < 0)
            {
                throw new ArgumentException($"sub array range {start}-{start + length - 1} caused an integer overflow and is outside the range of the native array 0-{Length - 1}");
            }
        }

        public void CopyTo(NativeArray<T> array) => Copy(this, array);

        public ReadOnlyNativeArray<TTarget> Reinterpret<TTarget>() where TTarget : unmanaged
        {
            long tSize = UnsafeUtility.SizeOf<T>();
            long uSize = UnsafeUtility.SizeOf<TTarget>();

            var byteLen = Length * tSize;
            var uLen = byteLen / uSize;

            CheckReinterpretSize<TTarget>(uSize, byteLen, uLen);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            return new ReadOnlyNativeArray<TTarget>(m_Buffer, (int)uLen, ref m_Safety);
#else
            return new ReadOnlyNativeArray<TTarget>(m_Buffer, (int)uLen);
#endif
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        void CheckReinterpretSize<TTarget>(long uSize, long byteLen, long uLen)
        {
            if (uLen * uSize != byteLen)
            {
                throw new InvalidOperationException($"Types {typeof(T)} (array length {Length}) and {typeof(TTarget)} cannot be aliased due to size constraints. The size of the types and lengths involved must line up.");
            }
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckElementReadAccess(index);
                return UnsafeUtility.ReadArrayElement<T>(m_Buffer, index);
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CheckElementReadAccess(int index)
        {
            if ((uint)index >= (uint)m_Length)
            {
                throw new IndexOutOfRangeException($"Index {index} is out of range (must be between 0 and {m_Length - 1}).");
            }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
        }

        public bool IsCreated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Buffer != null;
        }

        static void Copy(ReadOnlyNativeArray<T> src, NativeArray<T> dst)
        {
            CheckCopyLengths(src.Length, dst.Length);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(src.m_Safety);
            var dstSafetyHandle = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(dst);
            AtomicSafetyHandle.CheckWriteAndThrow(dstSafetyHandle);
#endif
            var dstPointer = (byte*)dst.GetUnsafePtr();
            UnsafeUtility.MemCpy(
                dstPointer,
                (byte*)src.m_Buffer,
                src.Length * UnsafeUtility.SizeOf<T>());
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        static void CheckCopyLengths(int srcLength, int dstLength)
        {
            if (srcLength != dstLength)
                throw new ArgumentException("source and destination length must be the same");
        }
    }
}
