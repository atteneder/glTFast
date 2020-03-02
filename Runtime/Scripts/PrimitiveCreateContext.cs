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
using Unity.Jobs;
using System.Runtime.InteropServices;
using UnityEngine.Profiling;

namespace GLTFast {

    using Schema;

    class PrimitiveCreateContext : PrimitiveCreateContextBase {

        public Mesh mesh;

        /// TODO remove begin
        public Vector3[] positions;
        public Vector3[] normals;
        public Vector2[] uvs0;
        public Vector2[] uvs1;
        public Vector4[] tangents;
        public Color32[] colors32;
        public Color[] colors;
        /// TODO remove end

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
            Profiler.BeginSample("Job Complete");
            jobHandle.Complete();
            Profiler.EndSample();
            var msh = new UnityEngine.Mesh();
            if( positions.Length > 65536 ) {
#if UNITY_2017_3_OR_NEWER
                msh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
                throw new System.Exception("Meshes with more than 65536 vertices are only supported from Unity 2017.3 onwards.");
#endif
            }
            msh.name = mesh.name;
            Profiler.BeginSample("SetVertices");
            msh.vertices = positions;
            Profiler.EndSample();

            Profiler.BeginSample("SetIndices");
            msh.subMeshCount = indices.Length;
            for (int i = 0; i < indices.Length; i++) {
                msh.SetIndices(indices[i],topology,i);
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetUVs");
            if(uvs0!=null) {
                msh.uv = uvs0;
            }
            if(uvs1!=null) {
                msh.uv2 = uvs1;
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetNormals");
            if(normals!=null) {
                msh.normals = normals;
            } else
            if( needsNormals && ( topology==MeshTopology.Triangles || topology==MeshTopology.Quads ) ) {
                Profiler.BeginSample("RecalculateNormals");
                msh.RecalculateNormals();
                Profiler.EndSample();
            }
            Profiler.EndSample();
            Profiler.BeginSample("SetColor");
            if (colors!=null) {
                msh.colors = colors;
            } else if(colors32!=null) {
                msh.colors32 = colors32;
            }
            Profiler.EndSample();
            Profiler.BeginSample("SetTangents");
            if(tangents!=null) {
                msh.tangents = tangents;
            } else
            if( needsTangents && uvs0!=null && (topology==MeshTopology.Triangles || topology==MeshTopology.Quads) ) {
                Profiler.BeginSample("RecalculateTangents");
                msh.RecalculateTangents();
                Profiler.EndSample();
            }
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