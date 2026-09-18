// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Unity.Cloud.Gltfast.Export
{
    interface IMeshData
    {
        int subMeshCount { get; }

        MeshTopology GetTopology(int subMesh);
        int GetIndexCount(int subMesh);

        ValueTask<NativeArray<byte>> GetVertexDataAsync(int stream, bool sync);
    }

    interface IMeshData<TIndex> : IMeshData where TIndex : unmanaged
    {
        ValueTask<NativeArray<TIndex>> GetIndexDataAsync(bool sync);
    }
}
