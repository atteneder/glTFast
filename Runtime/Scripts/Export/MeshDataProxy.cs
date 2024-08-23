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
    class MeshDataProxy<TIndex> : IMeshData<TIndex> where TIndex : struct
    {
        Mesh.MeshData m_MeshData;

        public MeshDataProxy(Mesh.MeshData meshData)
        {
            m_MeshData = meshData;
        }

        public int subMeshCount => m_MeshData.subMeshCount;

        public MeshTopology GetTopology(int subMesh)
        {
            return m_MeshData.GetSubMesh(subMesh).topology;
        }

        public int GetIndexCount(int subMesh)
        {
            return m_MeshData.GetSubMesh(subMesh).indexCount;
        }

#if ASYNC_MESH_DATA
        public Task<NativeArray<TIndex>> GetIndexData()
        {
            return Task.FromResult(m_MeshData.GetIndexData<TIndex>());
        }

        public Task<NativeArray<byte>> GetVertexData(int stream)
        {
            return Task.FromResult(m_MeshData.GetVertexData<byte>(stream));
        }
#else
        public NativeArray<TIndex> GetIndexData()
        {
            return m_MeshData.GetIndexData<TIndex>();
        }

        public NativeArray<byte> GetVertexData(int stream)
        {
            return m_MeshData.GetVertexData<byte>(stream);
        }
#endif


    }
}
