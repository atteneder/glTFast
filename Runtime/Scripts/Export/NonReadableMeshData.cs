// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

#if UNITY_2021_3_OR_NEWER
#if UNITY_2023_3_OR_NEWER
#define ASYNC_MESH_DATA
#endif

using System;
#if ASYNC_MESH_DATA
using System.Threading.Tasks;
#endif
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace GLTFast.Export
{
    class NonReadableMeshData<TIndex> : IMeshData<TIndex> where TIndex : struct
    {
        Mesh m_Mesh;

        NativeArray<TIndex> m_IndexData;
        NativeArray<byte>[] m_VertexData;

        public NonReadableMeshData(Mesh mesh)
        {
            m_Mesh = mesh;
        }

        public int subMeshCount => m_Mesh.subMeshCount;

        public MeshTopology GetTopology(int subMesh)
        {
            return m_Mesh.GetTopology(subMesh);
        }

        public int GetIndexCount(int subMesh)
        {
            return (int)m_Mesh.GetIndexCount(subMesh);
        }


#if ASYNC_MESH_DATA
        public async Task<NativeArray<TIndex>> GetIndexData()
#else
        public NativeArray<TIndex> GetIndexData()
#endif
        {
            if (!m_IndexData.IsCreated)
            {
                using var indexBuffer = m_Mesh.GetIndexBuffer();
                m_IndexData = new NativeArray<TIndex>(indexBuffer.count, Allocator.Persistent);
#if ASYNC_MESH_DATA
                var request = await AsyncGPUReadback.RequestIntoNativeArrayAsync(ref m_IndexData, indexBuffer);
#else
                var request = AsyncGPUReadback.RequestIntoNativeArray(ref m_IndexData, indexBuffer);
                request.WaitForCompletion();
#endif
                Assert.IsTrue(request.done);
                Assert.IsFalse(request.hasError);
            }
            return m_IndexData;
        }

#if ASYNC_MESH_DATA
        public async Task<NativeArray<byte>> GetVertexData(int stream)
#else
        public NativeArray<byte> GetVertexData(int stream)
#endif
        {
            Assert.IsTrue(stream >= 0 && stream < 4, "stream must in range 0 to 3");
            m_VertexData ??= new NativeArray<byte>[4];
            if (!m_VertexData[stream].IsCreated)
            {
                using var vertexBuffer = m_Mesh.GetVertexBuffer(stream);
                m_VertexData[stream] = new NativeArray<byte>(vertexBuffer.count * vertexBuffer.stride, Allocator.Persistent);
#if ASYNC_MESH_DATA
                var request = await AsyncGPUReadback.RequestIntoNativeArrayAsync(ref m_VertexData[stream], vertexBuffer);
#else
                var request = AsyncGPUReadback.RequestIntoNativeArray(ref m_VertexData[stream], vertexBuffer);
                request.WaitForCompletion();
#endif
                Assert.IsTrue(request.done);
                Assert.IsFalse(request.hasError);
            }
            return m_VertexData[stream];
        }
    }
}
#endif // UNITY_2021_2_OR_NEWER
