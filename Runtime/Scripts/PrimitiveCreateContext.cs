// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Unity.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine;

namespace GLTFast
{

    class PrimitiveCreateContext : PrimitiveCreateContextBase
    {

        public VertexBufferConfigBase vertexData;

        public JobHandle jobHandle;
        int[][] m_Indices;

        public GCHandle calculatedIndicesHandle;

        public MeshTopology topology;

        public PrimitiveCreateContext(int primitiveIndex, int materialCount, string meshName)
            : base(primitiveIndex, materialCount, meshName)
        {
            m_Indices = new int[materialCount][];
        }

        public void SetIndices(int subMesh, int[] indices)
        {
            m_Indices[subMesh] = indices;
        }

        public override bool IsCompleted => jobHandle.IsCompleted;

        public override async Task<Primitive?> CreatePrimitive()
        {
            Profiler.BeginSample("CreatePrimitive");
            jobHandle.Complete();
            var msh = new Mesh
            {
                name = m_MeshName
            };

            vertexData.ApplyOnMesh(msh);

            Profiler.BeginSample("SetIndices");
            var indexCount = 0;
            var allBounds = vertexData.Bounds;
            for (var i = 0; i < m_Indices.Length; i++)
            {
                indexCount += m_Indices[i].Length;
            }
            Profiler.BeginSample("SetIndexBufferParams");
            msh.SetIndexBufferParams(indexCount, IndexFormat.UInt32); //TODO: UInt16 maybe?
            Profiler.EndSample();
            msh.subMeshCount = m_Indices.Length;
            indexCount = 0;
            for (var i = 0; i < m_Indices.Length; i++)
            {
                Profiler.BeginSample("SetIndexBufferData");
                msh.SetIndexBufferData(m_Indices[i], 0, indexCount, m_Indices[i].Length, defaultMeshUpdateFlags);
                Profiler.EndSample();
                Profiler.BeginSample("SetSubMesh");
                var subMeshDescriptor = new SubMeshDescriptor
                {
                    indexStart = indexCount,
                    indexCount = m_Indices[i].Length,
                    topology = topology,
                    baseVertex = 0,
                    firstVertex = 0,
                    vertexCount = vertexData.VertexCount
                };
                if (allBounds.HasValue)
                {
                    // Setting the sub-meshes' bounds to the overall bounds
                    // Calculating the actual sub-mesh bounds (by iterating the verts referenced
                    // by the sub-mesh indices) would be slow. Also, hardly any glTFs re-use
                    // the same vertex buffer across primitives of a node (which is the
                    // only way a mesh can have sub-meshes)
                    subMeshDescriptor.bounds = allBounds.Value;
                }
                msh.SetSubMesh(i, subMeshDescriptor, defaultMeshUpdateFlags);
                Profiler.EndSample();
                indexCount += m_Indices[i].Length;
            }
            Profiler.EndSample();

            if (vertexData.calculateNormals)
            {
                Profiler.BeginSample("RecalculateNormals");
                msh.RecalculateNormals();
                Profiler.EndSample();
            }
            if (vertexData.calculateTangents)
            {
                Profiler.BeginSample("RecalculateTangents");
                msh.RecalculateTangents();
                Profiler.EndSample();
            }

            if (allBounds.HasValue)
            {
                msh.bounds = allBounds.Value;
            }
            else
            {
                Profiler.BeginSample("RecalculateBounds");
#if DEBUG
                Debug.LogError("Bounds have to be recalculated (slow operation). Check if position accessors have proper min/max values");
#endif
                msh.RecalculateBounds();
                Profiler.EndSample();
            }

#if GLTFAST_KEEP_MESH_DATA
            Profiler.BeginSample("UploadMeshData");
            msh.UploadMeshData(false);
            Profiler.EndSample();
#else
            // Don't upload explicitly. Unity takes care of upload on demand/deferred

            // Profiler.BeginSample("UploadMeshData");
            // msh.UploadMeshData(true);
            // Profiler.EndSample();
#endif

            if (morphTargetsContext != null)
            {
                await morphTargetsContext.ApplyOnMeshAndDispose(msh);
            }

            Profiler.BeginSample("Dispose");
            Dispose();
            Profiler.EndSample();

            Profiler.EndSample();

            return new Primitive(msh, m_Materials);
        }

        void Dispose()
        {
            if (calculatedIndicesHandle.IsAllocated)
            {
                calculatedIndicesHandle.Free();
            }
        }
    }
}
