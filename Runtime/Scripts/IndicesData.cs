// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;

namespace Unity.Cloud.Gltfast
{
    struct IndicesData : IDisposable
    {
        const Allocator k_Allocator = Allocator.Persistent;

        public readonly IndexFormat IndexFormat;
        public readonly int SubMeshCount => m_Indices.Length;
        NativeArray<ushort>[] m_Indices;

        public IndicesData(IndexFormat indexFormat, int count)
        {
            IndexFormat = indexFormat;
            m_Indices = new NativeArray<ushort>[count];
        }

        public void Allocate(int submesh, int length)
        {
            m_Indices[submesh] = IndexFormat switch
            {
                IndexFormat.UInt16 =>
                    new NativeArray<ushort>(length, k_Allocator, NativeArrayOptions.UninitializedMemory),
                IndexFormat.UInt32 =>
                    new NativeArray<uint>(length, k_Allocator, NativeArrayOptions.UninitializedMemory)
                        .Reinterpret<ushort>(UnsafeUtility.SizeOf<uint>()),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public int GetTotalIndexCount()
        {
            var total = 0;
            foreach (var indices in m_Indices)
            {
                total += indices.Length;
            }
            return IndexFormat == IndexFormat.UInt16
                ? total
                : total / 2;
        }

        public NativeArray<ushort> GetIndices16(int index)
        {
            return m_Indices[index];
        }

        public NativeArray<uint> GetIndices32(int index)
        {
            return m_Indices[index].Reinterpret<uint>(UnsafeUtility.SizeOf<ushort>());
        }

        public void Dispose()
        {
            if (m_Indices != null)
            {
                foreach (var indices in m_Indices)
                {
                    if (indices.IsCreated)
                    {
                        indices.Dispose();
                    }
                }
                m_Indices = null;
            }
        }
    }
}
