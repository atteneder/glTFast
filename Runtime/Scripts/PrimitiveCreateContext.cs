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

using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;
using System.Threading.Tasks;

namespace GLTFast {

    using Schema;

    class PrimitiveCreateContext : PrimitiveCreateContextBase {

        public Mesh mesh;
        public VertexBufferConfigBase vertexData;

        public JobHandle jobHandle;
        public int[][] indices;

        public GCHandle calculatedIndicesHandle;

        public MeshTopology topology;

        public override bool IsCompleted => jobHandle.IsCompleted;

        public override async Task<Primitive?> CreatePrimitive() {
            Profiler.BeginSample("CreatePrimitive");
            jobHandle.Complete();
            var msh = new UnityEngine.Mesh();
            msh.name = mesh.name;

            vertexData.ApplyOnMesh(msh,defaultMeshUpdateFlags);

            Profiler.BeginSample("SetIndices");
            int indexCount = 0;
            var allBounds = vertexData.bounds;
            for (int i = 0; i < indices.Length; i++) {
                indexCount += indices[i].Length;
            }
            Profiler.BeginSample("SetIndexBufferParams");
            msh.SetIndexBufferParams(indexCount,IndexFormat.UInt32); //TODO: UInt16 maybe?
            Profiler.EndSample();
            msh.subMeshCount = indices.Length;
            indexCount = 0;
            for (int i = 0; i < indices.Length; i++) {
                Profiler.BeginSample("SetIndexBufferData");
                msh.SetIndexBufferData(indices[i],0,indexCount,indices[i].Length,defaultMeshUpdateFlags);
                Profiler.EndSample();
                Profiler.BeginSample("SetSubMesh");
                var subMeshDescriptor = new SubMeshDescriptor{
                    indexStart = indexCount,
                    indexCount = indices[i].Length,
                    topology = topology,
                    baseVertex = 0,
                    firstVertex = 0,
                    vertexCount = vertexData.vertexCount
                };
                if (allBounds.HasValue) {
                    // Setting the submeshes' bounds to the overall bounds
                    // Calculating the actual sub-mesh bounds (by iterating the verts referenced
                    // by the sub-mesh indices) would be slow. Also, hardly any glTFs re-use
                    // the same vertex buffer across primitives of a node (which is the
                    // only way a mesh can have sub-meshes)
                    subMeshDescriptor.bounds = allBounds.Value;
                }
                msh.SetSubMesh(i,subMeshDescriptor,defaultMeshUpdateFlags);
                Profiler.EndSample();
                indexCount += indices[i].Length;
            }
            Profiler.EndSample();

            if(vertexData.calculateNormals) {
                Profiler.BeginSample("RecalculateNormals");
                msh.RecalculateNormals();
                Profiler.EndSample();
            }
            if(vertexData.calculateTangents) {
                Profiler.BeginSample("RecalculateTangents");
                msh.RecalculateTangents();
                Profiler.EndSample();
            }
            
            if (allBounds.HasValue) {
                msh.bounds = allBounds.Value;
            } else {
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
            /// Don't upload explicitely. Unity takes care of upload on demand/deferred

            // Profiler.BeginSample("UploadMeshData");
            // msh.UploadMeshData(true);
            // Profiler.EndSample();
#endif

            if (morphTargetsContext != null) {
                await morphTargetsContext.ApplyOnMeshAndDispose(msh);
            }

            Profiler.BeginSample("Dispose");
            Dispose();
            Profiler.EndSample();

            Profiler.EndSample();

            return new Primitive(msh,materials);
        }
        
        void Dispose() {
            if(calculatedIndicesHandle.IsAllocated) {
                calculatedIndicesHandle.Free();
            }
        }
    }
}
