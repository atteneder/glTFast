// Copyright 2020 Andreas Atteneder
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
#if !UNITY_2019_3_OR_NEWER
#define LEGACY_MESH
#endif

using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;

namespace GLTFast {

    using Schema;

    class PrimitiveCreateContext : PrimitiveCreateContextBase {

        public Mesh mesh;
        public VertexBufferConfigBase vertexData;

        public JobHandle jobHandle;
        public int[][] indices;

        public GCHandle calculatedIndicesHandle;

        public MeshTopology topology;

        public override bool IsCompleted {
            get {
                return jobHandle.IsCompleted;
            }  
        }

        public override Primitive? CreatePrimitive() {
            Profiler.BeginSample("CreatePrimitive");
            jobHandle.Complete();
            var msh = new UnityEngine.Mesh();
            msh.name = mesh.name;

            MeshUpdateFlags flags = MeshUpdateFlags.Default;// (MeshUpdateFlags)~0;
            vertexData.ApplyOnMesh(msh,flags);

            Profiler.BeginSample("SetIndices");
            int indexCount = 0;
            for (int i = 0; i < indices.Length; i++) {
                indexCount += indices[i].Length;
            }
            msh.SetIndexBufferParams(indexCount,IndexFormat.UInt32); //TODO: UInt16 maybe?
            msh.subMeshCount = indices.Length;
            indexCount = 0;
            for (int i = 0; i < indices.Length; i++) {
                Profiler.BeginSample("SetIndexBufferData");
                msh.SetIndexBufferData(indices[i],0,indexCount,indices[i].Length,flags);
                Profiler.EndSample();
                Profiler.BeginSample("SetSubMesh");
                msh.SetSubMesh(i,new SubMeshDescriptor(indexCount,indices[i].Length,topology),flags);
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
            
            Profiler.BeginSample("RecalculateBounds");
            msh.RecalculateBounds(); // TODO: make optional! maybe calculate bounds in Job.
            Profiler.EndSample();

            Profiler.BeginSample("UploadMeshData");
            msh.UploadMeshData(true);
            Profiler.EndSample();

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
