// SPDX-FileCopyrightText: 2025 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Collections;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Provides read-only access to byte data and the ability to dispose the underlying resources
    /// when not required anymore.
    /// </summary>
    interface IReadOnlyDisposableData : IDisposable
    {
        /// <summary>
        /// Read-only byte data.
        /// </summary>
        public NativeArray<byte>.ReadOnly Data { get; }
    }

    struct ReadOnlyData : IReadOnlyDisposableData
    {
        public NativeArray<byte>.ReadOnly Data { get; }
        IDisposable m_Disposable;

        public ReadOnlyData(NativeArray<byte>.ReadOnly data, IDisposable disposable)
        {
            Data = data;
            m_Disposable = disposable;
        }

        public void Dispose()
        {
            m_Disposable?.Dispose();
        }
    }

    struct ReadOnlyDisposableData : IReadOnlyDisposableData
    {
        public NativeArray<byte>.ReadOnly Data => m_Data.AsReadOnly();
        NativeArray<byte> m_Data;

        public ReadOnlyDisposableData(NativeArray<byte> data)
        {
            m_Data = data;
        }

        public void Dispose()
        {
            m_Data.Dispose();
        }
    }
}
