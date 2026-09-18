// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Buffers;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Cloud.Gltfast
{
    sealed unsafe class NativeMemoryManager<T> : MemoryManager<T> where T : unmanaged
    {
        NativeArray<T> m_Array;

        public NativeMemoryManager(NativeArray<T> source)
        {
            m_Array = source;
        }

        public override Span<T> GetSpan() => m_Array.AsSpan();

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            if (elementIndex < 0 || elementIndex >= m_Array.Length)
                throw new ArgumentOutOfRangeException(nameof(elementIndex));
            return new MemoryHandle(m_Array.GetUnsafeReadOnlyPtr());
        }

        public override void Unpin() { }

        protected override void Dispose(bool disposing) { }
    }
}
