// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0


#if UNITY_2023_3_OR_NEWER
#define ASYNC_MESH_DATA
#endif

using System;
#if ASYNC_MESH_DATA
using System.Threading.Tasks;
#endif
using Unity.Collections;
using UnityEngine;

namespace GLTFast.Export
{
    interface IMeshData
    {
        int subMeshCount { get; }

        MeshTopology GetTopology(int subMesh);
        int GetIndexCount(int subMesh);

#if ASYNC_MESH_DATA
        Task<NativeArray<byte>> GetVertexData(int stream);
#else
        NativeArray<byte> GetVertexData(int stream);
#endif
    }

    interface IMeshData<TIndex> : IMeshData where TIndex : struct
    {
#if ASYNC_MESH_DATA
        Task<NativeArray<TIndex>> GetIndexData();
#else
        NativeArray<TIndex> GetIndexData();
#endif
    }
}
