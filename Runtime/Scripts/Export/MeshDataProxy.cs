// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Unity.Cloud.Gltfast.Export
{
    class MeshDataProxy<TIndex> : IMeshData<TIndex> where TIndex : unmanaged
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

        public ValueTask<NativeArray<TIndex>> GetIndexDataAsync(bool sync)
        {
            return new ValueTask<NativeArray<TIndex>>(m_MeshData.GetIndexData<TIndex>());
        }

        public ValueTask<NativeArray<byte>> GetVertexDataAsync(int stream, bool sync)
        {
            return new ValueTask<NativeArray<byte>>(m_MeshData.GetVertexData<byte>(stream));
        }
    }
}
