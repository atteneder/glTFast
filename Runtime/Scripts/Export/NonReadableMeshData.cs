// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace Unity.Cloud.Gltfast.Export
{
    class NonReadableMeshData<TIndex> : IMeshData<TIndex> where TIndex : unmanaged
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


        public async ValueTask<NativeArray<TIndex>> GetIndexDataAsync(bool sync)
        {
            if (!m_IndexData.IsCreated)
            {
                using var indexBuffer = m_Mesh.GetIndexBuffer();
                m_IndexData = new NativeArray<TIndex>(indexBuffer.count, Allocator.Persistent);
                AsyncGPUReadbackRequest request;
                if (!sync)
                {
                    request = await AsyncGPUReadback.RequestIntoNativeArrayAsync(ref m_IndexData, indexBuffer);
                }
                else
                {
                    request = AsyncGPUReadback.RequestIntoNativeArray(ref m_IndexData, indexBuffer);
                    request.WaitForCompletion();
                }
                Assert.IsTrue(request.done);
                Assert.IsFalse(request.hasError);
            }
            return m_IndexData;
        }

        public async ValueTask<NativeArray<byte>> GetVertexDataAsync(int stream, bool sync)
        {
            Assert.IsTrue(stream >= 0 && stream < 4, "stream must in range 0 to 3");
            m_VertexData ??= new NativeArray<byte>[4];
            if (!m_VertexData[stream].IsCreated)
            {
                using var vertexBuffer = m_Mesh.GetVertexBuffer(stream);
                m_VertexData[stream] = new NativeArray<byte>(vertexBuffer.count * vertexBuffer.stride, Allocator.Persistent);
                AsyncGPUReadbackRequest request;
                if (!sync)
                {
                    request = await AsyncGPUReadback.RequestIntoNativeArrayAsync(ref m_VertexData[stream], vertexBuffer);
                }
                else
                {
                    request = AsyncGPUReadback.RequestIntoNativeArray(ref m_VertexData[stream], vertexBuffer);
                    request.WaitForCompletion();
                }
                Assert.IsTrue(request.done);
                Assert.IsFalse(request.hasError);
            }
            return m_VertexData[stream];
        }
    }
}
