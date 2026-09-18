// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if MESHOPT_IS_RECENT
#define MESHOPT_IS_ENABLED
#endif

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Unity.Cloud.Gltfast.Logging;
using Unity.Cloud.Gltfast.Objects;
#if MESHOPT_IS_ENABLED
using Meshoptimizer;
#endif
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using Buffer = Unity.Cloud.Gltfast.Objects.Buffer;

namespace Unity.Cloud.Gltfast
{
    /// <summary>
    /// Owns a glTF asset's raw buffer memory and resolves buffer views and accessors into it.
    /// </summary>
    /// <remarks>
    /// Buffer memory is not owned directly: <see cref="m_Buffers"/> holds non-owning views. The
    /// objects that own the memory (downloads, pinned managed arrays, <see cref="UriValue"/>s) are
    /// handed to the tracking callback and disposed by the owner of this store.
    /// </remarks>
    sealed class BufferStore : IDisposable
    {
        readonly ImportContext m_Context;
        readonly Action<IDisposable> m_TrackDisposable;
        readonly Action m_ReleaseMemoryOwners;

        int m_LeaseCount;
        bool m_DisposeRequested;

        Root m_Root;
        Uri m_BaseUri;

        ReadOnlyNativeArray<byte>[] m_Buffers;
        GlbBinChunk[] m_BinChunks;
        Dictionary<int, Task<bool>> m_BufferLoadTasks;

        /// optional glTF-binary buffer
        /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#binary-buffer
        GlbBinChunk? m_GlbBinChunk;

#if MESHOPT_IS_ENABLED
        Dictionary<int, NativeArray<byte>> m_MeshoptBufferViews;
        NativeArray<int> m_MeshoptReturnValues;
        JobHandle m_MeshoptJobHandle;
#endif

        ICodeLogger Logger => m_Context.Logger;

        public BufferStore(
            ImportContext context,
            Action<IDisposable> trackDisposable,
            Action releaseMemoryOwners
            )
        {
            m_Context = context;
            m_TrackDisposable = trackDisposable;
            m_ReleaseMemoryOwners = releaseMemoryOwners;
        }

        /// <summary>
        /// Number of leases that have not been disposed yet.
        /// </summary>
        public int LeaseCount => m_LeaseCount;

        /// <summary>
        /// Creates a lease and holds the buffer memory until it is disposed.
        /// </summary>
        public IGltfBufferData AcquireLease()
        {
            m_LeaseCount++;
            return new BufferDataLease(this);
        }

        internal void ReleaseLease()
        {
            m_LeaseCount--;
            if (m_LeaseCount <= 0 && m_DisposeRequested)
            {
                ReleaseMemory();
            }
        }

        /// <summary>
        /// Supplies the de-serialized document. Not a constructor parameter because the root is
        /// assigned during JSON parsing, which also starts the buffer loads.
        /// </summary>
        public void Initialize(Root root, Uri baseUri)
        {
            m_Root = root;
            m_BaseUri = baseUri;
        }

        public bool HasGlbBinChunk => m_GlbBinChunk.HasValue;

        public void SetGlbBinChunk(GlbBinChunk chunk)
        {
            Assert.IsFalse(m_GlbBinChunk.HasValue); // There can only be one binary chunk
            m_GlbBinChunk = chunk;
        }

        /// <summary>
        /// Points buffer 0 at the glTF-binary buffer chunk within the document's own memory.
        /// </summary>
        public void AssignGlbBinChunk(NativeArray<byte>.ReadOnly bytes)
        {
            if (!m_GlbBinChunk.HasValue || m_BinChunks == null)
            {
                return;
            }
            m_BinChunks[0] = m_GlbBinChunk.Value;
            var wrapper = new ReadOnlyNativeArrayFromNativeArray<byte>(bytes);
            m_Buffers[0] = wrapper.Array;
        }

        /// <summary>
        /// Allocates buffer storage and starts loading every buffer that has a URI.
        /// </summary>
        public void StartBufferLoads(CancellationToken cancellationToken)
        {
            if (m_Root.Buffers == null)
            {
                return;
            }

            var bufferCount = m_Root.Buffers.Count;
            if (bufferCount > 0)
            {
                m_Buffers = new ReadOnlyNativeArray<byte>[bufferCount];
                m_BinChunks = new GlbBinChunk[bufferCount];
            }

            for (var i = 0; i < bufferCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequestedWithTracking();

                var buffer = m_Root.Buffers[i];
                if (buffer.Uri != null)
                {
                    m_BufferLoadTasks ??= new Dictionary<int, Task<bool>>();
                    if (buffer.Uri.IsData || buffer.Uri.IsFailed)
                    {
                        Logger?.Warning(LogCode.EmbedSlow);
                        m_BufferLoadTasks[i] = LoadBufferFromDataUri(i, buffer, cancellationToken);
                    }
                    else
                    {
                        m_BufferLoadTasks[i] = LoadBufferFromUriAsync(
                            i, UriHelper.GetUriString(buffer.Uri.AsString(), m_BaseUri));
                    }
                }
            }
        }

        public async Task<bool> WaitForBufferDownloads(CancellationToken cancellationToken)
        {
            if (m_BufferLoadTasks != null)
            {
                foreach (var loadTaskPair in m_BufferLoadTasks)
                {
                    cancellationToken.ThrowIfCancellationRequestedWithTracking();
                    if (!await loadTaskPair.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

#pragma warning disable CS1998 // async method lacking 'await' is intentional: this stays a Task to match LoadBufferFromUriAsync.
        async Task<bool> LoadBufferFromDataUri(int bufferIndex, Buffer buffer, CancellationToken cancellationToken)
#pragma warning restore CS1998
        {
            cancellationToken.ThrowIfCancellationRequestedWithTracking();

            if (buffer.Uri.IsFailed)
            {
                Logger?.Error(LogCode.EmbedBufferLoadFailed);
                return false;
            }

            var mimeType = buffer.Uri.MimeType;
            if (!mimeType.StartsWith("application/", StringComparison.Ordinal)
                || !(
                    mimeType.AsSpan(12).SequenceEqual("octet-stream")
                    || mimeType.AsSpan(12).SequenceEqual("gltf-buffer")
                    )
                )
            {
                Logger?.Error(
                    LogCode.BufferDataUriUnexpectedMimeType,
                    bufferIndex.ToString(),
                    mimeType
                    );
                return false;
            }

            if (!buffer.Uri.TryGetData(out var data))
            {
                Logger?.Error(LogCode.EmbedBufferLoadFailed);
                return false;
            }

            if (data.Length < buffer.ByteLength)
            {
                Logger?.Error(
                    LogCode.BufferContentUndersized,
                    bufferIndex.ToString(),
                    buffer.ByteLength.ToString(),
                    data.Length.ToString()
                    );
                return false;
            }

            // The UriValue (tracked by the owner, which drained the converter's pending list)
            // retains ownership of the NativeArray.
            m_Buffers[bufferIndex] = new ReadOnlyNativeArray<byte>(data);
            if (bufferIndex != 0 || !m_GlbBinChunk.HasValue)
            {
                m_BinChunks[bufferIndex] = new GlbBinChunk(0, (uint)m_Buffers[bufferIndex].Length);
            }
            return true;
        }

        async Task<bool> LoadBufferFromUriAsync(int index, Uri uri)
        {
            var request = m_Context.DownloadProvider.RequestAsync(uri);
            var download = await request;
            if (download.Success)
            {
                Profiler.BeginSample("GetData");

                var wrapper = new ReadOnlyNativeArrayFromNativeArray<byte>(download.Data);
                m_Buffers[index] = wrapper.Array;

                m_TrackDisposable(download);

                Profiler.EndSample();

                if (index != 0 || !m_GlbBinChunk.HasValue)
                {
                    m_BinChunks[index] = new GlbBinChunk(0, (uint)m_Buffers[index].Length);
                }

                return true;
            }

            Logger?.Error(LogCode.BufferLoadFailed, download.Error, index.ToString());
            return false;
        }

        /// <summary>
        /// Provides a glTF accessor.
        /// </summary>
        /// <param name="index">glTF accessor index</param>
        /// <returns>The accessor, or null if the index does not address one.</returns>
        public Accessor GetAccessor(int index)
        {
            return GltfIndex.TryGetElement(m_Root.Accessors, index, out var accessor) ? accessor : null;
        }

        /// <summary>
        /// Get glTF accessor and its raw data
        /// </summary>
        /// <param name="index">glTF accessor index</param>
        /// <param name="accessor">De-serialized glTF accessor</param>
        /// <param name="data">Pointer to accessor's data in memory</param>
        /// <param name="byteStride">Element byte stride</param>
        public unsafe void GetAccessorAndData(
            int index, out Accessor accessor, out void* data, out int? byteStride)
        {
            if (!GltfIndex.TryGetElement(m_Root.Accessors, index, out accessor)
                || !GltfIndex.TryGetIndex(accessor.BufferView, m_Root.BufferViews?.Count ?? 0, out var bufferViewIndex))
            {
                data = null;
                byteStride = 0;
                return;
            }
            TryGetBufferViewPointer(bufferViewIndex, accessor.ByteOffset, out data, out byteStride);
        }

        /// <summary>
        /// Get sparse indices raw data
        /// </summary>
        /// <param name="sparseIndices">glTF sparse indices accessor</param>
        /// <param name="data">Pointer to accessor's data in memory</param>
        public unsafe void GetAccessorSparseIndices(AccessorSparseIndices sparseIndices, out void* data)
        {
            if (!GltfIndex.TryGetIndex(sparseIndices.BufferView, m_Root.BufferViews?.Count ?? 0, out var bufferViewIndex))
            {
                Logger?.Error(
                    sparseIndices.BufferView.HasValue ? LogCode.IndexOutOfRange : LogCode.RequiredPropertyMissing,
                    "accessor.sparse.indices.bufferView",
                    GltfIndex.Describe(sparseIndices.BufferView));
                data = null;
                return;
            }
            TryGetBufferViewPointer(bufferViewIndex, sparseIndices.ByteOffset, out data, out _);
        }

        /// <summary>
        /// Get sparse value raw data
        /// </summary>
        /// <param name="sparseValues">glTF sparse values accessor</param>
        /// <param name="data">Pointer to accessor's data in memory</param>
        public unsafe void GetAccessorSparseValues(AccessorSparseValues sparseValues, out void* data)
        {
            if (!GltfIndex.TryGetIndex(sparseValues.BufferView, m_Root.BufferViews?.Count ?? 0, out var bufferViewIndex))
            {
                Logger?.Error(
                    sparseValues.BufferView.HasValue ? LogCode.IndexOutOfRange : LogCode.RequiredPropertyMissing,
                    "accessor.sparse.values.bufferView",
                    GltfIndex.Describe(sparseValues.BufferView));
                data = null;
                return;
            }
            TryGetBufferViewPointer(bufferViewIndex, sparseValues.ByteOffset, out data, out _);
        }

        public ReadOnlyNativeArray<byte> GetBuffer(int index)
        {
            return m_Buffers[index];
        }

        public BufferAccessStatus TryGetBufferView(
            int bufferViewIndex,
            out ReadOnlyNativeArray<byte> data,
            out int? byteStride,
            int offset = 0,
            int length = 0
            )
        {
            data = default;
            byteStride = null;
            if (!TryGetBufferViewObject(bufferViewIndex, out var bufferView))
            {
                return BufferAccessStatus.ObjectIndexOutOfRange;
            }
#if MESHOPT_IS_ENABLED
            if (bufferView.Extensions?.ExtMeshoptCompression != null)
            {
                byteStride = bufferView.Extensions.ExtMeshoptCompression.ByteStride;
                if (!TryGetMeshoptBufferView(bufferViewIndex, out var entireBuffer))
                {
                    return BufferAccessStatus.BufferUnavailable;
                }
                if (offset == 0 && length <= 0)
                {
                    data = new ReadOnlyNativeArray<byte>(entireBuffer);
                    return BufferAccessStatus.Success;
                }
                if (offset < 0)
                {
                    return BufferAccessStatus.DataIndexOutOfRange;
                }
                var meshoptByteLength = (long)length;
                if (meshoptByteLength <= 0)
                {
                    meshoptByteLength = entireBuffer.Length - (long)offset;
                }
                if (meshoptByteLength < 0 || offset + meshoptByteLength > entireBuffer.Length)
                {
                    return BufferAccessStatus.DataIndexOutOfRange;
                }
                data = new ReadOnlyNativeArray<byte>(entireBuffer.GetSubArray(offset, (int)meshoptByteLength));
                return BufferAccessStatus.Success;
            }
#endif
            byteStride = bufferView.ByteStride;
            return TryGetBufferView(bufferView, out data, offset, length);
        }

        public BufferAccessStatus TryGetBufferView(
            IBufferView bufferView,
            out ReadOnlyNativeArray<byte> data,
            int offset = 0,
            int length = 0
        )
        {
            data = default;
            if (offset < 0)
            {
                return BufferAccessStatus.DataIndexOutOfRange;
            }
            var byteLength = (long)length;
            if (byteLength <= 0)
            {
                byteLength = bufferView.ByteLength - (long)offset;
            }
            if (byteLength < 0 || offset + byteLength > bufferView.ByteLength)
            {
                return BufferAccessStatus.DataIndexOutOfRange;
            }
            var status = TryResolveRange(bufferView, offset, byteLength, out var bufferIndex, out var start);
            if (status != BufferAccessStatus.Success)
            {
                return status;
            }
            data = m_Buffers[bufferIndex].GetSubArray(start, (int)byteLength);
            return BufferAccessStatus.Success;
        }

        public BufferAccessStatus TryGetAccessorData<T>(
            int bufferViewIndex,
            int count,
            out ReadOnlyNativeArray<T> data,
            int offset = 0
            ) where T : unmanaged
        {
            data = default;
            if (!TryGetBufferViewObject(bufferViewIndex, out var bufferView))
            {
                return BufferAccessStatus.ObjectIndexOutOfRange;
            }
#if MESHOPT_IS_ENABLED
            var meshopt = bufferView.Extensions?.ExtMeshoptCompression;
            if (meshopt != null)
            {
                if (!TryGetMeshoptBufferView(bufferViewIndex, out var fullSlice))
                {
                    return BufferAccessStatus.BufferUnavailable;
                }
                var meshoptElementByteSize = UnsafeUtility.SizeOf<T>();
                // The decoded buffer is laid out at the extension's stride, so the same tight
                // packing requirement applies as for an uncompressed buffer view.
                var meshoptStride = bufferView.ByteStride ?? meshopt.ByteStride;
                if (meshoptStride.HasValue && meshoptStride.Value != meshoptElementByteSize)
                {
                    return BufferAccessStatus.StridedUnsupported;
                }
                var meshoptByteLength = (long)count * meshoptElementByteSize;
                if (offset == 0 && (count <= 0 || meshoptByteLength == fullSlice.Length))
                {
                    data = new ReadOnlyNativeArray<byte>(fullSlice).Reinterpret<T>();
                    return BufferAccessStatus.Success;
                }

                if (offset < 0
                    || count <= 0
                    || offset + meshoptByteLength > fullSlice.Length)
                {
                    return BufferAccessStatus.DataIndexOutOfRange;
                }
                data = new ReadOnlyNativeArray<byte>(fullSlice)
                    .GetSubArray(offset, (int)meshoptByteLength)
                    .Reinterpret<T>();
                return BufferAccessStatus.Success;
            }
#endif
            return TryGetAccessorData(bufferView, count, out data, offset);
        }

        public BufferAccessStatus TryGetAccessorData<T>(
            IBufferView bufferView,
            int count,
            out ReadOnlyNativeArray<T> data,
            int offset = 0
        ) where T : unmanaged
        {
            data = default;
            if (count < 0)
            {
                return BufferAccessStatus.DataIndexOutOfRange;
            }
            var elementByteSize = UnsafeUtility.SizeOf<T>();
            // A contiguous reinterpret only describes the data when it is tightly packed.
            if (bufferView.ByteStride.HasValue && bufferView.ByteStride.Value != elementByteSize)
            {
                return BufferAccessStatus.StridedUnsupported;
            }
            var byteLength = (long)count * elementByteSize;
            if (offset + byteLength > bufferView.ByteLength)
            {
                return BufferAccessStatus.DataIndexOutOfRange;
            }
            var status = TryResolveRange(bufferView, offset, byteLength, out var bufferIndex, out var start);
            if (status != BufferAccessStatus.Success)
            {
                return status;
            }
            data = m_Buffers[bufferIndex].GetSubArray(start, (int)byteLength).Reinterpret<T>();
            return BufferAccessStatus.Success;
        }

        public BufferAccessStatus TryGetStridedAccessorData<T>(
            int bufferViewIndex,
            int count,
            out ReadOnlyNativeStridedArray<T> data,
            int offset = 0
            ) where T : unmanaged
        {
            data = default;
            if (!TryGetBufferViewObject(bufferViewIndex, out var bufferView))
            {
                return BufferAccessStatus.ObjectIndexOutOfRange;
            }
#if MESHOPT_IS_ENABLED
            var meshopt = bufferView.Extensions?.ExtMeshoptCompression;
            if (meshopt != null)
            {
                if (!TryGetMeshoptBufferView(bufferViewIndex, out var fullSlice))
                {
                    return BufferAccessStatus.BufferUnavailable;
                }
                var elementByteSize = UnsafeUtility.SizeOf<T>();
                // The decoded buffer is laid out at the extension's stride; the parent buffer
                // view's stride is optional for meshopt compressed buffer views.
                var meshoptByteStride = bufferView.ByteStride ?? meshopt.ByteStride ?? elementByteSize;
                if (offset < 0 || count < 0 || meshoptByteStride < elementByteSize)
                {
                    return BufferAccessStatus.DataIndexOutOfRange;
                }
                var meshoptByteLength = count == 0
                    ? 0L
                    : (long)(count - 1) * meshoptByteStride + elementByteSize;
                if (offset + meshoptByteLength > fullSlice.Length)
                {
                    return BufferAccessStatus.DataIndexOutOfRange;
                }
                unsafe
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    var safety = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(fullSlice);
#endif
                    data = new ReadOnlyNativeStridedArray<T>(
                        fullSlice.GetUnsafeReadOnlyPtr(),
                        fullSlice.Length,
                        offset,
                        count,
                        meshoptByteStride
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                        , ref safety
#endif
                        );
                }
                return BufferAccessStatus.Success;
            }
#endif
            return TryGetStridedAccessorData(bufferView, count, out data, offset);
        }

        public BufferAccessStatus TryGetStridedAccessorData<T>(
            IBufferView bufferView,
            int count,
            out ReadOnlyNativeStridedArray<T> data,
            int offset = 0
        ) where T : unmanaged
        {
            data = default;
            if (count < 0)
            {
                return BufferAccessStatus.DataIndexOutOfRange;
            }
            var elementByteSize = UnsafeUtility.SizeOf<T>();
            var byteStride = bufferView.ByteStride ?? elementByteSize;
            if (byteStride < elementByteSize)
            {
                return BufferAccessStatus.DataIndexOutOfRange;
            }
            // The final element occupies elementByteSize bytes rather than a full stride, so a
            // spec conforming buffer view may end before offset + count * byteStride.
            var byteLength = count == 0 ? 0L : (long)(count - 1) * byteStride + elementByteSize;
            if (offset + byteLength > bufferView.ByteLength)
            {
                return BufferAccessStatus.DataIndexOutOfRange;
            }
            var status = TryResolveRange(bufferView, offset, byteLength, out var bufferIndex, out var start);
            if (status != BufferAccessStatus.Success)
            {
                return status;
            }
            data = m_Buffers[bufferIndex].ToStrided<T>(start, count, byteStride);
            return BufferAccessStatus.Success;
        }

        /// <summary>
        /// Resolves a buffer view (meshopt-aware) to a pointer into buffer memory.
        /// </summary>
        public unsafe bool TryGetBufferViewPointer(
            int bufferViewIndex,
            int byteOffset,
            out void* data,
            out int? byteStride
            )
        {
            data = null;
            byteStride = null;
            if (!TryGetBufferViewObject(bufferViewIndex, out var bufferView))
            {
                return false;
            }
#if MESHOPT_IS_ENABLED
            var meshopt = bufferView.Extensions?.ExtMeshoptCompression;
            if (meshopt != null)
            {
                byteStride = meshopt.ByteStride;
                if (!TryGetMeshoptBufferView(bufferViewIndex, out var meshoptBuffer)
                    || byteOffset < 0
                    || byteOffset > meshoptBuffer.Length)
                {
                    return false;
                }
                data = (byte*)meshoptBuffer.GetUnsafeReadOnlyPtr() + byteOffset;
                return true;
            }
#endif
            byteStride = bufferView.ByteStride;
            if (TryResolveRange(bufferView, byteOffset, 0, out var bufferIndex, out var start)
                != BufferAccessStatus.Success)
            {
                return false;
            }
            data = (byte*)m_Buffers[bufferIndex].GetUnsafeReadOnlyPtr() + start;
            return true;
        }

        public async ValueTask<NativeArray<byte>.ReadOnly> GetBufferViewAsync(
            IBufferView bufferView,
            int offset = 0,
            int length = 0
            )
        {
            if (GltfIndex.TryGetIndex(bufferView.Buffer, m_Buffers?.Length ?? 0, out var bufferIndex))
            {
                if (!m_Buffers![bufferIndex].IsCreated && (m_BufferLoadTasks == null
                        || !m_BufferLoadTasks.TryGetValue(bufferIndex, out var download)
                        || !await download))
                {
                    return default;
                }

                return TryGetBufferView(
                    bufferView, out var data, offset, length) == BufferAccessStatus.Success
                    ? data.AsNativeArrayReadOnly()
                    : default;
            }
            return default;
        }

        /// <summary>
        /// Validates a byte range within a buffer view and resolves it to a start index into the
        /// buffer's memory.
        /// </summary>
        /// <remarks>
        /// Offsets, lengths and strides reach this point straight from JSON, un-validated, so the
        /// arithmetic is done in 64 bit. In 32 bit a hostile or broken document could overflow its
        /// way past the bounds check, and the container level checks that would catch the result
        /// are conditional on ENABLE_UNITY_COLLECTIONS_CHECKS, so they are absent in players.
        /// </remarks>
        BufferAccessStatus TryResolveRange(
            IBufferView bufferView,
            long offset,
            long byteLength,
            out int bufferIndex,
            out int start
            )
        {
            start = 0;
            var bufferViewOffset = (long)bufferView.ByteOffset;
            if (offset < 0 || byteLength < 0 || bufferViewOffset < 0)
            {
                bufferIndex = 0;
                return BufferAccessStatus.DataIndexOutOfRange;
            }
            if (!GltfIndex.TryGetIndex(bufferView.Buffer, m_Buffers?.Length ?? 0, out bufferIndex))
            {
                return BufferAccessStatus.ObjectIndexOutOfRange;
            }
            var buffer = m_Buffers![bufferIndex];
            if (!buffer.IsCreated)
            {
                return BufferAccessStatus.BufferUnavailable;
            }
            // Bound by the buffer's usable range, not by the array's length: for glTF-binary the
            // array is the entire document, of which only the binary chunk belongs to buffer 0.
            var chunk = m_BinChunks[bufferIndex];
            if (bufferViewOffset + offset + byteLength > chunk.Length)
            {
                return BufferAccessStatus.DataIndexOutOfRange;
            }
            var totalOffset = chunk.Start + bufferViewOffset + offset;
            if (totalOffset < 0 || totalOffset + byteLength > buffer.Length)
            {
                return BufferAccessStatus.DataIndexOutOfRange;
            }
            start = (int)totalOffset;
            return BufferAccessStatus.Success;
        }

        bool TryGetBufferViewObject(int bufferViewIndex, out BufferView bufferView)
        {
            return GltfIndex.TryGetElement(m_Root?.BufferViews, bufferViewIndex, out bufferView);
        }

#if MESHOPT_IS_ENABLED
        bool TryGetMeshoptBufferView(int bufferViewIndex, out NativeArray<byte> buffer)
        {
            if (m_MeshoptBufferViews != null
                && m_MeshoptBufferViews.TryGetValue(bufferViewIndex, out buffer)
                && buffer.IsCreated)
            {
                return true;
            }
            buffer = default;
            return false;
        }

        public void MeshoptDecode()
        {
            if (m_Root.BufferViews != null)
            {
                List<JobHandle> jobHandlesList = null;
                for (var i = 0; i < m_Root.BufferViews.Count; i++)
                {
                    var bufferView = m_Root.BufferViews[i];
                    if (bufferView.Extensions?.ExtMeshoptCompression != null)
                    {
                        var meshopt = bufferView.Extensions.ExtMeshoptCompression;
                        if (jobHandlesList == null)
                        {
                            m_MeshoptBufferViews = new Dictionary<int, NativeArray<byte>>();
                            jobHandlesList = new List<JobHandle>(m_Root.BufferViews.Count);
                            m_MeshoptReturnValues = new NativeArray<int>(m_Root.BufferViews.Count, Allocator.TempJob);
                        }

                        if (!meshopt.ByteStride.HasValue)
                        {
                            Logger?.Error(LogCode.TypeUnsupported, "Meshopt", "Missing byteStride");
                            continue;
                        }

                        var byteStride = meshopt.ByteStride.Value;
                        if (byteStride <= 0
                            || meshopt.Count <= 0
                            || (long)meshopt.Count * byteStride > int.MaxValue)
                        {
                            Logger?.Error(LogCode.BufferViewAccessFailed, i.ToString());
                            continue;
                        }

                        if (TryGetBufferView(meshopt, out var origBufferView) == BufferAccessStatus.Success)
                        {
                            var arr = new NativeArray<byte>(meshopt.Count * byteStride, Allocator.Persistent);
                            var jobHandle = Decode.DecodeGltfBuffer(
                                m_MeshoptReturnValues.GetSubArray(i, 1),
                                arr,
                                meshopt.Count,
                                byteStride,
                                origBufferView.AsNativeArrayReadOnly(),
                                meshopt.Mode.ToMeshoptimizerMode(),
                                meshopt.Filter.ToMeshoptimizerFilter()
                            );
                            jobHandlesList.Add(jobHandle);
                            m_MeshoptBufferViews[i] = arr;
                        }
                        else
                        {
                            Logger?.Error(LogCode.BufferViewAccessFailed, i.ToString());
                        }
                    }
                }

                if (jobHandlesList != null)
                {
                    using var jobHandles = new NativeArray<JobHandle>(jobHandlesList.ToArray(), Allocator.Temp);
                    m_MeshoptJobHandle = JobHandle.CombineDependencies(jobHandles);
                }
            }
        }

        public async Task<bool> WaitForMeshoptDecode()
        {
            var success = true;
            if (m_MeshoptBufferViews != null)
            {
                while (!m_MeshoptJobHandle.IsCompleted)
                {
                    await Task.Yield();
                }
                m_MeshoptJobHandle.Complete();

                foreach (var returnValue in m_MeshoptReturnValues)
                {
                    success &= returnValue == 0;
                }
                m_MeshoptReturnValues.Dispose();
            }
            return success;
        }
#endif // MESHOPT_IS_ENABLED

        /// <summary>
        /// Releases the buffer memory, unless leases are still open on it. In that case the
        /// release is deferred until the last of them is disposed.
        /// </summary>
        public void RequestDispose()
        {
            m_DisposeRequested = true;
            if (m_LeaseCount <= 0)
            {
                ReleaseMemory();
            }
        }

        /// <summary>
        /// Releases the buffer memory even when leases are still open on it. Reading from
        /// those leases afterward throws <see cref="ObjectDisposedException"/>.
        /// </summary>
        public void ForceDispose()
        {
            if (m_LeaseCount > 0)
            {
                Logger?.Error(LogCode.BufferDataForceDisposed, m_LeaseCount.ToString());
                m_LeaseCount = 0;
            }
            RequestDispose();
        }

        void IDisposable.Dispose() => RequestDispose();

        void ReleaseMemory()
        {
            m_DisposeRequested = false;
            m_Buffers = null;
            m_BinChunks = null;
            m_BufferLoadTasks = null;
            m_GlbBinChunk = null;

#if MESHOPT_IS_ENABLED
            m_MeshoptJobHandle.Complete();
            if (m_MeshoptBufferViews != null)
            {
                foreach (var nativeBuffer in m_MeshoptBufferViews.Values)
                {
                    nativeBuffer.Dispose();
                }
                m_MeshoptBufferViews = null;
            }
            if (m_MeshoptReturnValues.IsCreated)
            {
                m_MeshoptReturnValues.Dispose();
            }
#endif
            m_ReleaseMemoryOwners?.Invoke();
        }

        internal BufferAccessStatus ReadBufferView(
            int bufferViewIndex,
            out NativeArray<byte>.ReadOnly data,
            out int? byteStride
            )
        {
            data = default;
            byteStride = null;
            if (m_Root == null || !GltfIndex.TryGetIndex(bufferViewIndex, m_Root.BufferViews?.Count ?? 0, out _))
            {
                return BufferAccessStatus.ObjectIndexOutOfRange;
            }
            if (m_Buffers == null)
            {
                return BufferAccessStatus.BufferUnavailable;
            }
            var status = TryGetBufferView(bufferViewIndex, out var view, out byteStride);
            if (status != BufferAccessStatus.Success)
            {
                return status;
            }
            data = view.AsNativeArrayReadOnly();
            return BufferAccessStatus.Success;
        }

        BufferAccessStatus ResolveAccessor<T>(int accessorIndex, out Accessor accessor, out int bufferViewIndex)
            where T : unmanaged
        {
            bufferViewIndex = 0;
            accessor = null;
            if (m_Root == null || !GltfIndex.TryGetElement(m_Root.Accessors, accessorIndex, out accessor))
            {
                return BufferAccessStatus.ObjectIndexOutOfRange;
            }
            if (accessor.IsSparse)
            {
                return BufferAccessStatus.SparseUnsupported;
            }
            if (!GltfIndex.TryGetIndex(accessor.BufferView, m_Root.BufferViews?.Count ?? 0, out bufferViewIndex))
            {
                return BufferAccessStatus.ObjectIndexOutOfRange;
            }
            if (UnsafeUtility.SizeOf<T>() != accessor.ElementByteSize)
            {
                return BufferAccessStatus.TypeMismatch;
            }
            return m_Buffers == null ? BufferAccessStatus.BufferUnavailable : BufferAccessStatus.Success;
        }

        internal BufferAccessStatus ReadAccessorData<T>(int accessorIndex, out NativeArray<T>.ReadOnly data)
            where T : unmanaged
        {
            data = default;
            var status = ResolveAccessor<T>(accessorIndex, out var accessor, out var bufferViewIndex);
            if (status != BufferAccessStatus.Success)
            {
                return status;
            }

            status = TryGetAccessorData<T>(bufferViewIndex, accessor.Count, out var view, accessor.ByteOffset);
            if (status != BufferAccessStatus.Success)
            {
                return status;
            }
            data = view.AsNativeArrayReadOnly();
            return BufferAccessStatus.Success;
        }

        internal BufferAccessStatus ReadStridedAccessorData<T>(
            int accessorIndex,
            out ReadOnlyNativeStridedArray<T> data
            )
            where T : unmanaged
        {
            data = default;
            var status = ResolveAccessor<T>(accessorIndex, out var accessor, out var bufferViewIndex);
            return status != BufferAccessStatus.Success
                ? status
                : TryGetStridedAccessorData(bufferViewIndex, accessor.Count, out data, accessor.ByteOffset);
        }
    }
}
