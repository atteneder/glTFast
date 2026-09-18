// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using Unity.Collections;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Read access to a glTF asset's buffer data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Buffer data comes with a lease: the import keeps its buffers alive for as long as at least
    /// one lease has not been disposed. Dispose it as soon as the data is no longer needed, so the
    /// memory can be released.
    /// </para>
    /// <para>
    /// Disposing the <see cref="GltfImport"/> releases the buffers regardless of outstanding
    /// leases. Reading afterwards throws <see cref="ObjectDisposedException"/>.
    /// </para>
    /// <para>
    /// Data is provided in glTF's own coordinate system and value range. No conversion,
    /// normalization or coordinate flip is applied. Use
    /// <see cref="Objects.Accessor.ComponentType"/>, <see cref="Objects.Accessor.Type"/> and
    /// <see cref="Objects.Accessor.Normalized"/> to determine how to interpret it.
    /// </para>
    /// <para>
    /// Use it from the main thread. The containers it provides may be read from C# jobs and
    /// from threads of your own for as long as the lease has not been disposed.
    /// </para>
    /// </remarks>
    public interface IGltfBufferData : IDisposable
    {
        /// <summary>
        /// Provides a buffer view's raw bytes.
        /// </summary>
        /// <param name="bufferViewIndex">glTF buffer view index.</param>
        /// <param name="data">The buffer view's bytes, if the result is
        /// <see cref="BufferAccessStatus.Success"/>.</param>
        /// <param name="byteStride">The effective byte stride, or null when the data is tightly
        /// packed. For compressed buffer views this is the extension's stride, which can differ
        /// from <see cref="Objects.BufferView.ByteStride"/>.</param>
        /// <returns>The outcome of the request.</returns>
        BufferAccessStatus GetBufferView(
            int bufferViewIndex,
            out NativeArray<byte>.ReadOnly data,
            out int? byteStride
            );

        /// <summary>
        /// Provides an accessor's tightly packed data.
        /// </summary>
        /// <remarks>
        /// Only tightly packed data can be provided as a plain array. An accessor whose buffer view
        /// declares a byte stride differing from the element size holds interleaved data and is
        /// reported as <see cref="BufferAccessStatus.StridedUnsupported"/>; request it via
        /// <see cref="GetStridedAccessorData{T}"/> instead.
        /// </remarks>
        /// <param name="accessorIndex">glTF accessor index.</param>
        /// <param name="data">The accessor's elements, if the result is
        /// <see cref="BufferAccessStatus.Success"/>.</param>
        /// <typeparam name="T">Element type. Its size has to match the accessor's
        /// <see cref="Objects.Accessor.ElementByteSize"/>.</typeparam>
        /// <returns>The outcome of the request.</returns>
        BufferAccessStatus GetAccessorData<T>(int accessorIndex, out NativeArray<T>.ReadOnly data)
            where T : unmanaged;

        /// <summary>
        /// Provides an accessor's data as a strided view, which is how interleaved vertex data is
        /// laid out.
        /// </summary>
        /// <param name="accessorIndex">glTF accessor index.</param>
        /// <param name="data">The accessor's elements, if the result is
        /// <see cref="BufferAccessStatus.Success"/>.</param>
        /// <typeparam name="T">Element type. Its size has to match the accessor's
        /// <see cref="Objects.Accessor.ElementByteSize"/>.</typeparam>
        /// <returns>The outcome of the request.</returns>
        BufferAccessStatus GetStridedAccessorData<T>(
            int accessorIndex,
            out ReadOnlyNativeStridedArray<T> data
            )
            where T : unmanaged;
    }
}
