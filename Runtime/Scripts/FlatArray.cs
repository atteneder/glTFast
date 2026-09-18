// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.Cloud.Gltfast
{
    class FlatArray<T>
    {
        readonly T[] m_Array;
        readonly int[] m_Indices;

        public int Length => m_Array.Length;

        public T this[int key] => m_Array[key];

        public FlatArray(int[] indices)
        {
            Assert.AreEqual(0, indices[0]);
            m_Indices = indices;
            var totalCapacity = indices[indices.Length - 1];
            m_Array = new T[totalCapacity];
        }

        public int GetLength(int primaryIndex)
        {
            Assert.IsTrue(primaryIndex >= 0);
            Assert.IsTrue(primaryIndex < m_Indices.Length - 1);
            var index = m_Indices[primaryIndex];
            var end = m_Indices[primaryIndex + 1];
            return end - index;
        }

        public T GetValue(int primaryIndex, int secondaryIndex)
        {
            var index = GetIndex(primaryIndex, secondaryIndex);
            return m_Array[index];
        }

        public void SetValue(int primaryIndex, int secondaryIndex, T value)
        {
            var index = GetIndex(primaryIndex, secondaryIndex);
            m_Array[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int GetIndex(int primaryIndex, int secondaryIndex)
        {
            Assert.IsTrue(primaryIndex >= 0);
            Assert.IsTrue(secondaryIndex >= 0);
            Assert.IsTrue(primaryIndex < m_Indices.Length - 1);
            var index = m_Indices[primaryIndex];
            var end = m_Indices[primaryIndex + 1];
            Assert.IsTrue(secondaryIndex < end - index);
            return index + secondaryIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void GetIndexRange(int primaryIndex, out int start, out int end)
        {
            Assert.IsTrue(primaryIndex >= 0);
            Assert.IsTrue(primaryIndex < m_Indices.Length - 1);
            start = m_Indices[primaryIndex];
            end = m_Indices[primaryIndex + 1];
        }

        public IEnumerable<T> Values(int primaryIndex)
        {
            GetIndexRange(primaryIndex, out var start, out var end);
            for (var i = start; i < end; i++)
            {
                yield return m_Array[i];
            }
        }

    }
}
