// SPDX-FileCopyrightText: 2026 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.Gltfast.Addons;
using Unity.Cloud.Gltfast.Objects;
using UnityEngine;

namespace Unity.Cloud.Gltfast.Documentation.Examples
{
    #region PositionSumAddon
    /// <summary>
    /// Sums up every vertex position of a glTF asset, straight from its buffers.
    /// </summary>
    class PositionSumAddon : ImportAddonInstance, IBufferDataConsumer
    {
        GltfImport m_GltfImport;

        public Vector3 Sum { get; private set; }

        public Task<bool> ConsumeBufferDataAsync(IGltfBufferData bufferData, CancellationToken cancellationToken)
        {
            var root = m_GltfImport.Root;
            if (root.Accessors == null)
            {
                return Task.FromResult(true);
            }

            var sum = Vector3.zero;
            for (var accessorIndex = 0; accessorIndex < root.Accessors.Count; accessorIndex++)
            {
                var accessor = root.Accessors[accessorIndex];

                // The accessor describes the data; the buffer data provides it. Only ask for a type
                // that matches, otherwise the request reports a TypeMismatch.
                if (accessor.ComponentType != AccessorDataType.Float
                    || accessor.Type != AccessorType.Vector3)
                {
                    continue;
                }

                // Vertex data is usually interleaved, so ask for a strided view. It serves tightly
                // packed data just as well, whereas GetAccessorData reports StridedUnsupported for
                // anything interleaved.
                var status = bufferData.GetStridedAccessorData<Vector3>(accessorIndex, out var values);
                if (status != BufferAccessStatus.Success)
                {
                    // For example SparseUnsupported, or IndexOutOfRange for a malformed asset.
                    Debug.LogWarning($"Accessor {accessorIndex} is unavailable: {status}");
                    continue;
                }

                for (var i = 0; i < values.Length; i++)
                {
                    // Values are in glTF's coordinate system. No conversion was applied.
                    sum += values[i];
                }
            }

            Sum = sum;

            // Returning false here would abort the import.
            return Task.FromResult(true);
        }

        public override void Inject(GltfImport gltfImport)
        {
            m_GltfImport = gltfImport;
            gltfImport.AddImportAddonInstance(this);
        }

        public override bool SupportsGltfExtension(string extensionName) => false;
        public override void Inject(IInstantiator instantiator) { }
        public override void Dispose() { }
    }
    #endregion

    static class BufferDataAccess
    {
        #region ReadBufferDataDuringImport
        public static async Task<Vector3> SumPositionsAsync(string filePath)
        {
            using var gltf = new GltfImport();

            // Buffer data only exists while the import is running, so read it from an add-on.
            var addon = new PositionSumAddon();
            addon.Inject(gltf);

            return await gltf.LoadAsync(filePath)
                ? addon.Sum
                : Vector3.zero;
        }
        #endregion

        #region RetainBufferDataBeyondImport
        /// <summary>
        /// Keeps a glTF asset's buffer data readable after the import completed, by holding a
        /// lease of its own.
        /// </summary>
        class BufferRetainingAddon : ImportAddonInstance, IBufferDataConsumer
        {
            GltfImport m_GltfImport;

            public IGltfBufferData Lease { get; private set; }

            public Task<bool> ConsumeBufferDataAsync(
                IGltfBufferData bufferData,
                CancellationToken cancellationToken
                )
            {
                // The lease passed in is disposed once this returns. Leasing another one keeps
                // the buffer memory alive until that one is disposed.
                Lease = m_GltfImport.LeaseBufferData();
                return Task.FromResult(true);
            }

            public override void Inject(GltfImport gltfImport)
            {
                m_GltfImport = gltfImport;
                gltfImport.AddImportAddonInstance(this);
            }

            public override bool SupportsGltfExtension(string extensionName) => false;
            public override void Inject(IInstantiator instantiator) { }
            public override void Dispose() => Lease?.Dispose();
        }

        public static async Task<int> BufferViewSizeAfterImportAsync(string filePath, int bufferViewIndex)
        {
            using var gltf = new GltfImport();
            var addon = new BufferRetainingAddon();
            addon.Inject(gltf);

            if (!await gltf.LoadAsync(filePath))
            {
                return 0;
            }

            // The import is done, but the retained lease still provides the data. Without it,
            // this would report BufferUnavailable.
            using var bufferData = addon.Lease;
            return bufferData.GetBufferView(bufferViewIndex, out var data, out _) == BufferAccessStatus.Success
                ? data.Length
                : 0;
        }
        #endregion
    }
}
