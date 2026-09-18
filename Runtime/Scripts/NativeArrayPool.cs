// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_ANIMATION && UNITY_6000_2_OR_NEWER

using System;
using Unity.Collections;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Manages a pool of re-usable <see cref="NativeArray{T}"/> arrays.
    /// Arrays are temporary and have to get disposed within the same frame.
    /// Do not use any one of those arrays concurrently!
    /// </summary>
    /// <typeparam name="T">Member type.</typeparam>
    sealed class NativeArrayPool<T> : IDisposable where T : struct
    {
        readonly NativeArray<T>[] m_Buffers;

        public NativeArrayPool(int maxDimensions)
        {
            m_Buffers = new NativeArray<T>[maxDimensions];
        }

        public ref NativeArray<T> GetBuffer(int dimension)
        {
            Assert.IsTrue(dimension < m_Buffers.Length);
            return ref m_Buffers[dimension];
        }

        public void ReserveBuffers(int length, int dimensions)
        {
            Assert.IsTrue(dimensions <= m_Buffers.Length);
            Profiler.BeginSample("NativeArrayPool.ReserveBuffers");
            for (var dimension = 0; dimension < dimensions; dimension++)
            {
                if (!m_Buffers[dimension].IsCreated)
                {
                    m_Buffers[dimension] = new NativeArray<T>(
                        length,
                        Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory
                    );
                    continue;
                }

                if (m_Buffers[dimension].Length < length)
                {
                    m_Buffers[dimension].Dispose();
                    m_Buffers[dimension] = new NativeArray<T>(
                        length,
                        Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory
                    );
                }
            }
            Profiler.EndSample();
        }

        public void Dispose()
        {
            for (var i = 0; i < m_Buffers.Length; i++)
            {
                if (m_Buffers[i].IsCreated)
                {
                    m_Buffers[i].Dispose();
                }
                m_Buffers[i] = default;
            }
        }
    }
}
#endif // UNITY_ANIMATION
